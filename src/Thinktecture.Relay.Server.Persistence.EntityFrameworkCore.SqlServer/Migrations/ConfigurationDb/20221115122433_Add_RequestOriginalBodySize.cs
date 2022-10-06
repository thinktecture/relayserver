using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.Migrations.ConfigurationDb
{
    public partial class Add_RequestOriginalBodySize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RequestOriginalBodySize",
                table: "Requests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestOriginalBodySize",
                table: "Requests");
        }
    }
}
