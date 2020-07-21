using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Migrations
{
    public partial class V2020_07_21_0820 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Commercial_Major = table.Column<int>(nullable: true),
                    Commercial_Minor = table.Column<int>(nullable: true),
                    Commercial_Revision = table.Column<int>(nullable: true),
                    Commercial_Patch = table.Column<int>(nullable: true),
                    Retail_Major = table.Column<int>(nullable: true),
                    Retail_Minor = table.Column<int>(nullable: true),
                    Retail_Revision = table.Column<int>(nullable: true),
                    Retail_Patch = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineVersions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineVersions");
        }
    }
}
