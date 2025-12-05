using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSerhanKitapEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerhanKitaplar",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    KitapName = table.Column<string>(type: "TEXT", nullable: false),
                    KitapYazar = table.Column<string>(type: "TEXT", nullable: false),
                    KitapSayfaSayisi = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerhanKitaplar", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerhanKitaplar");
        }
    }
}
