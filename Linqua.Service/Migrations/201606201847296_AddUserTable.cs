namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "linqua.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        MicrosoftAccountId = c.String(maxLength: 256),
                        Email = c.String(maxLength: 512),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.MicrosoftAccountId);
            
            AddColumn("linqua.Entries", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("linqua.Entries", "User_Id");
            AddForeignKey("linqua.Entries", "User_Id", "linqua.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("linqua.Entries", "User_Id", "linqua.Users");
            DropIndex("linqua.Users", new[] { "MicrosoftAccountId" });
            DropIndex("linqua.Entries", new[] { "User_Id" });
            DropColumn("linqua.Entries", "User_Id");
            DropTable("linqua.Users");
        }
    }
}
