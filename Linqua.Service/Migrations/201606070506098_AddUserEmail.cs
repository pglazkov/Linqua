namespace Linqua.Service.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddUserEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("linqua.Entries", "UserEmail", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("linqua.Entries", "UserEmail");
        }
    }
}
