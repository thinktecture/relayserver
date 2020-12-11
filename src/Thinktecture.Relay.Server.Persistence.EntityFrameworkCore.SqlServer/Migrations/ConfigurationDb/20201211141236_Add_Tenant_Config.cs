using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.Migrations.ConfigurationDb
{
    public partial class Add_Tenant_Config : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(nullable: false),
                    KeepAliveInterval = table.Column<TimeSpan>(nullable: true),
                    EnableTracing = table.Column<bool>(nullable: true),
                    ReconnectMinimumDelay = table.Column<TimeSpan>(nullable: true),
                    ReconnectMaximumDelay = table.Column<TimeSpan>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_Configs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configs");
        }
    }
}
