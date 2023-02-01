using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class BigChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspacePicture");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Pictures");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Pictures",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "Pictures",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Pictures",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "Pictures",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Pictures",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "WorkspacePicture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PictureId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFolded = table.Column<bool>(type: "INTEGER", nullable: false),
                    Left = table.Column<double>(type: "REAL", nullable: false),
                    Opacity = table.Column<double>(type: "REAL", nullable: false),
                    RotateFlip = table.Column<int>(type: "INTEGER", nullable: false),
                    Top = table.Column<double>(type: "REAL", nullable: false),
                    WorkspaceName = table.Column<string>(type: "TEXT", nullable: true),
                    ZoomRate = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspacePicture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspacePicture_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspacePicture_Workspaces_WorkspaceName",
                        column: x => x.WorkspaceName,
                        principalTable: "Workspaces",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspacePicture_PictureId",
                table: "WorkspacePicture",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspacePicture_WorkspaceName",
                table: "WorkspacePicture",
                column: "WorkspaceName");
        }
    }
}
