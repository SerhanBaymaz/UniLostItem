using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLostItemAndClaimEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LostItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactInfo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LocationLabel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LostItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LostItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemClaims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtensionCount = table.Column<int>(type: "integer", nullable: false),
                    LostItemId = table.Column<string>(type: "text", nullable: false),
                    ClaimantId = table.Column<string>(type: "text", nullable: false),
                    OwnerComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OwnerResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "text", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AdminComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemClaims_AspNetUsers_ClaimantId",
                        column: x => x.ClaimantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemClaims_LostItems_LostItemId",
                        column: x => x.LostItemId,
                        principalTable: "LostItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemClaims_ClaimantId",
                table: "ItemClaims",
                column: "ClaimantId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemClaims_LostItemId",
                table: "ItemClaims",
                column: "LostItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemClaims_Status",
                table: "ItemClaims",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LostItems_Category",
                table: "LostItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LostItems_ItemType",
                table: "LostItems",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_LostItems_Status",
                table: "LostItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LostItems_UserId",
                table: "LostItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemClaims");

            migrationBuilder.DropTable(
                name: "LostItems");
        }
    }
}