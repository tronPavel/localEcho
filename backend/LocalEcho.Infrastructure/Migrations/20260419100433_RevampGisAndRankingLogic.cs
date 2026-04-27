using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace LocalEcho.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevampGisAndRankingLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconColor",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Geometry>(
                name: "Location",
                table: "Markers",
                type: "geometry(Geometry, 4326)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry(Point, 4326)");

            migrationBuilder.AlterColumn<Guid>(
                name: "DistrictId",
                table: "Markers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Markers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "Markers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MarkerId",
                table: "MarkerImages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "MarkerResolutionId",
                table: "MarkerImages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarkerResolutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkerResolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarkerResolutions_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarkerResolutions_Markers_MarkerId",
                        column: x => x.MarkerId,
                        principalTable: "Markers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarkerImages_MarkerResolutionId",
                table: "MarkerImages",
                column: "MarkerResolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkerResolutions_MarkerId",
                table: "MarkerResolutions",
                column: "MarkerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarkerResolutions_ResolvedByUserId",
                table: "MarkerResolutions",
                column: "ResolvedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MarkerImages_MarkerResolutions_MarkerResolutionId",
                table: "MarkerImages",
                column: "MarkerResolutionId",
                principalTable: "MarkerResolutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarkerImages_MarkerResolutions_MarkerResolutionId",
                table: "MarkerImages");

            migrationBuilder.DropTable(
                name: "MarkerResolutions");

            migrationBuilder.DropIndex(
                name: "IX_MarkerImages_MarkerResolutionId",
                table: "MarkerImages");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Markers");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "Markers");

            migrationBuilder.DropColumn(
                name: "MarkerResolutionId",
                table: "MarkerImages");

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "Markers",
                type: "geometry(Point, 4326)",
                nullable: false,
                oldClrType: typeof(Geometry),
                oldType: "geometry(Geometry, 4326)");

            migrationBuilder.AlterColumn<Guid>(
                name: "DistrictId",
                table: "Markers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MarkerId",
                table: "MarkerImages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconColor",
                table: "Districts",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
