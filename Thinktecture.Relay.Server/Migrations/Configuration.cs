using System.Data.Entity.Migrations;

namespace Thinktecture.Relay.Server.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<Repository.RelayContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
			ContextKey = "Thinktecture.Relay.Server.Repository.RelayContext";
		}

		protected override void Seed(Repository.RelayContext context)
		{
			//  This method will be called after migrating to the latest version.

			//  You can use the DbSet<T>.AddOrUpdate() helper extension method 
			//  to avoid creating duplicate seed data. E.g.
			//
			//    context.People.AddOrUpdate(
			//      p => p.FullName,
			//      new Person { FullName = "Andrew Peters" },
			//      new Person { FullName = "Brice Lambson" },
			//      new Person { FullName = "Rowan Miller" }
			//    );
			//
		}
	}
}
