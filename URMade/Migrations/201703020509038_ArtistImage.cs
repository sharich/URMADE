namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArtistImage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArtistSlideshowImages",
                c => new
                    {
                        ArtistSlideshowImageId = c.Int(nullable: false, identity: true),
                        ImageURL = c.String(),
                        Artist_ArtistId = c.Int(),
                    })
                .PrimaryKey(t => t.ArtistSlideshowImageId)
                .ForeignKey("dbo.Artists", t => t.Artist_ArtistId)
                .Index(t => t.Artist_ArtistId);
            
            AddColumn("dbo.Artists", "ImageURL", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ArtistSlideshowImages", "Artist_ArtistId", "dbo.Artists");
            DropIndex("dbo.ArtistSlideshowImages", new[] { "Artist_ArtistId" });
            DropColumn("dbo.Artists", "ImageURL");
            DropTable("dbo.ArtistSlideshowImages");
        }
    }
}
