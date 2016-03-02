namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequestLogEntriesChartIndex : DbMigration
    {
        public override void Up()
        {
            Sql(@"
CREATE NONCLUSTERED INDEX [ChartIndex] ON [dbo].[RequestLogEntries]
(
	[OnPremiseConnectorInDate] ASC,
	[OnPremiseConnectorOutDate] ASC,
    [LinkId] ASC
)
INCLUDE ( 	[ContentBytesIn],
	[ContentBytesOut]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
");
        }
        
        public override void Down()
        {
            Sql(@"
DROP INDEX [NonClusteredIndex-20160125-133848] ON [dbo].[RequestLogEntries]
");
        }
    }
}
