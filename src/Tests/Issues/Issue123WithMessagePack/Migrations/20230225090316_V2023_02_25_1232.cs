using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Issue123WithMessagePack.Migrations
{
    /// <inheritdoc />
    public partial class V2023_02_25_1232 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOnly",
                table: "People",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeOnly",
                table: "People",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOnly",
                table: "People");

            migrationBuilder.DropColumn(
                name: "TimeOnly",
                table: "People");
        }
    }
}
