namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserEmail : DbMigration
    {
        public override void Up()
        {
            DropColumn("linqua.Entries", "UserEmail");
        }
        
        public override void Down()
        {
            AddColumn("linqua.Entries", "UserEmail", c => c.String(maxLength: 256));
        }
    }
}
