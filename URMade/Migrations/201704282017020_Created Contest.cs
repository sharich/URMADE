namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedContest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Contests",
                c => new
                    {
                        ContestId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Type = c.Int(nullable: false),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        HomeBannerURL = c.String(),
                    })
                .PrimaryKey(t => t.ContestId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Contests");
        }
    }
}
