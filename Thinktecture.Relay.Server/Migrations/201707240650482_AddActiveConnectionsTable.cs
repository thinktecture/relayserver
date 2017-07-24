namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddActiveConnectionsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActiveConnections",
                c => new
                    {
                        LinkId = c.Guid(nullable: false),
                        ConnectionId = c.String(nullable: false, maxLength: 128),
                        OriginId = c.Guid(nullable: false),
                        ConnectorVersion = c.Int(nullable: false),
                        LastActivity = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.LinkId, t.ConnectionId, t.OriginId })
                .ForeignKey("dbo.Links", t => t.LinkId, cascadeDelete: true)
                .Index(t => t.LinkId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActiveConnections", "LinkId", "dbo.Links");
            DropIndex("dbo.ActiveConnections", new[] { "LinkId" });
            DropTable("dbo.ActiveConnections");
        }
    }
}
