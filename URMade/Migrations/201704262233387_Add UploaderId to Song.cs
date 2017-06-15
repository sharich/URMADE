namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUploaderIdtoSong : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "UploaderId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Songs", "UploaderId");
        }
    }
}
