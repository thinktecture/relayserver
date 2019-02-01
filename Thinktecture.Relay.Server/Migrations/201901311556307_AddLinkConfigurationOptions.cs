namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLinkConfigurationOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Links", "TokenRefreshWindow", c => c.Time(precision: 7));
            AddColumn("dbo.Links", "HeartbeatInterval", c => c.Time(precision: 7));
            AddColumn("dbo.Links", "ReconnectMinWaitTime", c => c.Time(precision: 7));
            AddColumn("dbo.Links", "ReconnectMaxWaitTime", c => c.Time(precision: 7));
            AddColumn("dbo.Links", "AbsoluteConnectionLifetime", c => c.Time(precision: 7));
            AddColumn("dbo.Links", "SlidingConnectionLifetime", c => c.Time(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Links", "SlidingConnectionLifetime");
            DropColumn("dbo.Links", "AbsoluteConnectionLifetime");
            DropColumn("dbo.Links", "ReconnectMaxWaitTime");
            DropColumn("dbo.Links", "ReconnectMinWaitTime");
            DropColumn("dbo.Links", "HeartbeatInterval");
            DropColumn("dbo.Links", "TokenRefreshWindow");
        }
    }
}
