using System.Drawing;
using System.Security.Cryptography;

namespace ImageManager.Tools
{
    class ImageComparer
    {
        /// <summary>
        /// 返回图片感知hash值
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ulong GetImagePHash(Bitmap bitmap)
        {
            // 缩放为8*8
            using var thumb = (Bitmap)bitmap.GetThumbnailImage(8, 8, () => { return false; }, IntPtr.Zero);
            // 转为灰度图
            int[,] pixels = new int[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var pixel = thumb.GetPixel(i, j);
                    pixels[i, j] = (pixel.R * 19595 + pixel.G * 38469 + pixel.B * 7472) >> 16;
                }
            }
            // 计算平均值
            int avg = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    avg += pixels[i, j];
                }
            }
            avg /= 64;
            // 计算hash值
            ulong hash = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pixels[i, j] >= avg)
                    {
                        hash |= (ulong)1 << (i * 8 + j);
                    }
                }
            }
            return hash;
        }
        public static int GetHammingDistance(ulong hash1, ulong hash2)
        {
            ulong xor = hash1 ^ hash2;
            int distance = 0;
            while (xor != 0)
            {
                distance++;
                xor &= xor - 1;
            }
            return distance;
        }
        public static string GetMD5Hash(Stream stream)
        {
            var hash = MD5.HashData(stream);
            return Convert.ToHexString(hash);
        }
    }
}
