using Microsoft.EntityFrameworkCore.Migrations;

namespace Issue9SQLiteInt32.Migrations
{
    public partial class V2020_03_23_0014 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NumericDecimalValue",
                table: "People",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumericDecimalValue",
                table: "People");
        }
    }
}
