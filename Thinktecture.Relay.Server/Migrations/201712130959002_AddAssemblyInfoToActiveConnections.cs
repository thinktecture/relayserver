namespace Thinktecture.Relay.Server.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class AddAssemblyInfoToActiveConnections : DbMigration
	{
		public override void Up()
		{
			AddColumn("dbo.ActiveConnections", "AssemblyVersion", c => c.String(nullable: true));

			Sql("UPDATE dbo.ActiveConnections SET AssemblyVersion = 'Unknown' WHERE AssemblyVersion IS NULL");

			AlterColumn("dbo.ActiveConnections", "AssemblyVersion", c => c.String(nullable: false, defaultValue: "Unknown"));
		}

		public override void Down()
		{
			DropColumn("dbo.ActiveConnections", "AssemblyVersion");
		}
	}
}
