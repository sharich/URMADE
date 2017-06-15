namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VideoSongOwner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "Owner_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Videos", "Owner_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Songs", "Owner_Id");
            CreateIndex("dbo.Videos", "Owner_Id");
            AddForeignKey("dbo.Songs", "Owner_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Videos", "Owner_Id", "dbo.AspNetUsers", "Id");
            DropColumn("dbo.Songs", "UploaderId");
            DropColumn("dbo.Videos", "UploaderId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Videos", "UploaderId", c => c.String());
            AddColumn("dbo.Songs", "UploaderId", c => c.String());
            DropForeignKey("dbo.Videos", "Owner_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Songs", "Owner_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Videos", new[] { "Owner_Id" });
            DropIndex("dbo.Songs", new[] { "Owner_Id" });
            DropColumn("dbo.Videos", "Owner_Id");
            DropColumn("dbo.Songs", "Owner_Id");
        }
    }
}
