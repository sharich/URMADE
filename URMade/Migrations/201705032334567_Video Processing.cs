namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VideoProcessing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Videos", "JobId", c => c.String());
            AddColumn("dbo.Videos", "EncodedAssetId", c => c.String());
            AddColumn("dbo.Videos", "SmoothStreamingUri", c => c.String());
            DropColumn("dbo.Videos", "VideoBlobURL");
            DropColumn("dbo.Videos", "VideoURL");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Videos", "VideoURL", c => c.String());
            AddColumn("dbo.Videos", "VideoBlobURL", c => c.String());
            DropColumn("dbo.Videos", "SmoothStreamingUri");
            DropColumn("dbo.Videos", "EncodedAssetId");
            DropColumn("dbo.Videos", "JobId");
        }
    }
}
