using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Connect_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedUpdatedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HostName = table.Column<string>(type: "TEXT", nullable: false),
                    GuestName = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentTurn = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Winner = table.Column<string>(type: "TEXT", nullable: true),
                    GameCode = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameMoves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    MoveTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameMoves_AspNetGames_GameId",
                        column: x => x.GameId,
                        principalTable: "AspNetGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetGames",
                columns: new[] { "Id", "CreationDate", "CurrentTurn", "GameCode", "GuestName", "HostName", "ModificationDate", "Status", "Winner" },
                values: new object[,]
                {
                    { new Guid("04867947-5fbe-4b6b-8f4f-95b0cd92a6aa"), new DateTime(2025, 1, 24, 20, 18, 14, 932, DateTimeKind.Utc).AddTicks(8470), "red", "bga0dgz47", null, "Younes", new DateTime(2025, 1, 24, 20, 18, 14, 932, DateTimeKind.Utc).AddTicks(8470), 0, null },
                    { new Guid("1553ec4f-ba85-42f6-8886-252f25501bbf"), new DateTime(2025, 1, 24, 20, 18, 14, 932, DateTimeKind.Utc).AddTicks(8470), "red", "gyxmd4c7m", "Maria", "Younes", new DateTime(2025, 1, 24, 20, 18, 14, 932, DateTimeKind.Utc).AddTicks(8470), 1, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "CreationDate", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { new Guid("5a9e2504-8873-479b-8e18-366ece6de4f4"), new DateTime(2025, 1, 24, 20, 18, 14, 895, DateTimeKind.Utc).AddTicks(5110), "AQAAAAIAAYagAAAAECliaLHYRYEDwfO5GDtmg3qmEPAkcWn6vlnBxMHET6h8t4ouK+tn6TdPFkvlS+ZPyA==", "Younes" },
                    { new Guid("8293b367-034d-4f18-a865-563fa5a61d78"), new DateTime(2025, 1, 24, 20, 18, 14, 932, DateTimeKind.Utc).AddTicks(8320), "AQAAAAIAAYagAAAAEBqo/3oVisL5QaQ/KPkRF92c5o7z98KNax/skNb4vDqqsI19otIMi2YnjXcw3TG9Tw==", "Maria" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameMoves_GameId",
                table: "GameMoves",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "GameMoves");

            migrationBuilder.DropTable(
                name: "AspNetGames");
        }
    }
}
