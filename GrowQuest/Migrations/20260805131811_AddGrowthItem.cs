using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrowQuest.Migrations
{
    /// <inheritdoc />
    public partial class AddGrowthItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrowthItems",
                columns: table => new
                {
                    GrowthItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    ProgressPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthItems", x => x.GrowthItemId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrowthItems");
        }
    }
}
