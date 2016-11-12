namespace JHernandezHPE.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using JHernandezHPE.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<JHernandezHPE.Models.JHernandezHPEContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(JHernandezHPE.Models.JHernandezHPEContext context)
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
            context.Players.AddOrUpdate(p => p.Username,
            new Player
            {
                Username = "jhernandez",
                Points = 9,
            },
            new Player
            {
                Username = "askywalker",
                Points = 9,
            },
            new Player
            {
                Username = "bwayne",
                Points = 9,
            },
            new Player
            {
                Username = "tstark",
                Points = 9,
            }
            );

        }
    }
}
