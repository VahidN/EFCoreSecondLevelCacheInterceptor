using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Migrations;

public partial class V2020_10_15_1029 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BlogData",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                SiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
            });

        migrationBuilder.CreateTable(
            name: "Blogs",
            columns: table => new
            {
                BlogId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Blogs", x => x.BlogId);
            });

        migrationBuilder.CreateTable(
            name: "DateTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                AddDateValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UpdateDateValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                RelativeAddTimeValue = table.Column<TimeSpan>(type: "time", nullable: true),
                RelativeUpdateTimeValue = table.Column<TimeSpan>(type: "time", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DateTypes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EngineVersions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Commercial_Major = table.Column<int>(type: "int", nullable: true),
                Commercial_Minor = table.Column<int>(type: "int", nullable: true),
                Commercial_Revision = table.Column<int>(type: "int", nullable: true),
                Commercial_Patch = table.Column<int>(type: "int", nullable: true),
                Retail_Major = table.Column<int>(type: "int", nullable: true),
                Retail_Minor = table.Column<int>(type: "int", nullable: true),
                Retail_Revision = table.Column<int>(type: "int", nullable: true),
                Retail_Patch = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EngineVersions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tags", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UserStatus = table.Column<int>(type: "int", nullable: false),
                ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Points = table.Column<long>(type: "bigint", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ByteValue = table.Column<byte>(type: "tinyint", nullable: false),
                CharValue = table.Column<string>(type: "nvarchar(1)", nullable: false),
                DateTimeOffsetValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DecimalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                DoubleValue = table.Column<double>(type: "float", nullable: false),
                FloatValue = table.Column<float>(type: "real", nullable: false),
                GuidValue = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TimeSpanValue = table.Column<TimeSpan>(type: "time", nullable: true),
                ShortValue = table.Column<short>(type: "smallint", nullable: false),
                ByteArrayValue = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                UintValue = table.Column<long>(type: "bigint", nullable: false),
                UlongValue = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                UshortValue = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Posts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<int>(type: "int", nullable: false),
                BlogId = table.Column<int>(type: "int", nullable: false),
                post_type = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Posts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Posts_Blogs_BlogId",
                    column: x => x.BlogId,
                    principalTable: "Blogs",
                    principalColumn: "BlogId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Posts_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                ProductId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ProductName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.ProductId);
                table.ForeignKey(
                    name: "FK_Products_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TagProducts",
            columns: table => new
            {
                TagId = table.Column<int>(type: "int", nullable: false),
                ProductProductId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TagProducts", x => new { x.TagId, x.ProductProductId });
                table.ForeignKey(
                    name: "FK_TagProducts_Products_ProductProductId",
                    column: x => x.ProductProductId,
                    principalTable: "Products",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TagProducts_Tags_TagId",
                    column: x => x.TagId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "Blogs",
            columns: new[] { "BlogId", "Url" },
            values: new object[,]
            {
                { 1, "https://site1.com" },
                { 2, "https://site2.com" }
            });

        migrationBuilder.InsertData(
            table: "Tags",
            columns: new[] { "Id", "Name" },
            values: new object[,]
            {
                { 1, "Tag4" },
                { 2, "Tag1" },
                { 3, "Tag2" },
                { 4, "Tag3" }
            });

        migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "AddDate", "ByteArrayValue", "ByteValue", "CharValue", "DateTimeOffsetValue", "DecimalValue", "DoubleValue", "FloatValue", "GuidValue", "ImageData", "IsActive", "Name", "Points", "ShortValue", "TimeSpanValue", "UintValue", "UlongValue", "UpdateDate", "UserStatus", "UshortValue" },
            values: new object[] { 1, null, new byte[] { 1, 2 }, (byte)1, "C", null, 1.1m, 1.3, 1.2f, new Guid("236bbe40-b861-433c-8789-b152a99cfe3e"), null, true, "User1", 1000L, (short)2, null, 1L, 1m, null, 0, 1m });

        migrationBuilder.InsertData(
            table: "Posts",
            columns: new[] { "Id", "BlogId", "Title", "UserId", "post_type" },
            values: new object[,]
            {
                { 1, 1, "Post1", 1, "post_base" },
                { 2, 1, "Post2", 1, "post_base" }
            });

        migrationBuilder.InsertData(
            table: "Products",
            columns: new[] { "ProductId", "IsActive", "Notes", "ProductName", "ProductNumber", "UserId" },
            values: new object[,]
            {
                { 1, false, "Notes ...", "Product4", "004", 1 },
                { 2, true, "Notes ...", "Product1", "001", 1 },
                { 3, true, "Notes ...", "Product2", "002", 1 },
                { 4, true, "Notes ...", "Product3", "003", 1 }
            });

        migrationBuilder.InsertData(
            table: "TagProducts",
            columns: new[] { "ProductProductId", "TagId" },
            values: new object[,]
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Posts_BlogId",
            table: "Posts",
            column: "BlogId");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_UserId",
            table: "Posts",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_ProductName",
            table: "Products",
            column: "ProductName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_UserId",
            table: "Products",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_TagProducts_ProductProductId",
            table: "TagProducts",
            column: "ProductProductId");

        migrationBuilder.CreateIndex(
            name: "IX_TagProducts_TagId",
            table: "TagProducts",
            column: "TagId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BlogData");

        migrationBuilder.DropTable(
            name: "DateTypes");

        migrationBuilder.DropTable(
            name: "EngineVersions");

        migrationBuilder.DropTable(
            name: "Posts");

        migrationBuilder.DropTable(
            name: "TagProducts");

        migrationBuilder.DropTable(
            name: "Blogs");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Tags");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
