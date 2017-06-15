namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingCancelFieldtoMembership : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Memberships", "PendingCancel", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Memberships", "PendingCancel");
        }
    }
}
