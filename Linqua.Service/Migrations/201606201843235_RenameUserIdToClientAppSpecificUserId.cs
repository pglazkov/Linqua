namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameUserIdToClientAppSpecificUserId : DbMigration
    {
        public override void Up()
        {
            RenameColumn("linqua.Entries", "UserId", "ClientAppSpecificUserId");
        }
        
        public override void Down()
        {
            RenameColumn("linqua.Entries", "ClientAppSpecificUserId", "UserId");
        }
    }
}
