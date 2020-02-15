using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Migrations
{
    public partial class V2020_02_15_0902 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Users");
        }
    }
}
