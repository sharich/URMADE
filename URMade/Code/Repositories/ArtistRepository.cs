using URMade.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace URMade
{
    public class ArtistRepository
    {
        private ApplicationDbContext Context { get; set; }

        public ArtistRepository(ApplicationDbContext context)
        {
            Context = context;
        }

		public Artist GetArtist(int id)
		{
			return Context.Artists.FirstOrDefault(p => p.ArtistId == id);
		}

		public PlaylistModel GetTopArtistsPlaylist(string type, string TitleMatch)
		{
			return new PlaylistModel(Context.Artists, new PlaylistQuery() {Type = type, Name = TitleMatch});
		}

		public ArtistIndexViewModel GetArtistIndexViewModel()
		{
			return new ArtistIndexViewModel(Context.Artists);
		}

		public ArtistViewModel GetArtistViewModel(string Slug)
        {
            Artist artist			= Context.Artists.FirstOrDefault(p => p.Slug == Slug);
			ApplicationUser user	= artist != null ? artist.Members.First() : null;

            if (user == null)
				return new ArtistViewModel();

			PlaylistQuery query = new PlaylistQuery() {ArtistId = artist.ArtistId};
			return new ArtistViewModel(artist, user, new PlaylistModel(Context.Songs, query), new PlaylistModel(Context.Videos, query), new PlaylistModel(Context.ContestsEntries, query));
        }

		public ArtistViewModel GetArtistViewModel(int ArtistId)
        {
            Artist artist			= Context.Artists.FirstOrDefault(p => p.ArtistId == ArtistId);
			ApplicationUser user	= artist != null ? artist.Members.First() : null;

            if (user == null)
				return new ArtistViewModel();

			PlaylistQuery query = new PlaylistQuery() {ArtistId = artist.ArtistId};
			return new ArtistViewModel(artist, user, new PlaylistModel(Context.Songs, query), new PlaylistModel(Context.Videos, query), new PlaylistModel(Context.ContestsEntries, query));
        }

		public List<SearchItemViewModel> Search(string match, int max)
		{
			List<Artist> artists = Context.Artists.Where(p => p.Name.Contains(match)).OrderByDescending(p => p.Fans.Count).Take(max).ToList();

			if (artists == null || artists.Count < 1)
				return null;

			List<SearchItemViewModel> result = new List<SearchItemViewModel>();
			foreach (Artist artist in artists)
				result.Add(new SearchItemViewModel(artist));

			return result;
		}

		public List<FeaturedArtist> GetTopBands(int max)
		{
			List<Artist> query				= Context.Artists.Where(p => p.ArtistType == "Band").OrderByDescending(p => p.Fans.Count).Take(max).ToList();
			List<FeaturedArtist> artists	= new List<FeaturedArtist>();

			foreach (Artist artist in query)
				artists.Add(new FeaturedArtist(artist));

			return artists;
		}

		public List<FeaturedArtist> GetTopWriters(int max)
		{
			List<Artist> query				= Context.Artists.Where(p => p.ArtistType == "SongWriter").OrderByDescending(p => p.Fans.Count).Take(max).ToList();
			List<FeaturedArtist> artists	= new List<FeaturedArtist>();

			foreach (Artist artist in query)
				artists.Add(new FeaturedArtist(artist));

			return artists;
		}

        public Artist CreateNewArtistAndSave(ApplicationUser user)
        {
            Random r = new Random();
            Artist Artist = new Artist();

            Artist.Name = user.Name;
            Artist.ArtistType = user.AccountType;
            Artist.BiographyShort = !string.IsNullOrEmpty(user.BiographyShort) ? user.BiographyShort.Replace("\n", "<br />") : user.BiographyShort;
            Artist.BiographyLong = !string.IsNullOrEmpty(user.BiographyLong) ? user.BiographyLong.Replace("\n", "<br />") : user.BiographyLong;
            Artist.ImageURL = user.PhotoURL;

			string slug = Artist.Name.Trim();
			Regex regex = new Regex("[ <>?:\"{}_+|~,./;'[\\]\\-=\\\\`!@#$%^&*()]");
			slug		= regex.Replace(slug, "");

			if (Context.Artists.Any(p => p.Slug == slug))
				slug += Context.Artists.Max(p => p.ArtistId);

			Artist.Slug = slug;

            if (string.IsNullOrWhiteSpace(Artist.ImageURL)) Artist.ImageURL = "/content/blur-backgrounds/" + r.Next(1, 32) + ".png";

            Artist.SlideshowImages = new List<ArtistSlideshowImage>();
            Artist.SlideshowImages.Add(new ArtistSlideshowImage() { ImageURL = "/content/blur-backgrounds/" + r.Next(1, 32) + ".png" });

            user.Artists.Add(Artist);
            Context.SaveChanges();
            
            return Artist;
        }

		/// <summary>
		/// Artist will be deleted, Media will be unlinked and left on the users account
		/// </summary>
		/// <param name="id"></param>
		public void DeleteArtistAndSave(int id)
		{
			Artist artist			= Context.Artists.FirstOrDefault(p => p.ArtistId == id);
			ApplicationUser user	= SecurityHelper.GetLoggedInUser();

			if (artist == null || user == null)
				return;

			if (user.HasPermission(Permission.EditArtists) || user.HasPermission(Permission.EditMyArtists) && user.Artists.Contains(artist))
			{
                if (!artist.ImageURL.StartsWith("/"))
                    AzureUploadManager.DeleteBlob(artist.ImageURL);
				foreach (ArtistSlideshowImage image in artist.SlideshowImages)
				{
                    if (!image.ImageURL.StartsWith("/"))
                        AzureUploadManager.DeleteBlob(image.ImageURL);
                }
				Context.ArtistSlideshowImages.RemoveRange(artist.SlideshowImages);
				
				artist.Songs.Clear();
				artist.Videos.Clear();

				Context.SocialLinks.RemoveRange(artist.SocialLinks);

				Context.Artists.Remove(artist);
				Context.SaveChanges();
			}
		}

		public bool AddFanAndSave(int ArtistId)
        {
            Artist artist = Context.Artists.FirstOrDefault(p => p.ArtistId == ArtistId);
            if (artist == null) return false;

            ApplicationUser User = SecurityHelper.GetLoggedInUser();
			if(User==null) return false;
            User.FavoriteArtists.Add(artist);
            Context.SaveChanges();
            return true;

        }
        public bool RemoveFanAndSave(int ArtistId)
        {
            Artist artist = Context.Artists.FirstOrDefault(p => p.ArtistId == ArtistId);
            if (artist == null) return false;

            ApplicationUser User = SecurityHelper.GetLoggedInUser();
            User.FavoriteArtists.Remove(artist);

            Context.SaveChanges();
            return true;

        }

        public SocialLinkViewModel GetSocialLinkViewModel(int SocialLinkId)
        {
            ApplicationUser User = SecurityHelper.GetLoggedInUser();

            SocialLink SocialLink = Context.SocialLinks.FirstOrDefault(p => p.SocialLinkId == SocialLinkId);
            
            // Make sure the social link is connected to one of the users Artists
            if (!SocialLink.Artist.Members.Any(p => p.Id == User.Id)) return null;

            return new SocialLinkViewModel(SocialLink);
        }

		public string GetDefaultName(int artistId)
		{
			Artist artist			= Context.Artists.FirstOrDefault(p => p.ArtistId == artistId);
			ApplicationUser user	= artist.Members.First();

			return user != null ? user.Name : "Default";
		}

		public void ChangeArtistTypeAndSave(int artistId, string artistSlug, string artistType, ref string message)
		{
			var user	= SecurityHelper.GetLoggedInUser();
			var artist	= Context.Artists.FirstOrDefault(p => p.ArtistId == artistId);

			if (artist == null || (!user.HasPermission(Permission.EditArtists) && !user.Artists.Any(p => p.ArtistId == artistId)))
				return;

			bool changed = false;

			if (!string.IsNullOrWhiteSpace(artistSlug))
			{
				string slug = artistSlug.Trim();
				Regex regex = new Regex("[ <>?:\"{}_+|~,./;'[\\]\\-=\\\\`!@#$%^&*()]");
				slug		= regex.Replace(slug, "");

				if (artistSlug == artist.Slug) { }
				else if (Context.Artists.Any(p => p.Slug == artistSlug))
					message = "That url has already been taken by someone else.";
				else
				{
					artist.Slug	= artistSlug;
					changed		= true;
				}
			}

			if (!string.IsNullOrWhiteSpace(artistType) && artistType != artist.ArtistType)
			{
				artist.ArtistType = artistType;
				changed = true;
			}

			if (changed)
				Context.SaveChanges();

			return;
		}

		public bool SetNameAndSave(int id, string value)
		{
			ApplicationUser User	= SecurityHelper.GetLoggedInUser();
			Artist artist			= User.Artists.FirstOrDefault(p => p.ArtistId == id);

			if (artist == null)
				return false;

			artist.Name = value;
			Context.SaveChanges();

			return true;
		}

		public bool SetShortBiographyAndSave(int id, string value)
		{
			ApplicationUser User	= SecurityHelper.GetLoggedInUser();
			Artist artist			= User.Artists.FirstOrDefault(p => p.ArtistId == id);

			if (artist == null)
				return false;

			artist.BiographyShort = value;
			Context.SaveChanges();

			return true;
		}

		public bool SetStatusAndSave(int id, string value)
		{
			ApplicationUser User	= SecurityHelper.GetLoggedInUser();
			Artist artist			= User.Artists.FirstOrDefault(p => p.ArtistId == id);

			if (artist == null)
				return false;

			artist.Status = value;
			Context.SaveChanges();

			return true;
		}

		public bool SetLongBiographyAndSave(int id, string value)
		{
			ApplicationUser User	= SecurityHelper.GetLoggedInUser();
			Artist artist			= User.Artists.FirstOrDefault(p => p.ArtistId == id);

			if (artist == null)
				return false;

			artist.BiographyLong = value;
			Context.SaveChanges();

			return true;
		}

		public bool SetImageAndSave(int artistId, HttpPostedFileBase file, out string url)
		{
			ApplicationUser User	= SecurityHelper.GetLoggedInUser();
			Artist artist			= User.Artists.FirstOrDefault(p => p.ArtistId == artistId);

			if (artist == null)
			{
				url = "";
				return false;
			}

            string newImageURL = AzureUploadManager.UploadFile(file, "ArtistImage");

            if (!string.IsNullOrEmpty(newImageURL)) {
                if (!string.IsNullOrEmpty(artist.ImageURL) && artist.ImageURL.Contains("/content/ArtistImage/")) AzureUploadManager.DeleteBlob(artist.ImageURL);
                artist.ImageURL = newImageURL;
            }

            url = artist.ImageURL;

            Context.SaveChanges();
			return true;
		}

		public bool SetBannerImageURLsAndSave(int artistId, string[] slides, HttpFileCollectionBase files, out List<string> urls)
		{
			ApplicationUser User	= SecurityHelper.GetLoggedInUser();
			Artist artist			= User.Artists.FirstOrDefault(p => p.ArtistId == artistId);

			if (artist == null)
			{
				urls = null;
				return false;
			}

			ArtistSlideshowImage slide;
			HttpPostedFileBase file;
			List<ArtistSlideshowImage> slidesCurrent = artist.SlideshowImages.ToList();
			List<ArtistSlideshowImage> slidesPending = new List<ArtistSlideshowImage>();
			List<string> urlList = new List<string>();
			string url;

			if (slides != null)
			{
				foreach (string name in slides)
				{
					file = files[name];

					if (file != null) // upload new slide
					{
						url = AzureUploadManager.UploadFile(file, "BannerImage");

						if (!string.IsNullOrEmpty(url))
							urlList.Add(url);

						slidesPending.Add(new ArtistSlideshowImage() {Artist = artist, ImageURL = url});
					}
					else // keep existing slide
					{
						slide = artist.SlideshowImages.FirstOrDefault(p => p.ImageURL == name);

						if (slide != null)
						{
							urlList.Add(name);
							slidesPending.Add(slide);
						}
					}
				}
			}

			// Isolate and delete any slides that weren't passed back by in the request
			foreach (ArtistSlideshowImage s in slidesPending)
				slidesCurrent.Remove(s);

			foreach (ArtistSlideshowImage s in slidesCurrent)
				if (!string.IsNullOrEmpty(s.ImageURL) && s.ImageURL.Contains("/Content/BannerImage/"))
					AzureUploadManager.DeleteBlob(s.ImageURL);

			// Post back urls, and save changes
			urls = urlList;
			artist.SlideshowImages = slidesPending;
			Context.SaveChanges();
			return true;
		}

        public bool IsFanOf(int ArtistId)
        {
            Models.ApplicationUser User = SecurityHelper.GetLoggedInUser();
            if (User == null) return false;
            bool isFan = User.FavoriteArtists.Any(p => p.ArtistId == ArtistId);
            return isFan;
        }

        public SocialLink AddSocialLinkAndSave(SocialLinkViewModel model)
        {
            ApplicationUser User = SecurityHelper.GetLoggedInUser();

            SocialLink SocialLink = new SocialLink();
            
            // Make sure the User has access to the given Artist
            Artist Artist = User.Artists.FirstOrDefault(p => p.ArtistId == model.ArtistId);

            if (Artist != null)
            {
                Artist.SocialLinks.Add(SocialLink);

				SocialLink.SocialLinkId = model.SocialLinkId.HasValue ? model.SocialLinkId.Value : -1;
                SocialLink.Name			= model.Name;
                SocialLink.URL			= model.URL;

                Context.SaveChanges();
                return SocialLink;
            }
            return null;
        }

        public bool UpdateSocialLinkAndSave(SocialLinkViewModel model)
        {
            ApplicationUser User = SecurityHelper.GetLoggedInUser();

            SocialLink SocialLink = Context.SocialLinks.FirstOrDefault(p => p.SocialLinkId == model.SocialLinkId.Value);
            if (SocialLink == null) return false;

            // Make sure the social link is connected to one of the users Artists
            if (!SocialLink.Artist.Members.Any(p => p.Id == User.Id)) return false;
            
            SocialLink.Name = model.Name;
            SocialLink.URL = model.URL;

            Context.SaveChanges();
            return true;
        }

        public bool RemoveSocialLinkAndSave(int SocialLinkId)
        {
            ApplicationUser User = SecurityHelper.GetLoggedInUser();

            SocialLink SocialLink = Context.SocialLinks.FirstOrDefault(p => p.SocialLinkId == SocialLinkId);
            if (SocialLink == null) return false;

            // Make sure the social link is connected to one of the users Artists
            if (!SocialLink.Artist.Members.Any(p => p.Id == User.Id)) return false;

            Context.SocialLinks.Remove(SocialLink);
            Context.SaveChanges();
            return true;
        }

		public void RemoveSongAndSave(List<SongArtist> songArtists, Song song)
		{
			ApplicationUser User = SecurityHelper.GetLoggedInUser();
			Artist artist;

			foreach (SongArtist songArtist in songArtists)
			{
				artist = User.Artists.FirstOrDefault(p => p.ArtistId == songArtist.Id);

				if (artist != null)
				{
					Song s = artist.Songs.FirstOrDefault(p => p.SongId == song.SongId);

					if (s != null)
						artist.Songs.Remove(s);
				}
			}

			Context.SaveChanges();
		}

		public List<string> GetArtistSlideshow(int id)
		{
			Artist artist = GetArtist(id);

			if (artist == null)
				return null;

			List<string> result = new List<string>();

			foreach (ArtistSlideshowImage slide in artist.SlideshowImages)
				result.Add(slide.ImageURL);

			return result;
		}
    }
}