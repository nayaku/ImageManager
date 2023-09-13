using ImageManager.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System.Security.Cryptography;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePictureHash : MigrationCustom<ImageContext>
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "WeakHash",
                table: "Pictures",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Hash",
                table: "Pictures",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
        public override void PostUp(ImageContext context)
        {
            base.PostUp(context);
            // 更新图片的Hash
            foreach (var picture in context.Pictures)
            {
                var filePath = Path.Join(picture.ImageFolderPath, picture.Path);
                using var fs = File.OpenRead(filePath);
                using var reader = new BufferedStream(fs, 16 * 1024 * 1024); //16MB
                var md5 = MD5.HashData(reader);
                picture.Hash = md5;
                // 读取图片
                reader.Seek(0, SeekOrigin.Begin);
                var fif = FreeImageAPI.FreeImage.GetFileTypeFromStream(reader);
                var fibitmap = FreeImageAPI.FreeImage.LoadFromStream(reader, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref fif);
                // 判断是否为位图
                if (FreeImageAPI.FreeImage.GetImageType(fibitmap) != FreeImageAPI.FREE_IMAGE_TYPE.FIT_BITMAP)
                {
                    var newBitmap = FreeImageAPI.FreeImage.ConvertToType(fibitmap, FreeImageAPI.FREE_IMAGE_TYPE.FIT_BITMAP, true);
                    FreeImageAPI.FreeImage.Unload(fibitmap);
                    fibitmap = newBitmap;
                }
                // 转换为GDI+图片
                using var bitmap = FreeImageAPI.FreeImage.GetBitmap(fibitmap);
                FreeImageAPI.FreeImage.Unload(fibitmap);
                // 相似判断
                var phash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage()).Coefficients;
                picture.WeakHash = phash;
            }
            context.SaveChanges();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new System.NotImplementedException();
            //migrationBuilder.AlterColumn<ulong>(
            //    name: "WeakHash",
            //    table: "Pictures",
            //    type: "INTEGER",
            //    nullable: true,
            //    oldClrType: typeof(byte[]),
            //    oldType: "BLOB");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Hash",
            //    table: "Pictures",
            //    type: "TEXT",
            //    nullable: false,
            //    oldClrType: typeof(byte[]),
            //    oldType: "BLOB");
        }
    }
}
