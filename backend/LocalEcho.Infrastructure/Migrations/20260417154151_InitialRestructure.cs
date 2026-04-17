using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalEcho.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Markers");

            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "MarkerImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    MarkerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkerImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarkerImages_Markers_MarkerId",
                        column: x => x.MarkerId,
                        principalTable: "Markers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarkerImages_MarkerId",
                table: "MarkerImages",
                column: "MarkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarkerImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Markers",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeen",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
