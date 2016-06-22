namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserMicrosoftAccountIdUniqueIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("linqua.Users", new[] { "MicrosoftAccountId" });
            CreateIndex("linqua.Users", "MicrosoftAccountId", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("linqua.Users", new[] { "MicrosoftAccountId" });
            CreateIndex("linqua.Users", "MicrosoftAccountId");
        }
    }
}
