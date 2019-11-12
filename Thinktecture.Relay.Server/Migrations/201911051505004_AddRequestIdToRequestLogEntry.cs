namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestIdToRequestLogEntry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestLogEntries", "RequestId", c => c.String(maxLength: 36));
            CreateIndex("dbo.RequestLogEntries", "RequestId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.RequestLogEntries", new[] { "RequestId" });
            DropColumn("dbo.RequestLogEntries", "RequestId");
        }
    }
}
