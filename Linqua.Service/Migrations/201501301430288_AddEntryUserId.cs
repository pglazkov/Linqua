namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEntryUserId : DbMigration
    {
        public override void Up()
        {
            AddColumn("linqua.Entries", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("linqua.Entries", "UserId");
        }
    }
}
