using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint23Execution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                schema: "ord",
                table: "WorkOrder",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusBeforeHold",
                schema: "ord",
                table: "WorkOrder",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartedBy",
                schema: "ord",
                table: "Operation",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                schema: "ord",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "StatusBeforeHold",
                schema: "ord",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "StartedBy",
                schema: "ord",
                table: "Operation");
        }
    }
}
