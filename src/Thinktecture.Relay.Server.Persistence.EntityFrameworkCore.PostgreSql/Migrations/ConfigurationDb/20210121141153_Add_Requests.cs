using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    public partial class Add_Requests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(nullable: false),
                    RequestId = table.Column<Guid>(nullable: false),
                    RequestDate = table.Column<DateTimeOffset>(nullable: false),
                    RequestDuration = table.Column<long>(nullable: false),
                    RequestUrl = table.Column<string>(maxLength: 1000, nullable: false),
                    Target = table.Column<string>(maxLength: 100, nullable: false),
                    HttpMethod = table.Column<string>(maxLength: 10, nullable: false),
                    RequestBodySize = table.Column<long>(nullable: false),
                    HttpStatusCode = table.Column<int>(nullable: true),
                    ResponseBodySize = table.Column<long>(nullable: true),
                    Aborted = table.Column<bool>(nullable: false),
                    Failed = table.Column<bool>(nullable: false),
                    Expired = table.Column<bool>(nullable: false),
                    Errored = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TenantId",
                table: "Requests",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
