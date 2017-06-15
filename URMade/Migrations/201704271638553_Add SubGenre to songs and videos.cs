namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubGenretosongsandvideos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "SubGenre", c => c.String());
            AddColumn("dbo.Videos", "UploaderId", c => c.String());
            AddColumn("dbo.Videos", "SubGenre", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Videos", "SubGenre");
            DropColumn("dbo.Videos", "UploaderId");
            DropColumn("dbo.Songs", "SubGenre");
        }
    }
}
