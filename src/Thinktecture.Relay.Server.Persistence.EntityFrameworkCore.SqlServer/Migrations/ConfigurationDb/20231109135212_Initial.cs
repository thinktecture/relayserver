using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.Migrations.ConfigurationDb
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    TenantName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KeepAliveInterval = table.Column<TimeSpan>(type: "time", nullable: true),
                    EnableTracing = table.Column<bool>(type: "bit", nullable: true),
                    ReconnectMinimumDelay = table.Column<TimeSpan>(type: "time", nullable: true),
                    ReconnectMaximumDelay = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.TenantName);
                });

            migrationBuilder.CreateTable(
                name: "Origins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartupTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ShutdownTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSeenTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfigTenantName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.NormalizedName);
                    table.ForeignKey(
                        name: "FK_Tenants_Configs_ConfigTenantName",
                        column: x => x.ConfigTenantName,
                        principalTable: "Configs",
                        principalColumn: "TenantName");
                });

            migrationBuilder.CreateTable(
                name: "ClientSecrets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSecrets_Tenants_TenantName",
                        column: x => x.TenantName,
                        principalTable: "Tenants",
                        principalColumn: "NormalizedName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    OriginId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DisconnectTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSeenTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_Origins_OriginId",
                        column: x => x.OriginId,
                        principalTable: "Origins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Connections_Tenants_TenantName",
                        column: x => x.TenantName,
                        principalTable: "Tenants",
                        principalColumn: "NormalizedName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequestDuration = table.Column<long>(type: "bigint", nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Target = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestOriginalBodySize = table.Column<long>(type: "bigint", nullable: false),
                    RequestBodySize = table.Column<long>(type: "bigint", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseOriginalBodySize = table.Column<long>(type: "bigint", nullable: true),
                    ResponseBodySize = table.Column<long>(type: "bigint", nullable: true),
                    Aborted = table.Column<bool>(type: "bit", nullable: false),
                    Failed = table.Column<bool>(type: "bit", nullable: false),
                    Expired = table.Column<bool>(type: "bit", nullable: false),
                    Errored = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Tenants_TenantName",
                        column: x => x.TenantName,
                        principalTable: "Tenants",
                        principalColumn: "NormalizedName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecrets_TenantName",
                table: "ClientSecrets",
                column: "TenantName");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_OriginId",
                table: "Connections",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_TenantName",
                table: "Connections",
                column: "TenantName");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TenantName",
                table: "Requests",
                column: "TenantName");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ConfigTenantName",
                table: "Tenants",
                column: "ConfigTenantName");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientSecrets");

            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Origins");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Configs");
        }
    }
}
