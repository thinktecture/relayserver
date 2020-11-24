using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    public partial class Rename_Properties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeartbeatTime",
                table: "Origins");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Origins");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSeenTime",
                table: "Origins",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartupTime",
                table: "Origins",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Connections",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenTime",
                table: "Origins");

            migrationBuilder.DropColumn(
                name: "StartupTime",
                table: "Origins");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "HeartbeatTime",
                table: "Origins",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartTime",
                table: "Origins",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Connections",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);
        }
    }
}
