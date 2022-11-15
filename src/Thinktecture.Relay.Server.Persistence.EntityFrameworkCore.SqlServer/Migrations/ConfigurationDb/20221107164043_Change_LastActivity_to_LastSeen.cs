using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.Migrations.ConfigurationDb
{
    public partial class Change_LastActivity_to_LastSeen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastActivityTime",
                table: "Connections",
                newName: "LastSeenTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastSeenTime",
                table: "Connections",
                newName: "LastActivityTime");
        }
    }
}
