namespace Linqua.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddMaxLengthToTextColumn : DbMigration
    {
        public override void Up()
        {
            AlterColumn("linqua.Entries", "Text", c => c.String(maxLength: 256));
        }

        public override void Down()
        {
            AlterColumn("linqua.Entries", "Text", c => c.String());
        }
    }
}