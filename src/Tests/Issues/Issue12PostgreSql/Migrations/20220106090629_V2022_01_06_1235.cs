using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Issue12PostgreSql.Migrations
{
    public partial class V2022_01_06_1235 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Date1",
                table: "People",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date2",
                table: "People",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Time1",
                table: "People",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Time2",
                table: "People",
                type: "time without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date1",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Date2",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Time1",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Time2",
                table: "People");
        }
    }
}
