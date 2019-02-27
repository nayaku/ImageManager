using ImageManager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageManager
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 非法字符串列表
        /// </summary>
        public static char[] InvalidChars { get; private set; }

        static Utils()
        {
            var invalidArray = new List<char>();
            invalidArray.AddRange(Path.GetInvalidFileNameChars());
            invalidArray.AddRange(" ?_%[]^!\'\"".ToCharArray());
            InvalidChars = invalidArray.ToArray();
        }

        /// <summary>
        /// 转换路径字符串
        /// <ImgLib>将会替换为图片库的位置
        /// </summary>
        /// <param name="path">路径</param>
        public static string ConvertPath(string path)
        {
            return path.Replace("<ImgLib>", Settings.Default.ImageLibPath);
        }

        /// <summary>
        /// 复制到库
        /// </summary>
        /// <param name="sourcePath">源文件的位置</param>
        /// <returns>返回拷贝后的文件名。如果目标文件夹文件名已经存在，返回新的文件名。</returns>
        public static string CopyToLib(string sourcePath)
        {
            var destPath = Settings.Default.ImageLibPath;
            string destFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
            string destFileNameExtension = Path.GetExtension(sourcePath);
            if (!destPath.EndsWith("\\"))
            {
                destPath += "\\";
            }
            while (File.Exists(destPath + destFileNameWithoutExtension + destFileNameExtension))
            {
                destFileNameWithoutExtension += "_" + new Random().Next(100);
            }
            string destFileName = destFileNameWithoutExtension + destFileNameExtension;
            File.Copy(sourcePath, destPath + destFileName);
            return destFileName;
        }

        /// <summary>
        /// 判断字符是否是非法字符
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns></returns>
        public static bool IsValidChar(char c)
        {

            foreach (var ic in InvalidChars)
            {
                if (c == ic)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断字符串是否是合法字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidString(String str)
        {
            foreach (var c in str)
            {
                if (!IsValidChar(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 转换字符串成为合法字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertToValidString(String str)
        {
            var resStringBuilder = new StringBuilder();
            foreach (var c in str)
            {
                if (IsValidChar(c))
                {
                    resStringBuilder.Append(c);
                }
            }
            return resStringBuilder.ToString();
        }
        ///// <summary>
        ///// 获取MD5码
        ///// </summary>
        ///// <param name="path">路径</param>
        ///// <returns>返回md5码。如果文件不存在返回null</returns>
        //public static string GetMD5Hash(String path)
        //{
        //    path = ConvertPath(path);
        //    if (File.Exists(path))
        //    {
        //        var file = new FileStream(path, FileMode.Open);
        //        MD5 md5 = new MD5CryptoServiceProvider();
        //        byte[] retVal = md5.ComputeHash(file);

        //        StringBuilder sb = new StringBuilder();
        //        for (int i = 0; i < retVal.Length; i++)
        //        {
        //            sb.Append(retVal[i].ToString("x2"));
        //        }
        //        file.Close();
        //        return sb.ToString();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// 获取md5码
        /// </summary>
        /// <param name="path">路径。注意必须是实际的路径。</param>
        /// <returns>md5码</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetMD5ByHashAlgorithm(string path)
        {
            //if (!File.Exists(path))
            //    throw new ArgumentException(string.Format("<{0}>, 不存在", path));
            int bufferSize = 1024 * 16;//自定义缓冲区大小16K 
            byte[] buffer = new byte[bufferSize];
            Stream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
            int readLength = 0;//每次读取长度 
            var output = new byte[bufferSize];
            while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //计算MD5 
                hashAlgorithm.TransformBlock(buffer, 0, readLength, output, 0);
            }
            //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0) 
            hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
            string md5 = BitConverter.ToString(hashAlgorithm.Hash);
            hashAlgorithm.Clear();
            inputStream.Close();
            md5 = md5.Replace("-", "");
            return md5;
        }

        /// <summary>
        /// 返回数值最小的编号
        /// </summary>
        /// <param name="array">数组</param>
        /// <returns>数值下标</returns>
        public static int GetMinIndex(int[] array)
        {
            var len = array.Length;
            var minIndex = 0;
            for(var i = 1; i < len; i++)
            {
                if (array[minIndex] > array[i])
                    minIndex = i;
            }
            return minIndex;
        }

    }

}