﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    public partial class Rename_Properties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.RenameColumn(
		        name: "HeartbeatTime",
		        newName: "LastSeenTime",
		        table: "Origins");

	        migrationBuilder.RenameColumn(
		        name: "StartTime",
		        newName: "StartupTime",
		        table: "Origins");

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
	        migrationBuilder.RenameColumn(
		        name: "LastSeenTime",
		        newName: "HeartbeatTime",
		        table: "Origins");

	        migrationBuilder.RenameColumn(
		        name: "StartupTime",
		        newName: "StartTime",
		        table: "Origins");

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
