namespace Linqua.Service.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddUsersTableAndRelatedChanges : DbMigration
    {
        public override void Up()
        {
            RenameColumn("linqua.Entries", "UserId", "ClientAppSpecificUserId");
            RenameIndex("linqua.Entries", "IX_UserId", "IX_ClientAppSpecificUserId");

            CreateTable(
                "linqua.Users",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MicrosoftAccountId = c.String(maxLength: 256),
                        Email = c.String(maxLength: 512),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.MicrosoftAccountId, unique: true);
            
            
            AddColumn("linqua.Entries", "UserId", c => c.Guid(nullable: true));
            AddForeignKey("linqua.Entries", "UserId", "linqua.Users", "Id");

            DropColumn("linqua.Entries", "UserEmail");
        }
        
        public override void Down()
        {
            AddColumn("linqua.Entries", "UserEmail", c => c.String(maxLength: 256));

            DropForeignKey("linqua.Entries", "UserId", "linqua.Users");
            DropColumn("linqua.Entries", "UserId");

            RenameColumn("linqua.Entries", "ClientAppSpecificUserId", "UserId");
            RenameIndex("linqua.Entries", "IX_ClientAppSpecificUserId", "IX_UserId");
            
            DropIndex("linqua.Users", new[] { "MicrosoftAccountId" });
            
            DropTable("linqua.Users");
        }
    }
}
