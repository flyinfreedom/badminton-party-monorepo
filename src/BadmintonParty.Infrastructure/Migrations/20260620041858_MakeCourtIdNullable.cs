using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonParty.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCourtIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Courts_CourtId",
                table: "Groups");

            migrationBuilder.AlterColumn<string>(
                name: "CourtId",
                table: "Groups",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Courts_CourtId",
                table: "Groups",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "CourtId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Courts_CourtId",
                table: "Groups");

            migrationBuilder.AlterColumn<string>(
                name: "CourtId",
                table: "Groups",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Courts_CourtId",
                table: "Groups",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "CourtId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

