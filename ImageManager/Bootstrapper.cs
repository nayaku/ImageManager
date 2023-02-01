using ImageManager.Data;
using ImageManager.ViewModels;
using Stylet;
using StyletIoC;
using System;
using System.Diagnostics;
using System.IO;

namespace ImageManager
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            var userSettingData = UserSettingData.Default;
            builder.Bind<UserSettingData>().ToInstance(userSettingData);
            // create Database context
            builder.Bind<ImageContext>().ToSelf().InSingletonScope();

            // 清理文件
            Task.Run(() =>
            {
                if (userSettingData.WaitToDeleteFiles != null)
                {
                    foreach (var file in userSettingData.WaitToDeleteFiles)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    userSettingData.WaitToDeleteFiles = null;
                }
                foreach (var file in Directory.GetFiles(userSettingData.TempFolderPath))
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
            });
        }
    }
}
