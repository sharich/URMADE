namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaProcessing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "State", c => c.Int(nullable: false));
            AddColumn("dbo.Songs", "JobId", c => c.String());
            AddColumn("dbo.Songs", "EncodedAssetId", c => c.String());
            AddColumn("dbo.Songs", "SmoothStreamingUri", c => c.String());
            AddColumn("dbo.Songs", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Videos", "State", c => c.Int(nullable: false));
            AddColumn("dbo.Videos", "VideoBlobURL", c => c.String());
            AddColumn("dbo.Videos", "DateCreated", c => c.DateTime(nullable: false));
            DropColumn("dbo.Songs", "AudioURL");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Songs", "AudioURL", c => c.String());
            DropColumn("dbo.Videos", "DateCreated");
            DropColumn("dbo.Videos", "VideoBlobURL");
            DropColumn("dbo.Videos", "State");
            DropColumn("dbo.Songs", "DateCreated");
            DropColumn("dbo.Songs", "SmoothStreamingUri");
            DropColumn("dbo.Songs", "EncodedAssetId");
            DropColumn("dbo.Songs", "JobId");
            DropColumn("dbo.Songs", "State");
        }
    }
}
