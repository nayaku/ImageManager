using ImageManager.Data.Model;
using Joveler.Compression.XZ;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using Label = ImageManager.Data.Model.Label;

namespace ImageManager.Data
{
    /// <summary>
    /// Picture数据的外部存档
    /// </summary>
    /// <remarks>
    /// 文件结构分为头部和压缩数据两部分
    /// Head结构如下
    /// Magic: 4 bytes "PDAF"
    /// Version: 1 byte, UInt8, 版本号
    /// Length: 8 bytes, UInt64, 压缩数据的长度
    /// </remarks>
    public class PictureDataArchive
    {
        public static readonly string Magic = "PDAF";
        public static readonly string Extension = ".pdaf";

        private UserSettingData _userSettingData;
        private ImageContext _context;
        private string _filePath;
        private static bool _isInitialized = false;

        public List<Picture> Pictures { get; set; }
        public EventHandler<ProgressEventArgs> ProgressChanged { get; set; }

        public PictureDataArchive(UserSettingData userSettingData, ImageContext context, string filePath)
        {
            _userSettingData = userSettingData;
            _context = context;
            _filePath = filePath;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Save(CancellationToken cancellationToken)
        {
            ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 0, Message = "正在初始化……" });
            InitNativeLibrary();

            // 计算大小
            ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 0, Message = "正在计算总大小……" });
            var pictureStoreInfos = new List<PictureStoreInfo>();
            var totalByte = 0uL;
            var fileInfos = new List<FileInfo>();
            foreach (var picture in Pictures)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var fileInfo = new FileInfo(Path.Join(picture.ImageFolderPath, picture.Path));
                totalByte += (ulong)fileInfo.Length;
                fileInfos.Add(fileInfo);
                var pictureStoreInfo = new PictureStoreInfo()
                {
                    Title = picture.Title,
                    Labels = picture.Labels.Select(l => l.Name).ToList(),
                    Extension = fileInfo.Extension,
                    Length = (uint)fileInfo.Length,
                };
                pictureStoreInfos.Add(pictureStoreInfo);
            }

            // 转存数据库
            if (cancellationToken.IsCancellationRequested)
                return;
            using var pictureStoreInfosStream = new MemoryStream();
            var xml = new XmlSerializer(typeof(List<PictureStoreInfo>));
            xml.Serialize(pictureStoreInfosStream, pictureStoreInfos);
            pictureStoreInfosStream.Seek(0, SeekOrigin.Begin);
            Debug.WriteLine(new StreamReader(pictureStoreInfosStream).ReadToEnd());
            totalByte += (ulong)pictureStoreInfosStream.Length;

            // 准备压缩
            ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 0, Message = "正在创建压缩包……" });
            using var fs = new FileStream(_filePath, FileMode.Create);
            // 写入头部
            fs.Write(Encoding.UTF8.GetBytes(Magic));
            fs.WriteByte(1);
            fs.Write(BitConverter.GetBytes((ulong)0)); // 预留 文件长度
            // 头部
            var headLength = fs.Position;

            fs.Flush();

            // 写入压缩数据
            var compressOptions = new XZCompressOptions()
            {
                Level = LzmaCompLevel.Level9,
                ExtremeFlag = true,
                LeaveOpen = true,
            };
            var threadCompressOptions = new XZThreadedCompressOptions
            {
                // Thread equal as CPU core count
                Threads = Environment.ProcessorCount,
            };
            using var xzStream = new XZStream(fs, compressOptions, threadCompressOptions);
            var fin = false;
            Task.Run(() =>
            {
                ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 0, Message = "正在压缩……" });
                while (!fin)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    ulong progressIn;
                    lock (xzStream)
                        xzStream.GetProgress(out progressIn, out ulong progressOut);
                    ProgressChanged?.Invoke(this, new ProgressEventArgs
                    {
                        Progress = 100.0 * progressIn / totalByte,
                        Message = "正在压缩……"
                    });
                    Task.Delay(200).Wait();
                }
                ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 100, Message = "压缩完成！" });
            }, cancellationToken);

            // 写入数据库
            // Length: 4 bytes, uint32, 数据库的长度
            if (cancellationToken.IsCancellationRequested)
            {
                fin = true;
                lock (xzStream)
                    xzStream.Abort();
                return;
            }
            xzStream.Write(BitConverter.GetBytes((uint)pictureStoreInfosStream.Length));
            pictureStoreInfosStream.Seek(0, SeekOrigin.Begin);
            pictureStoreInfosStream.CopyTo(xzStream);
            xzStream.Flush();

            // 写入图片
            foreach (var fileinfo in fileInfos)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    fin = true;
                    lock (xzStream)
                        xzStream.Abort();
                    return;
                }
                using var fileStream = fileinfo.OpenRead();
                fileStream.CopyTo(xzStream);
                fileStream.Close();
                xzStream.Flush();
            }

            xzStream.Flush();
            fin = true;

            // 写入文件长度
            lock (xzStream)
                xzStream.Close();
            var blockLength = (ulong)(fs.Length - headLength);
            fs.Seek(headLength - 8, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes(blockLength));
            return;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="PictureDataArchiveFileFormatNotSupportException"></exception>
        /// <exception cref="PictureDataArchiveVersionNotSupportException"></exception>
        /// <exception cref="PictureDataArchiveFileDamagedException"></exception>
        public void Load(CancellationToken cancellationToken, ReadOnlyCollection<Picture> tempPictures)
        {
            ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 0, Message = "正在初始化……" });
            InitNativeLibrary();

            // 打开文件
            using var fs = new FileStream(_filePath, FileMode.Open);
            // 读取头部
            var magic = new byte[4];
            fs.Read(magic);
            if (Encoding.UTF8.GetString(magic) != Magic)
                throw new PictureDataArchiveFileFormatNotSupportException("文件格式不支持！");
            var version = fs.ReadByte();
            if (version != 1)
                throw new PictureDataArchiveVersionNotSupportException("不支持该文件版本！");
            byte[] buf;
            // 验证文件长度
            buf = new byte[8];
            fs.Read(buf);
            var length = BitConverter.ToUInt64(buf);
            if ((ulong)fs.Length != length + (ulong)fs.Position)
                throw new PictureDataArchiveFileDamagedException("文件损坏！");
            // 解压缩
            var decompressOptions = new XZDecompressOptions();
            using var xzStream = new XZStream(fs, decompressOptions);
            // 读取数据库
            buf = new byte[4];
            xzStream.Read(buf);
            var infoLength = BitConverter.ToUInt32(buf);
            buf = new byte[infoLength];
            xzStream.Read(buf);

            using var storeInfosStream = new MemoryStream(buf);
            Debug.WriteLine(new StreamReader(storeInfosStream).ReadToEnd());
            storeInfosStream.Seek(0, SeekOrigin.Begin);
            var xmlSerializer = new XmlSerializer(typeof(List<PictureStoreInfo>));
            var pictureStoreInfos = (List<PictureStoreInfo>)xmlSerializer.Deserialize(storeInfosStream);
            // 读取图片
            Pictures = new();
            for (var pictureStoreInfoIndex = 0; pictureStoreInfoIndex < pictureStoreInfos.Count; pictureStoreInfoIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                var pictureStoreInfo = pictureStoreInfos[pictureStoreInfoIndex];
                ProgressChanged?.Invoke(this, new ProgressEventArgs
                {
                    Progress = 100.0 * pictureStoreInfoIndex / pictureStoreInfos.Count,
                    Message = "正在解压缩……"
                });

                var fileInfo = new FileInfo(Path.Join(_userSettingData.TempFolderPath,
                    "Decompress" + pictureStoreInfo.Extension));
                using (var fileStream = fileInfo.OpenWrite())
                {
                    buf = new byte[1024 * 1024];// 1MB
                    var fileByte = pictureStoreInfo.Length;
                    while (fileByte > 0)
                    {
                        var readByte = xzStream.Read(buf, 0, (int)Math.Min(fileByte, (ulong)buf.Length));
                        fileStream.Write(buf, 0, readByte);
                        fileByte -= (uint)readByte;
                    }
                }
                var pictureFactory = new PictureFactory(_userSettingData, _context);
                var picture = pictureFactory.CreateTempPicture(fileInfo.FullName, tempPictures, true);
                picture.Title = pictureStoreInfo.Title;
                foreach (var labelName in pictureStoreInfo.Labels)
                {
                    var label = _context.Labels.FirstOrDefault(l => l.Name == labelName);
                    label ??= _context.Labels.Local.FirstOrDefault(l => l.Name == labelName);
                    if (label == null)
                    {
                        label = new Label
                        {
                            Name = labelName,
                        };
                        _context.Labels.Add(label);
                    }
                    picture.Labels.Add(label);
                }
                Pictures.Add(picture);
            }
            if (cancellationToken.IsCancellationRequested)
                return;
            ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = 100, Message = "解压缩完成！" });
            _context.SaveChanges();
        }

        public static bool IsPictureDataArchiveFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
            if (Path.GetExtension(filePath) != Extension)
                return false;
            using var fs = new FileStream(filePath, FileMode.Open);
            var magic = new byte[4];
            fs.Read(magic);
            return Encoding.UTF8.GetString(magic) == Magic;
        }

        private static void InitNativeLibrary()
        {
            if (_isInitialized)
                return;
            string libDir = "runtimes";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libDir = Path.Combine(libDir, "win-");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libDir = Path.Combine(libDir, "linux-");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libDir = Path.Combine(libDir, "osx-");

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    libDir += "x86";
                    break;
                case Architecture.X64:
                    libDir += "x64";
                    break;
                case Architecture.Arm:
                    libDir += "arm";
                    break;
                case Architecture.Arm64:
                    libDir += "arm64";
                    break;
            }
            libDir = Path.Combine(libDir, "native");

            string libPath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libPath = Path.Combine(libDir, "liblzma.dll");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libPath = Path.Combine(libDir, "liblzma.so");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libPath = Path.Combine(libDir, "liblzma.dylib");

            if (libPath == null)
                throw new PlatformNotSupportedException($"Unable to find native library.");
            if (!File.Exists(libPath))
                throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

            XZInit.GlobalInit(libPath);
            _isInitialized = true;
        }

        public class PictureStoreInfo
        {
            public string Title { get; set; }
            public List<string> Labels { get; set; }
            /// <summary>
            /// 图片后缀
            /// </summary>
            public string Extension { get; set; }
            public uint Length { get; set; }
        }

    }

    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 进度，0-100
        /// </summary>
        public double Progress { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
    /// <summary>
    /// 文件格式错误
    /// </summary>
    public class PictureDataArchiveFileFormatNotSupportException : Exception
    {
        public PictureDataArchiveFileFormatNotSupportException() { }
        public PictureDataArchiveFileFormatNotSupportException(string message) : base(message) { }
        public PictureDataArchiveFileFormatNotSupportException(string message, Exception innerException) :
            base(message, innerException)
        { }

    }

    /// <summary>
    /// 不支持的文件版本
    /// </summary>
    public class PictureDataArchiveVersionNotSupportException : PictureDataArchiveFileFormatNotSupportException
    {
        public PictureDataArchiveVersionNotSupportException() { }
        public PictureDataArchiveVersionNotSupportException(string message) : base(message) { }
        public PictureDataArchiveVersionNotSupportException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }

    /// <summary>
    /// 文件损坏
    /// </summary>
    public class PictureDataArchiveFileDamagedException : PictureDataArchiveFileFormatNotSupportException
    {
        public PictureDataArchiveFileDamagedException() { }
        public PictureDataArchiveFileDamagedException(string message) : base(message) { }
        public PictureDataArchiveFileDamagedException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }

}
