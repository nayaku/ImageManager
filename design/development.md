## drag & drop: e.Data.GetDataPresent(DataFormats.StringFormat) does not return True while it should 

It seems it needs to check for the `Text` format and not `StringFormat`

ref: [https://github.com/dotnet/docs-desktop/issues/160](https://github.com/dotnet/docs-desktop/issues/160)