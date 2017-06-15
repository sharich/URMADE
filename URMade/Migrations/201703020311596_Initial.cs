namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Artists",
                c => new
                    {
                        ArtistId = c.Int(nullable: false, identity: true),
                        Slug = c.String(),
                        Name = c.String(),
                        ArtistType = c.String(),
                        BiographyShort = c.String(),
                        BiographyLong = c.String(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.ArtistId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        AccountType = c.String(),
                        Slug = c.String(),
                        PhotoURL = c.String(),
                        BiographyShort = c.String(),
                        BiographyLong = c.String(),
                        FavoriteSongsArePublic = c.Boolean(nullable: false),
                        FavoriteVideosArePublic = c.Boolean(nullable: false),
                        FavoriteArtistsArePublic = c.Boolean(nullable: false),
                        LoginDisabled = c.Boolean(nullable: false),
                        PasswordResetToken = c.String(),
                        PasswordResetTokenExpires = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Songs",
                c => new
                    {
                        SongId = c.Int(nullable: false, identity: true),
                        Artist_ArtistId = c.Int(),
                    })
                .PrimaryKey(t => t.SongId)
                .ForeignKey("dbo.Artists", t => t.Artist_ArtistId)
                .Index(t => t.Artist_ArtistId);
            
            CreateTable(
                "dbo.Videos",
                c => new
                    {
                        VideoId = c.Int(nullable: false, identity: true),
                        Artist_ArtistId = c.Int(),
                    })
                .PrimaryKey(t => t.VideoId)
                .ForeignKey("dbo.Artists", t => t.Artist_ArtistId)
                .Index(t => t.Artist_ArtistId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.SocialLinks",
                c => new
                    {
                        SocialLinkId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        URL = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        Artist_ArtistId = c.Int(),
                    })
                .PrimaryKey(t => t.SocialLinkId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.Artists", t => t.Artist_ArtistId)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Artist_ArtistId);
            
            CreateTable(
                "dbo.UserGroups",
                c => new
                    {
                        UserGroupId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PermissionGroup = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserGroupId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.SelectOptions",
                c => new
                    {
                        SelectOptionId = c.Int(nullable: false, identity: true),
                        OptionGroup = c.String(),
                        Value = c.String(),
                        Text = c.String(),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SelectOptionId);
            
            CreateTable(
                "dbo.UsersFavoriteSongs",
                c => new
                    {
                        Song_SongId = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Song_SongId, t.ApplicationUser_Id })
                .ForeignKey("dbo.Songs", t => t.Song_SongId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.Song_SongId)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.UsersFavoriteVideos",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Video_VideoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Video_VideoId })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Videos", t => t.Video_VideoId, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Video_VideoId);
            
            CreateTable(
                "dbo.UsersUserGroups",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        UserGroup_UserGroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.UserGroup_UserGroupId })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.UserGroups", t => t.UserGroup_UserGroupId, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.UserGroup_UserGroupId);
            
            CreateTable(
                "dbo.UsersFavoriteArtits",
                c => new
                    {
                        Artist_ArtistId = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Artist_ArtistId, t.ApplicationUser_Id })
                .ForeignKey("dbo.Artists", t => t.Artist_ArtistId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.Artist_ArtistId)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.UsersArtists",
                c => new
                    {
                        Artist_ArtistId = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Artist_ArtistId, t.ApplicationUser_Id })
                .ForeignKey("dbo.Artists", t => t.Artist_ArtistId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.Artist_ArtistId)
                .Index(t => t.ApplicationUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.SocialLinks", "Artist_ArtistId", "dbo.Artists");
            DropForeignKey("dbo.UsersArtists", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UsersArtists", "Artist_ArtistId", "dbo.Artists");
            DropForeignKey("dbo.UsersFavoriteArtits", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UsersFavoriteArtits", "Artist_ArtistId", "dbo.Artists");
            DropForeignKey("dbo.UsersUserGroups", "UserGroup_UserGroupId", "dbo.UserGroups");
            DropForeignKey("dbo.UsersUserGroups", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.SocialLinks", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UsersFavoriteVideos", "Video_VideoId", "dbo.Videos");
            DropForeignKey("dbo.UsersFavoriteVideos", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Videos", "Artist_ArtistId", "dbo.Artists");
            DropForeignKey("dbo.UsersFavoriteSongs", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UsersFavoriteSongs", "Song_SongId", "dbo.Songs");
            DropForeignKey("dbo.Songs", "Artist_ArtistId", "dbo.Artists");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UsersArtists", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UsersArtists", new[] { "Artist_ArtistId" });
            DropIndex("dbo.UsersFavoriteArtits", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UsersFavoriteArtits", new[] { "Artist_ArtistId" });
            DropIndex("dbo.UsersUserGroups", new[] { "UserGroup_UserGroupId" });
            DropIndex("dbo.UsersUserGroups", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UsersFavoriteVideos", new[] { "Video_VideoId" });
            DropIndex("dbo.UsersFavoriteVideos", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UsersFavoriteSongs", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UsersFavoriteSongs", new[] { "Song_SongId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.SocialLinks", new[] { "Artist_ArtistId" });
            DropIndex("dbo.SocialLinks", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.Videos", new[] { "Artist_ArtistId" });
            DropIndex("dbo.Songs", new[] { "Artist_ArtistId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropTable("dbo.UsersArtists");
            DropTable("dbo.UsersFavoriteArtits");
            DropTable("dbo.UsersUserGroups");
            DropTable("dbo.UsersFavoriteVideos");
            DropTable("dbo.UsersFavoriteSongs");
            DropTable("dbo.SelectOptions");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.UserGroups");
            DropTable("dbo.SocialLinks");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.Videos");
            DropTable("dbo.Songs");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Artists");
        }
    }
}
