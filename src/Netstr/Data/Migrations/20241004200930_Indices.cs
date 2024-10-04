using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Netstr.Data.Migrations
{
    /// <inheritdoc />
    public partial class Indices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "Values",
                table: "Tags",
                newName: "OtherValues");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_EventId",
                table: "Tags",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "TagNameValueIdx",
                table: "Tags",
                columns: new[] { "Name", "Value", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EventLookupIdx",
                table: "Events",
                columns: new[] { "EventKind", "EventPublicKey", "EventCreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_EventId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "TagNameValueIdx",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "EventLookupIdx",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "OtherValues",
                table: "Tags",
                newName: "Values");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                columns: new[] { "EventId", "Name", "Values" });
        }
    }
}
