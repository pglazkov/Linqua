namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexOnUserId : DbMigration
    {
        public override void Up()
        {
            AlterColumn("linqua.Entries", "UserId", c => c.String(maxLength: 256));
            CreateIndex("linqua.Entries", "UserId");
        }
        
        public override void Down()
        {
            DropIndex("linqua.Entries", new[] { "UserId" });
            AlterColumn("linqua.Entries", "UserId", c => c.String());
        }
    }
}
