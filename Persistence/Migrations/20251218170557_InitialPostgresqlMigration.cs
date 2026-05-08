using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresqlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerhanKitaplar",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    KitapName = table.Column<string>(type: "text", nullable: false),
                    KitapYazar = table.Column<string>(type: "text", nullable: false),
                    KitapSayfaSayisi = table.Column<int>(type: "integer", nullable: false)
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
