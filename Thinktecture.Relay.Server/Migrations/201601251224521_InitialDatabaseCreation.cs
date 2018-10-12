namespace Thinktecture.Relay.Server.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class InitialDatabaseCreation : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.Links",
				c => new
				{
					Id = c.Guid(nullable: false),
					Identity = c.Int(nullable: false, identity: true),
					UserName = c.String(nullable: false, maxLength: 250),
					Password = c.String(nullable: false),
					Iterations = c.Int(nullable: false),
					Salt = c.String(nullable: false),
					SymbolicName = c.String(nullable: false, maxLength: 250),
					IsDisabled = c.Boolean(nullable: false),
					ForwardOnPremiseTargetErrorResponse = c.Boolean(nullable: false),
					AllowLocalClientRequestsOnly = c.Boolean(nullable: false),
					MaximumLinks = c.Int(nullable: false),
					CreationDate = c.DateTime(nullable: false),
				})
				.PrimaryKey(t => t.Id, clustered: false)
				.Index(t => t.Identity, clustered: true)
				.Index(t => t.UserName, unique: true, name: "UserNameIndex");

			CreateTable(
				"dbo.RequestLogEntries",
				c => new
				{
					Id = c.Guid(nullable: false),
					Identity = c.Int(nullable: false, identity: true),
					OriginId = c.Guid(nullable: false),
					HttpStatusCode = c.Int(nullable: false),
					LinkId = c.Guid(nullable: false),
					OnPremiseTargetKey = c.String(nullable: false),
					LocalUrl = c.String(nullable: false),
					ContentBytesIn = c.Long(nullable: false),
					ContentBytesOut = c.Long(nullable: false),
					OnPremiseConnectorInDate = c.DateTime(nullable: false),
					OnPremiseConnectorOutDate = c.DateTime(nullable: false),
					OnPremiseTargetInDate = c.DateTime(),
					OnPremiseTargetOutDate = c.DateTime(),
				})
				.PrimaryKey(t => t.Id, clustered: false)
				.ForeignKey("dbo.Links", t => t.LinkId, cascadeDelete: true)
				.Index(t => t.Identity, clustered: true)
				.Index(t => t.LinkId);

			CreateTable(
				"dbo.TraceConfigurations",
				c => new
				{
					Id = c.Guid(nullable: false),
					Identity = c.Int(nullable: false, identity: true),
					LinkId = c.Guid(nullable: false),
					StartDate = c.DateTime(nullable: false),
					EndDate = c.DateTime(nullable: false),
					CreationDate = c.DateTime(nullable: false),
				})
				.PrimaryKey(t => t.Id, clustered: false)
				.ForeignKey("dbo.Links", t => t.LinkId, cascadeDelete: true)
				.Index(t => t.Identity, clustered: true)
				.Index(t => t.LinkId);

			CreateTable(
				"dbo.Users",
				c => new
				{
					Id = c.Guid(nullable: false),
					Identity = c.Int(nullable: false, identity: true),
					UserName = c.String(nullable: false, maxLength: 250),
					Password = c.String(nullable: false),
					Salt = c.String(nullable: false),
					Iterations = c.Int(nullable: false),
					CreationDate = c.DateTime(nullable: false),
				})
				.PrimaryKey(t => t.Id, clustered: false)
				.Index(t => t.Identity, clustered: true)
				.Index(t => t.UserName, unique: true, name: "UserNameIndex");
		}

		public override void Down()
		{
			DropForeignKey("dbo.TraceConfigurations", "LinkId", "dbo.Links");
			DropForeignKey("dbo.RequestLogEntries", "LinkId", "dbo.Links");
			DropIndex("dbo.Users", "UserNameIndex");
			DropIndex("dbo.Users", new[] { "Identity" });
			DropIndex("dbo.TraceConfigurations", new[] { "LinkId" });
			DropIndex("dbo.TraceConfigurations", new[] { "Identity" });
			DropIndex("dbo.RequestLogEntries", new[] { "LinkId" });
			DropIndex("dbo.RequestLogEntries", new[] { "Identity" });
			DropIndex("dbo.Links", "UserNameIndex");
			DropIndex("dbo.Links", new[] { "Identity" });
			DropTable("dbo.Users");
			DropTable("dbo.TraceConfigurations");
			DropTable("dbo.RequestLogEntries");
			DropTable("dbo.Links");
		}
	}
}
