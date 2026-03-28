using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No operation needed - RowVersion is already correct in InitTables migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No operation needed - RowVersion is already correct in InitTables migration
        }
    }
}
