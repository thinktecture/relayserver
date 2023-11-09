using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    public partial class Add_MaximumConcurrentConnectorRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumConcurrentConnectorRequests",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumConcurrentConnectorRequests",
                table: "Tenants");
        }
    }
}
