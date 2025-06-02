using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CashFlow.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DailyConsolidatedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LineNumber",
                table: "Entries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Entries_LineNumber",
                table: "Entries",
                column: "LineNumber");

            migrationBuilder.CreateTable(
                name: "DailyConsolidated",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalCredits = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalDebits = table.Column<decimal>(type: "numeric", nullable: false),
                    LastLineNumberCalculated = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyConsolidated", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyConsolidated_Entries_LastLineNumberCalculated",
                        column: x => x.LastLineNumberCalculated,
                        principalTable: "Entries",
                        principalColumn: "LineNumber");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entry_LineNumber",
                table: "Entries",
                column: "LineNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyConsolidated_Date",
                table: "DailyConsolidated",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyConsolidated_LastLineNumberCalculated",
                table: "DailyConsolidated",
                column: "LastLineNumberCalculated",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyConsolidated");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Entries_LineNumber",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entry_LineNumber",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "Entries");
        }
    }
}
