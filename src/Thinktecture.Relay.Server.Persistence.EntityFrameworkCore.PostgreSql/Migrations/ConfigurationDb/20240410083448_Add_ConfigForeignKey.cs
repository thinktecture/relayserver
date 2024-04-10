using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    public partial class Add_ConfigForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Configs_ConfigTenantName",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ConfigTenantName",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ConfigTenantName",
                table: "Tenants");

            migrationBuilder.AlterColumn<string>(
                name: "TenantName",
                table: "Configs",
                type: "character varying(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Configs_Tenants_TenantName",
                table: "Configs",
                column: "TenantName",
                principalTable: "Tenants",
                principalColumn: "NormalizedName",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Configs_Tenants_TenantName",
                table: "Configs");

            migrationBuilder.AddColumn<string>(
                name: "ConfigTenantName",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantName",
                table: "Configs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ConfigTenantName",
                table: "Tenants",
                column: "ConfigTenantName");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Configs_ConfigTenantName",
                table: "Tenants",
                column: "ConfigTenantName",
                principalTable: "Configs",
                principalColumn: "TenantName");
        }
    }
}
