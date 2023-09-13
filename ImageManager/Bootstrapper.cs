using ImageManager.Data;
using ImageManager.Logging;
using ImageManager.Migrations;
using ImageManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StyletIoC;

namespace ImageManager
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        private Logging.Logger _logger = LoggerFactory.GetLogger(nameof(Bootstrapper));
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            var userSettingData = UserSettingData.Default;
            builder.Bind<UserSettingData>().ToInstance(userSettingData);

            // 准备数据库
            var context = new ImageContext();
            var migrator = context.GetService<IMigrator>();
            foreach (var migration in context.Database.GetPendingMigrations())
            {
                migrator.MigrateEx(migration, context);
            }
            // 为每个窗体都配置一个DB上下文
            builder.Bind<ImageContext>()
                .ToFactory(container => new ImageContext());


            // 清理
            Task.Run(() =>
            {
                // 解决SQLite并发处理的问题
                var context = new ImageContext();
                // 清理待删除文件
                if (userSettingData.WaitToDeleteFiles != null)
                {
                    foreach (var file in userSettingData.WaitToDeleteFiles)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    userSettingData.WaitToDeleteFiles = null;
                }
                // 清理临时文件夹
                foreach (var file in Directory.GetFiles(userSettingData.TempFolderPath))
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                // 清理0引用的标签
                var labels = context.Labels.Where(x => x.Num == 0).ToList();
                context.Labels.RemoveRange(labels);
                context.SaveChangesAsync();
            });
        }
    }
}
