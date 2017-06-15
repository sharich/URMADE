namespace URMade.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<URMade.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(URMade.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

			bool isChanged = false;

			if (context.SelectOptions.ToArray().Length < 1)
			{
				context.SelectOptions.AddOrUpdate(
					// Account Type
					new Models.SelectOption() {OptionGroup = "Account Type",	Text = "Listener",		Value = "Listener"},
					new Models.SelectOption() {OptionGroup = "Account Type",	Text = "Band",			Value = "Band"},
					new Models.SelectOption() {OptionGroup = "Account Type",	Text = "SongWriter",	Value = "SongWriter"},
					// Social Links
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Facebook",		Value = "facebook"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Instagram",		Value = "instagram"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Twitter",		Value = "twitter"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "YouTube",		Value = "youtube"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Google",		Value = "googleplus"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Tumblr",		Value = "tumblr"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Skype",			Value = "skype"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "iTunes",		Value = "itunes"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "SoundCloud",	Value = "soundcloud"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Spotify",		Value = "spotify"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "PayPal",		Value = "paypal"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Amazon",		Value = "amazon"},
					new Models.SelectOption() {OptionGroup = "Social Links",	Text = "Email",			Value = "mail"},
					// Genres
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Country",		Value = "country"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Country/Pop",	Value = "countrypop"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Country/Rock",	Value = "countryrock"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Blues",			Value = "blues"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Southern Rock",	Value = "southernrock"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Country/Alt",	Value = "countryalt"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Country/Indie",	Value = "countryindie"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Folk",			Value = "folk"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Old School",	Value = "oldschool"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Bluegrass",		Value = "bluegrass"},
					new Models.SelectOption() {OptionGroup = "Genre",			Text = "Americana",		Value = "americana"}
					);

				isChanged = true;
			}

			// ================================================================================
			// Standard User
			// ================================================================================

			if (!context.UserGroups.Any(p => p.Name == "Standard"))
			{
				context.UserGroups.Add(new Models.UserGroup()
				{
					Name			= "Standard",
					PermissionGroup	= PermissionGroup.EditMyArtists			|
									  PermissionGroup.EditMyUser			|
									  PermissionGroup.CanFavoriteAndVote
				});
				isChanged = true;
			}

			if (!context.UserGroups.Any(p => p.Name == "Premium"))
			{
				context.UserGroups.Add(new Models.UserGroup()
				{
					Name			= "Premium",
					PermissionGroup	= PermissionGroup.EditMyArtists			|
									  PermissionGroup.EditMyUser			|
									  PermissionGroup.CanFavoriteAndVote	|
									  PermissionGroup.PremiumArtist
				});
				isChanged = true;
			}

			if (isChanged)
				context.SaveChanges();
        }
    }
}
