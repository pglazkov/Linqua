namespace Linqua.Service.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddLanguageCodes : DbMigration
    {
        public override void Up()
        {
            AddColumn("linqua.Entries", "TextLanguageCode", c => c.String(defaultValue: "nl"));
            AddColumn("linqua.Entries", "DefinitionLanguageCode", c => c.String(defaultValue: "en"));
        }

        public override void Down()
        {
            DropColumn("linqua.Entries", "DefinitionLanguageCode");
            DropColumn("linqua.Entries", "TextLanguageCode");
        }
    }
}