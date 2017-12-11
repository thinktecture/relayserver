namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAccountLockoutData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "LastFailedLoginAttempt", c => c.DateTime());
            AddColumn("dbo.Users", "FailedLoginAttempts", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "FailedLoginAttempts");
            DropColumn("dbo.Users", "LastFailedLoginAttempt");
        }
    }
}
