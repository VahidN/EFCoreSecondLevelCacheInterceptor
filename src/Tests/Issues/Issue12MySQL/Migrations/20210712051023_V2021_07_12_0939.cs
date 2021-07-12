using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace Issue12MySQL.Migrations
{
    public partial class V2021_07_12_0939 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    AddDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ByteValue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CharValue = table.Column<int>(type: "int", nullable: false),
                    DateTimeOffsetValue = table.Column<DateTime>(type: "datetime", nullable: false),
                    DecimalValue = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DoubleValue = table.Column<double>(type: "double", nullable: false),
                    FloatValue = table.Column<float>(type: "float", nullable: false),
                    GuidValue = table.Column<byte[]>(type: "varbinary(16)", nullable: false),
                    TimeSpanValue = table.Column<TimeSpan>(type: "time", nullable: false),
                    ShortValue = table.Column<short>(type: "smallint", nullable: false),
                    ByteArrayValue = table.Column<byte[]>(type: "varbinary(4000)", nullable: true),
                    UintValue = table.Column<int>(type: "int unsigned", nullable: false),
                    UlongValue = table.Column<long>(type: "bigint unsigned", nullable: false),
                    UshortValue = table.Column<long>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
