using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Migrations
{
    public partial class V2020_04_05_1047 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    AddDateValue = table.Column<DateTimeOffset>(nullable: true),
                    UpdateDateValue = table.Column<DateTimeOffset>(nullable: false),
                    RelativeAddTimeValue = table.Column<TimeSpan>(nullable: true),
                    RelativeUpdateTimeValue = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateTypes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DateTypes");
        }
    }
}
