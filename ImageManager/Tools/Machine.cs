using ImageManager.Logging;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace ImageManager.Tools
{
    public static class Machine
    {
        public static string GetMachineCode()
        {
            var cpuId = GetCpuId();
            var boardId = GetBoardId();
            var hdId = GetHDId();
            var rawCode = $"{cpuId}-{boardId}-{hdId}";
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(rawCode));

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取CPU序列号
        /// </summary>
        /// <returns></returns>
        public static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("select ProcessorId from Win32_Processor");
                using var collection = searcher.Get();
                foreach (var item in collection)
                {
                    return item["ProcessorId"]?.ToString()?.Trim() ?? "";
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(Machine)).Log(LogLevel.Error, $"获取CPU ID失败: {ex.Message}");
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取主板编号
        /// </summary>
        /// <returns></returns>
        public static string GetBoardId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
                using var collection = searcher.Get();
                foreach (var item in collection)
                {
                    return item["UUID"]?.ToString()?.Trim() ?? "";
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(Machine)).Log(LogLevel.Error, $"获取主板ID失败: {ex.Message}");
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取系统盘所在物理硬盘的序列号
        /// </summary>
        public static string GetHDId()
        {
            try
            {
                // "C:"
                var systemDrive = Path.GetPathRoot(Environment.SystemDirectory)!.TrimEnd('\\');

                // 盘符 → 分区，取分区所在磁盘的索引
                using var partSearcher = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{systemDrive}'}} " +
                    "WHERE AssocClass = Win32_LogicalDiskToPartition");
                using var partCollection = partSearcher.Get();
                foreach (var partition in partCollection)
                {
                    var diskIndex = partition["DiskIndex"];

                    // 磁盘索引 → 物理盘序列号
                    using var diskSearcher = new ManagementObjectSearcher(
                        $"select SerialNumber from Win32_DiskDrive where Index = {diskIndex}");
                    using var diskCollection = diskSearcher.Get();
                    foreach (var disk in diskCollection)
                        return disk["SerialNumber"]?.ToString()?.Trim() ?? "";
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(Machine)).Log(LogLevel.Error, $"获取硬盘ID失败: {ex.Message}");
            }
            return string.Empty;
        }

        public static string GetOperatingSystem()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("select Caption from Win32_OperatingSystem");
                using var collection = searcher.Get();
                foreach (var item in collection)
                {
                    return item["Caption"]?.ToString()?.Trim() ?? "";
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(Machine)).Log(LogLevel.Error, $"获取操作系统失败: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
