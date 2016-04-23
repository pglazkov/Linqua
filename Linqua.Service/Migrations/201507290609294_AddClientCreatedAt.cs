namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddClientCreatedAt : DbMigration
    {
        public override void Up()
        {
            AddColumn("linqua.Entries", "ClientCreatedAt", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.UtcNow));

            Sql("UPDATE linqua.Entries SET ClientCreatedAt = CreatedAt");
        }

        public override void Down()
        {
            DropColumn("linqua.Entries", "ClientCreatedAt");
        }
    }
}