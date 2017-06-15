namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMembership : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Memberships",
                c => new
                    {
                        MemberId = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        CustomerId = c.String(),
                        CustomerCardId = c.String(),
                        NextPayment = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MemberId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Memberships");
        }
    }
}
