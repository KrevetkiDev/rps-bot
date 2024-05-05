using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BotRps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBetToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bet",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bet",
                table: "Users");
        }
    }
}
