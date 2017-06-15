namespace URMade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedContestFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Contests", "ImageURL", c => c.String());
            AddColumn("dbo.Contests", "Subtitle", c => c.String());
            AddColumn("dbo.Contests", "Description", c => c.String());
            AddColumn("dbo.Contests", "Rules", c => c.String());
            AddColumn("dbo.Contests", "CSSClassName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Contests", "CSSClassName");
            DropColumn("dbo.Contests", "Rules");
            DropColumn("dbo.Contests", "Description");
            DropColumn("dbo.Contests", "Subtitle");
            DropColumn("dbo.Contests", "ImageURL");
        }
    }
}
