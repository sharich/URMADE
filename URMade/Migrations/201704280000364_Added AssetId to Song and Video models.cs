namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAssetIdtoSongandVideomodels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "AssetId", c => c.String());
            AddColumn("dbo.Videos", "AssetId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Videos", "AssetId");
            DropColumn("dbo.Songs", "AssetId");
        }
    }
}
