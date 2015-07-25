namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTranslationState : DbMigration
    {
        public override void Up()
        {
            AddColumn("linqua.Entries", "TranslationState", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("linqua.Entries", "TranslationState");
        }
    }
}
