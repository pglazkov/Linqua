namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUsersTableAndRelatedChanges : DbMigration
    {
        public override void Up()
        {
            RenameColumn("linqua.Entries", "UserId", "ClientAppSpecificUserId");

            CreateTable(
                "linqua.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        MicrosoftAccountId = c.String(maxLength: 256),
                        Email = c.String(maxLength: 512),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.MicrosoftAccountId, unique: true);
            
            
            AddColumn("linqua.Entries", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("linqua.Entries", "User_Id");
            AddForeignKey("linqua.Entries", "User_Id", "linqua.Users", "Id");

            DropColumn("linqua.Entries", "UserEmail");
        }
        
        public override void Down()
        {
            AddColumn("linqua.Entries", "UserEmail", c => c.String(maxLength: 256));

            RenameColumn("linqua.Entries", "ClientAppSpecificUserId", "UserId");

            DropForeignKey("linqua.Entries", "User_Id", "linqua.Users");
            DropIndex("linqua.Users", new[] { "MicrosoftAccountId" });
            DropIndex("linqua.Entries", new[] { "User_Id" });
            DropColumn("linqua.Entries", "User_Id");
            DropTable("linqua.Users");
        }
    }
}
