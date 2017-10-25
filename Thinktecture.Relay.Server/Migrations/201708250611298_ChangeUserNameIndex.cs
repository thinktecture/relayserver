namespace Thinktecture.Relay.Server.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class ChangeUserNameIndex : DbMigration
	{
		public override void Up()
		{
			Sql(@"
DROP INDEX [UserNameIndex] ON [dbo].[Links]

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[Links]
(
	[UserName] ASC
)
INCLUDE
(
	[Id],
	[SymbolicName],
	[IsDisabled],
	[ForwardOnPremiseTargetErrorResponse],
	[AllowLocalClientRequestsOnly]
)");
		}

		public override void Down()
		{
			Sql(@"
DROP INDEX [UserNameIndex] ON [dbo].[Links];

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[Links] (
	[UserName] ASC
)
");
		}
	}
}
