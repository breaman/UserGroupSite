using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserGroupSite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetupEventIdToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeetupEventId",
                table: "Events",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetupEventId",
                table: "Events");
        }
    }
}
