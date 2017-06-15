using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace URMade.Models
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Artist> Artists { get; set; }
        public DbSet<SocialLink> SocialLinks { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Video> Videos { get; set; }
		public DbSet<ArtistSlideshowImage> ArtistSlideshowImages { get; set; }
		public DbSet<Contest> Contests { get; set; }
        public DbSet<ContestEntry> ContestsEntries { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<SelectOption> SelectOptions { get; set; }
		public DbSet<Membership> Memberships {get; set;}

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Add(new NonPublicColumnAttributeConvention());

            // Configure join table names
            modelBuilder.Entity<ApplicationUser>()
            .HasMany(p => p.UserGroups)
            .WithMany(r => r.Users)
            .Map(mc =>
            {
                mc.ToTable("UsersUserGroups");
            });

            modelBuilder.Entity<ApplicationUser>()
            .HasMany(p => p.Artists)
            .WithMany(r => r.Members)
            .Map(mc =>
            {
                mc.ToTable("UsersArtists");
            });

            modelBuilder.Entity<ApplicationUser>()
            .HasMany(p => p.FavoriteVideos)
            .WithMany(r => r.Fans)
            .Map(mc =>
            {
                mc.ToTable("UsersFavoriteVideos");
            });
            modelBuilder.Entity<ApplicationUser>()
            .HasMany(p => p.FavoriteSongs)
            .WithMany(r => r.Fans)
            .Map(mc =>
            {
                mc.ToTable("UsersFavoriteSongs");
            });
            modelBuilder.Entity<ApplicationUser>()
            .HasMany(p => p.FavoriteArtists)
            .WithMany(r => r.Fans)
            .Map(mc =>
            {
                mc.ToTable("UsersFavoriteArtits");
            });
            modelBuilder.Entity<ContestEntry>()
            .HasMany(p => p.Voters)
            .WithMany(r => r.ContestVotes)
            .Map(mc =>
            {
                mc.ToTable("ContestVotes");
            });
        }

    }

    public class UserGroup
    {
        [Key]
        public int UserGroupId { get; set; }

        public string Name { get; set; }

        public PermissionGroup PermissionGroup { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        
        public bool HasPermission(params Permission[] permissions)
        {
            return PermissionHelper.The(PermissionGroup).Has(permissions);
        }

        public bool HasPermissionAny(params Permission[] permissions)
        {
            return PermissionHelper.The(PermissionGroup).HasAny(permissions);
        }

        public IEnumerable<string> GetPermissionNames()
        {
            return PermissionHelper.The(PermissionGroup).PermissionNames();
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return PermissionHelper.The(PermissionGroup).PermissionList();
        }

        public IEnumerable<string> GetPermissionGroupNames()
        {
            return PermissionHelper.The(PermissionGroup).GroupNames();
        }

        public IEnumerable<PermissionGroup> GetPermissionGroups()
        {
            return PermissionHelper.The(PermissionGroup).ToList();
        }
    }

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.FavoriteArtists = new HashSet<Artist>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public string Name { get; set; }

        [Display(Name = "Account Type")]
        public string AccountType { get; set; }

        [Display(Name = "Profile URL")]
        public string Slug { get; set; }

        [Display(Name = "Profile Photo")]
        public string PhotoURL { get; set; }

        [Display(Name="Social Links")]
        public virtual ICollection<SocialLink> SocialLinks { get; set; }

        [Display(Name = "Biography")]
        public string BiographyShort { get; set; }
        [Display(Name = "Biography")]
        public string BiographyLong { get; set; }

        [Display(Name = "My favorite songs are visible to the public")]
        public bool FavoriteSongsArePublic { get; set; }
        [Display(Name = "My favorite videos are visible to the public")]
        public bool FavoriteVideosArePublic { get; set; }
        [Display(Name = "My favorite artists are visible to the public")]
        public bool FavoriteArtistsArePublic { get; set; }

        public virtual ICollection<Artist> Artists { get; set; }

        public virtual ICollection<Song> FavoriteSongs { get; set; }
        public virtual ICollection<Video> FavoriteVideos { get; set; }
        public virtual ICollection<Artist> FavoriteArtists { get; set; }

        public virtual ICollection<ContestEntry> ContestVotes { get; set; }

        public virtual ICollection<UserGroup> UserGroups { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<Video> Videos { get; set; }

        public bool LoginDisabled { get; set; }

        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }



        public bool HasPermission(params Permission[] permissions)
        {
            return PermissionHelper.The(UserGroups.PermissionGroups()).Have(permissions);
        }

        public bool HasPermissionAny(params Permission[] permissions)
        {
            return PermissionHelper.The(UserGroups.PermissionGroups()).HasAny(permissions);
        }

        public List<String> GetPermissionGroupNames()
        {
            return PermissionHelper.The(UserGroups.PermissionGroups()).GroupNames().ToList();
        }

        public PermissionGroup PermissionGroup()
        {
            return PermissionHelper.The(UserGroups.PermissionGroups()).ToList().MakeFlagset();
        }

		public bool IsPremiumMember()
		{
			return HasPermission(Permission.CanPublishUnlimitedSongs);
		}

        public bool IsMyArtists(string slug)
        {
            return Artists.Any(p => p.Slug == slug);
        }

		public bool IsMyArtists(int id)
		{
			return Artists.Any(p => p.ArtistId == id);
		}
    }

	/// <summary>
	/// Convention to support binding private or protected properties to EF columns.
	/// </summary>
	public sealed class NonPublicColumnAttributeConvention : Convention
    {
        public NonPublicColumnAttributeConvention()
        {
            Types().Having(NonPublicProperties)
                   .Configure((config, properties) =>
                   {
                       foreach (PropertyInfo prop in properties)
                       {
                           config.Property(prop);
                       }
                   });
        }

        private IEnumerable<PropertyInfo> NonPublicProperties(Type type)
        {
            var matchingProperties = type.GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance)
                                         .Where(propInfo => propInfo.GetCustomAttributes(typeof(ColumnAttribute), true).Length > 0)
                                         .ToArray();
            return matchingProperties.Length == 0 ? null : matchingProperties;
        }
    }

}