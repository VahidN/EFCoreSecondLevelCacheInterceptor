using System;
using System.Collections.Generic;
using Issue12PostgreSql.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Issue12PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class V202310171929 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Array = table.Column<int[]>(type: "integer[]", nullable: true),
                    List = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ByteValue = table.Column<byte>(type: "smallint", nullable: false),
                    CharValue = table.Column<char>(type: "character(1)", nullable: false),
                    DateTimeOffsetValue = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecimalValue = table.Column<decimal>(type: "numeric", nullable: false),
                    DoubleValue = table.Column<double>(type: "double precision", nullable: false),
                    FloatValue = table.Column<float>(type: "real", nullable: false),
                    GuidValue = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeSpanValue = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ShortValue = table.Column<short>(type: "smallint", nullable: false),
                    ByteArrayValue = table.Column<byte[]>(type: "bytea", nullable: true),
                    UintValue = table.Column<long>(type: "bigint", nullable: false),
                    UlongValue = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UshortValue = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OptionDefinitions = table.Column<List<BlogOption>>(type: "jsonb", nullable: true),
                    Date1 = table.Column<DateOnly>(type: "date", nullable: false),
                    Time1 = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Date2 = table.Column<DateOnly>(type: "date", nullable: true),
                    Time2 = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_PersonId",
                table: "Addresses",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PersonId",
                table: "Books",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
