using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserGroupSite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopicSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    VolunteerSpeakerId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicSuggestions_AspNetUsers_VolunteerSpeakerId",
                        column: x => x.VolunteerSpeakerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TopicSuggestionLikes",
                columns: table => new
                {
                    TopicSuggestionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSuggestionLikes", x => new { x.TopicSuggestionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TopicSuggestionLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopicSuggestionLikes_TopicSuggestions_TopicSuggestionId",
                        column: x => x.TopicSuggestionId,
                        principalTable: "TopicSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopicSuggestionLikes_UserId",
                table: "TopicSuggestionLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicSuggestions_VolunteerSpeakerId",
                table: "TopicSuggestions",
                column: "VolunteerSpeakerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopicSuggestionLikes");

            migrationBuilder.DropTable(
                name: "TopicSuggestions");
        }
    }
}
