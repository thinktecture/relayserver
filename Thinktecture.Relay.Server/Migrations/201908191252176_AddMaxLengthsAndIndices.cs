namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMaxLengthsAndIndices : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Links", "Password", c => c.String(nullable: false, maxLength: 32));
            AlterColumn("dbo.Links", "Salt", c => c.String(nullable: false, maxLength: 32));
            AlterColumn("dbo.RequestLogEntries", "OnPremiseTargetKey", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.RequestLogEntries", "LocalUrl", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 32));
            AlterColumn("dbo.Users", "Salt", c => c.String(nullable: false, maxLength: 32));
            CreateIndex("dbo.RequestLogEntries", "HttpStatusCode");
            CreateIndex("dbo.RequestLogEntries", "OnPremiseTargetKey");
            CreateIndex("dbo.RequestLogEntries", "LocalUrl");
            CreateIndex("dbo.RequestLogEntries", "ContentBytesIn");
            CreateIndex("dbo.RequestLogEntries", "ContentBytesOut");
            CreateIndex("dbo.RequestLogEntries", "OnPremiseConnectorInDate");
            CreateIndex("dbo.RequestLogEntries", "OnPremiseConnectorOutDate");
            CreateIndex("dbo.RequestLogEntries", "OnPremiseTargetInDate");
            CreateIndex("dbo.RequestLogEntries", "OnPremiseTargetOutDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.RequestLogEntries", new[] { "OnPremiseTargetOutDate" });
            DropIndex("dbo.RequestLogEntries", new[] { "OnPremiseTargetInDate" });
            DropIndex("dbo.RequestLogEntries", new[] { "OnPremiseConnectorOutDate" });
            DropIndex("dbo.RequestLogEntries", new[] { "OnPremiseConnectorInDate" });
            DropIndex("dbo.RequestLogEntries", new[] { "ContentBytesOut" });
            DropIndex("dbo.RequestLogEntries", new[] { "ContentBytesIn" });
            DropIndex("dbo.RequestLogEntries", new[] { "LocalUrl" });
            DropIndex("dbo.RequestLogEntries", new[] { "OnPremiseTargetKey" });
            DropIndex("dbo.RequestLogEntries", new[] { "HttpStatusCode" });
            AlterColumn("dbo.Users", "Salt", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Password", c => c.String(nullable: false));
            AlterColumn("dbo.RequestLogEntries", "LocalUrl", c => c.String(nullable: false));
            AlterColumn("dbo.RequestLogEntries", "OnPremiseTargetKey", c => c.String(nullable: false));
            AlterColumn("dbo.Links", "Salt", c => c.String(nullable: false));
            AlterColumn("dbo.Links", "Password", c => c.String(nullable: false));
        }
    }
}
