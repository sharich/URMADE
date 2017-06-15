namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContestEntry : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContestEntries",
                c => new
                    {
                        EntryId = c.Int(nullable: false, identity: true),
                        EntryPricePaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Contest_ContestId = c.Int(),
                        Song_SongId = c.Int(),
                        Video_VideoId = c.Int(),
                    })
                .PrimaryKey(t => t.EntryId)
                .ForeignKey("dbo.Contests", t => t.Contest_ContestId)
                .ForeignKey("dbo.Songs", t => t.Song_SongId)
                .ForeignKey("dbo.Videos", t => t.Video_VideoId)
                .Index(t => t.Contest_ContestId)
                .Index(t => t.Song_SongId)
                .Index(t => t.Video_VideoId);
            
            CreateTable(
                "dbo.ContestVotes",
                c => new
                    {
                        ContestEntry_EntryId = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ContestEntry_EntryId, t.ApplicationUser_Id })
                .ForeignKey("dbo.ContestEntries", t => t.ContestEntry_EntryId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.ContestEntry_EntryId)
                .Index(t => t.ApplicationUser_Id);
            
            AddColumn("dbo.Contests", "EntryPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Contests", "MemberEntryPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ContestVotes", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ContestVotes", "ContestEntry_EntryId", "dbo.ContestEntries");
            DropForeignKey("dbo.ContestEntries", "Video_VideoId", "dbo.Videos");
            DropForeignKey("dbo.ContestEntries", "Song_SongId", "dbo.Songs");
            DropForeignKey("dbo.ContestEntries", "Contest_ContestId", "dbo.Contests");
            DropIndex("dbo.ContestVotes", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.ContestVotes", new[] { "ContestEntry_EntryId" });
            DropIndex("dbo.ContestEntries", new[] { "Video_VideoId" });
            DropIndex("dbo.ContestEntries", new[] { "Song_SongId" });
            DropIndex("dbo.ContestEntries", new[] { "Contest_ContestId" });
            DropColumn("dbo.Contests", "MemberEntryPrice");
            DropColumn("dbo.Contests", "EntryPrice");
            DropTable("dbo.ContestVotes");
            DropTable("dbo.ContestEntries");
        }
    }
}
