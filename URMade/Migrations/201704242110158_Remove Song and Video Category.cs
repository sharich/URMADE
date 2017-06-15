namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSongandVideoCategory : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Songs", "Category");
            DropColumn("dbo.Videos", "Category");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Videos", "Category", c => c.String());
            AddColumn("dbo.Songs", "Category", c => c.String());
        }
    }
}
