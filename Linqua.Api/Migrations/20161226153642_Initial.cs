using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Linqua.Api.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Auth0Id = table.Column<string>(maxLength: 256, nullable: false),
                    Email = table.Column<string>(maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ClientCreatedAt = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "sysutcdatetime()"),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: true, defaultValueSql: "sysutcdatetime()"),
                    Definition = table.Column<string>(nullable: true),
                    DefinitionLanguageCode = table.Column<string>(nullable: true, defaultValueSql: "'en'"),
                    Deleted = table.Column<bool>(nullable: false),
                    IsLearnt = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    Text = table.Column<string>(maxLength: 256, nullable: true),
                    TextLanguageCode = table.Column<string>(nullable: true),
                    TranslationState = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: true),
                    UserId = table.Column<Guid>(nullable: true),
                    Version = table.Column<byte[]>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_linqua.Entries_linqua.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreatedAt",
                table: "Entries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Text",
                table: "Entries",
                column: "Text");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_UserId",
                table: "Entries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Auth0Id",
                table: "Users",
                column: "Auth0Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
