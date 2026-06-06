using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using ComTypes = System.Runtime.InteropServices.ComTypes;

static class VirtualFileHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct FILEDESCRIPTOR
    {
        public uint dwFlags;
        public Guid clsid;
        public System.Drawing.Size sizel;
        public System.Drawing.Point pointl;
        public uint dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
    }

    public static string[] SaveVirtualFileToDisk(System.Windows.IDataObject data)
    {
        // 尝试把 WPF IDataObject 转为 COM IDataObject（多数情况下可行）
        if (data is not ComTypes.IDataObject comData)
            return [];

        try
        {
            // 1) 读 FILEGROUPDESCRIPTORW（包含文件名）
            FORMATETC fmt = new()
            {
                cfFormat = (short)DataFormats.GetDataFormat("FileGroupDescriptorW").Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero,
                tymed = TYMED.TYMED_HGLOBAL
            };

            STGMEDIUM medium;
            comData.GetData(ref fmt, out medium);
            string[] names;
            try
            {
                IntPtr hGlobal = medium.unionmember;
                IntPtr ptr = GlobalLock(hGlobal);
                try
                {
                    int cItems = Marshal.ReadInt32(ptr);
                    names = new string[cItems];
                    long basePtr = ptr.ToInt64() + 4;
                    for (int i = 0; i < cItems; i++)
                    {
                        IntPtr fdPtr = new(basePtr + i * Marshal.SizeOf(typeof(FILEDESCRIPTOR)));
                        var fd = Marshal.PtrToStructure<FILEDESCRIPTOR>(fdPtr);
                        names[i] = fd.cFileName;
                    }
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }
            }
            finally
            {
                ReleaseStgMedium(ref medium);
            }

            if (names.Length == 0) return [];

            // 2) 针对每个索引读 FileContents
            var results = new string[names.Length];
            for (var index = 0; index < names.Length; index++)
            {
                FORMATETC fmtContents = new()
                {
                    cfFormat = (short)DataFormats.GetDataFormat("FileContents").Id,
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = index,
                    ptd = IntPtr.Zero,
                    tymed = TYMED.TYMED_ISTREAM | TYMED.TYMED_HGLOBAL
                };

                comData.GetData(ref fmtContents, out medium);
                Stream? contentStream = null;
                try
                {
                    if (medium.tymed == TYMED.TYMED_ISTREAM)
                    {
                        var comStream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                        try
                        {
                            contentStream = ComStreamToManagedStream(comStream);
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(comStream);
                        }
                    }
                    else if (medium.tymed == TYMED.TYMED_HGLOBAL)
                    {
                        IntPtr hGlobal = medium.unionmember;
                        IntPtr dataPtr = GlobalLock(hGlobal);
                        try
                        {
                            // 大小通过 GlobalSize 获取
                            int size = GlobalSize(hGlobal);
                            byte[] buf = new byte[size];
                            Marshal.Copy(dataPtr, buf, 0, size);
                            contentStream = new MemoryStream(buf);
                        }
                        finally
                        {
                            GlobalUnlock(hGlobal);
                        }
                    }

                    if (contentStream != null)
                    {
                        string outPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(names[index]));
                        using var fs = File.Create(outPath);
                        contentStream.CopyTo(fs);
                        results[index] = outPath;
                    }
                }
                finally
                {
                    ReleaseStgMedium(ref medium);
                    contentStream?.Close();
                }
            }
            return results;
        }
        catch (COMException)
        {
            // 读取过程中可能仍然抛出 DV_E_FORMATETC 等，捕获并返回
            return [];
        }
    }

    // 辅助：把 COM IStream 转为 .NET Stream（简单实现）
    private static Stream ComStreamToManagedStream(IStream comStream)
    {
        comStream.Seek(0, 0, 0);
        comStream.Stat(out STATSTG stat, 0);
        long size = stat.cbSize;
        byte[] buf = new byte[size];
        comStream.Read(buf, buf.Length, IntPtr.Zero);
        return new MemoryStream(buf);
    }

    [DllImport("kernel32.dll")]
    static extern IntPtr GlobalLock(IntPtr hMem);
    [DllImport("kernel32.dll")]
    static extern bool GlobalUnlock(IntPtr hMem);
    [DllImport("kernel32.dll")]
    static extern int GlobalSize(IntPtr hMem);
    [DllImport("ole32.dll")]
    static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
}