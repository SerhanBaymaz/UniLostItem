using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SerhanKitaplar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "SerhanKitaplar",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SerhanKitaplar",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SerhanKitaplar",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SerhanKitaplar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SerhanKitaplar",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SerhanKitaplar");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "SerhanKitaplar");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SerhanKitaplar");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SerhanKitaplar");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SerhanKitaplar");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SerhanKitaplar");
        }
    }
}
