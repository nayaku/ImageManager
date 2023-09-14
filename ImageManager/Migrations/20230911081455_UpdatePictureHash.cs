using ImageManager.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System.Security.Cryptography;
using System.Text;

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
                // md5
                var md5String = Encoding.ASCII.GetString(picture.Hash);
                var md5 = Convert.FromHexString(md5String);
                picture.Hash = md5;
                
                // phash
                var filePath = Path.Join(picture.ImageFolderPath, picture.Path);
                var fif = FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN;
                using var bitmap = FreeImageAPI.FreeImage.LoadBitmap(filePath, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref fif);
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
