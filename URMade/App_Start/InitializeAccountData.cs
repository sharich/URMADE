using URMade.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace URMade
{
    public class InitializeAccountData
    {
        public static void InitializeDatabase()
        {
            var context = DependencyResolver.Current.GetService<ApplicationDbContext>();

            if (context.UserGroups != null && context.UserGroups.Count() == 0)
            {
				// ================================================================================
				// Standard User
				// ================================================================================

				context.UserGroups.Add(new UserGroup()
				{
					Name			= "Standard",
					PermissionGroup	= PermissionGroup.EditMyArtists			|
									  PermissionGroup.EditMyUser			|
									  PermissionGroup.CanFavoriteAndVote
				});

				// ================================================================================
				// Premium User
				// ================================================================================

				context.UserGroups.Add(new UserGroup()
				{
					Name			= "Premium",
					PermissionGroup	= PermissionGroup.EditMyArtists			|
									  PermissionGroup.EditMyUser			|
									  PermissionGroup.CanFavoriteAndVote	|
									  PermissionGroup.PremiumArtist
				});

				// ================================================================================
				// Administrator
				// ================================================================================

                context.UserGroups.Add(new UserGroup()
                {
                    Name			= "Admins",
                    PermissionGroup = PermissionGroup.EditArtists			|
									  PermissionGroup.EditMyArtists			|
									  PermissionGroup.EditMyUser			|
									  PermissionGroup.EditSelectOptions		|
									  PermissionGroup.EditUserGroups		|
									  PermissionGroup.EditUsers				|
									  PermissionGroup.Impersonate			|
									  PermissionGroup.MultipleArtists		|
									  PermissionGroup.CanFavoriteAndVote	|
									  PermissionGroup.PremiumArtist
                });

				// ================================================================================
				// Developer
				// ================================================================================

                context.UserGroups.Add(new UserGroup()
                {
                    Name			= "Developers",
                    PermissionGroup = PermissionGroup.EditArtists			|
									  PermissionGroup.EditMyArtists			|
									  PermissionGroup.EditMyUser			|
									  PermissionGroup.EditSelectOptions		|
									  PermissionGroup.EditUserGroups		|
									  PermissionGroup.EditUsers				|
									  PermissionGroup.Impersonate			|
									  PermissionGroup.MultipleArtists		|
									  PermissionGroup.CanFavoriteAndVote	|
									  PermissionGroup.PremiumArtist
                });

                context.SaveChanges();
            }

            if (Settings.DeveloperAccountEmails.Count() > 0)
            {
                UserGroup developerGroup = context.UserGroups.FirstOrDefault(p => p.Name == "Developers");

                if (developerGroup != null)
                {
                    if (developerGroup.Users == null)
						developerGroup.Users = new List<ApplicationUser>();

                    var usersToAdd = new List<string>();

                    foreach (string email in Settings.DeveloperAccountEmails)
                    {
                        var user = context.Users.FirstOrDefault(p => p.Email == email);

                        if (user == null)
                            usersToAdd.Add(email);
                        else
                            developerGroup.Users.Add(user);
                    }


                    if (usersToAdd.Count() > 0)
                    {
                        var userRepo = DependencyResolver.Current.GetService<UserRepository>();

                        foreach (string email in usersToAdd)
                        {
                            var user = userRepo.CreateUserAndSave("Developer", email, email);

                            if (user != null)
								developerGroup.Users.Add(user);
                        }
                    }

                    context.SaveChanges();
                }
            }
        }
    }
}