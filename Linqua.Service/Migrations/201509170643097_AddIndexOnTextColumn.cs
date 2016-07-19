namespace Linqua.Service.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddIndexOnTextColumn : DbMigration
    {
        public override void Up()
        {
            CreateIndex("linqua.Entries", "Text");
        }

        public override void Down()
        {
            DropIndex("linqua.Entries", new[] {"Text"});
        }
    }
}