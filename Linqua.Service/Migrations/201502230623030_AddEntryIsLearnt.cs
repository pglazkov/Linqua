namespace Linqua.Service.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddEntryIsLearnt : DbMigration
    {
        public override void Up()
        {
            AddColumn("linqua.Entries", "IsLearnt", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("linqua.Entries", "IsLearnt");
        }
    }
}