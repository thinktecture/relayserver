using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    public partial class Add_ResponseOriginalBodySize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ResponseOriginalBodySize",
                table: "Requests",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseOriginalBodySize",
                table: "Requests");
        }
    }
}
