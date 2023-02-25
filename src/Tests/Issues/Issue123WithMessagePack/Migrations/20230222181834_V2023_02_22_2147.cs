using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Issue123WithMessagePack.Migrations
{
    /// <inheritdoc />
    public partial class V202302222147 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Span",
                table: "People",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Span",
                table: "People");
        }
    }
}
