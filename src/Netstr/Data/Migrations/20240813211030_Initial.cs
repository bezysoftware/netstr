using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Netstr.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<string>(type: "text", nullable: false),
                    EventPublicKey = table.Column<string>(type: "text", nullable: false),
                    EventCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventKind = table.Column<long>(type: "bigint", nullable: false),
                    EventContent = table.Column<string>(type: "text", nullable: false),
                    EventSignature = table.Column<string>(type: "text", nullable: false),
                    EventDeduplication = table.Column<string>(type: "text", nullable: true),
                    EventExpiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FirstSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Values = table.Column<string[]>(type: "text[]", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => new { x.EventId, x.Name, x.Values });
                    table.ForeignKey(
                        name: "FK_Tags_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "EventIdIdx",
                table: "Events",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ReplaceableEventsIdx",
                table: "Events",
                columns: new[] { "EventPublicKey", "EventKind", "EventDeduplication" },
                unique: true,
                filter: "\r\n                    (\"EventKind\" = 0) OR \r\n                    (\"EventKind\" = 3) OR \r\n                    (\"EventKind\" >= 10000 AND \"EventKind\" < 20000) OR \r\n                    (\"EventKind\" >= 30000 AND \"EventKind\" < 40000)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
