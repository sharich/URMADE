namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SongVideoDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "Title", c => c.String());
            AddColumn("dbo.Songs", "Category", c => c.String());
            AddColumn("dbo.Songs", "AlbumArtURL", c => c.String());
            AddColumn("dbo.Songs", "AudioURL", c => c.String());
            AddColumn("dbo.Songs", "FanCount", c => c.Int(nullable: false));
            AddColumn("dbo.Videos", "Title", c => c.String());
            AddColumn("dbo.Videos", "Category", c => c.String());
            AddColumn("dbo.Videos", "AlbumArtURL", c => c.String());
            AddColumn("dbo.Videos", "VideoURL", c => c.String());
            AddColumn("dbo.Videos", "FanCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Videos", "FanCount");
            DropColumn("dbo.Videos", "VideoURL");
            DropColumn("dbo.Videos", "AlbumArtURL");
            DropColumn("dbo.Videos", "Category");
            DropColumn("dbo.Videos", "Title");
            DropColumn("dbo.Songs", "FanCount");
            DropColumn("dbo.Songs", "AudioURL");
            DropColumn("dbo.Songs", "AlbumArtURL");
            DropColumn("dbo.Songs", "Category");
            DropColumn("dbo.Songs", "Title");
        }
    }
}
