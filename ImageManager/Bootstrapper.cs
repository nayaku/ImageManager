using ImageManager.Data;
using ImageManager.ViewModels;
using Stylet;
using StyletIoC;
using System;

namespace ImageManager
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<UserSettingData>().ToInstance(UserSettingData.Default);
            // create Database context
            builder.Bind<ImageContext>().ToSelf().InSingletonScope();

        }
    }
}
