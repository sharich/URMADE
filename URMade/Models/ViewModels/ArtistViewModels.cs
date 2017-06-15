using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace URMade.Models
{
    public class ArtistViewModel
    {
        public ArtistViewModel()
        {

        }

        public ArtistViewModel(Artist Artist, ApplicationUser user, PlaylistModel songs, PlaylistModel videos, PlaylistModel contestEntries)
        {
			var currentUser = SecurityHelper.GetLoggedInUser();

            ArtistId = Artist.ArtistId;
            Slug = Artist.Slug;
            Name = Artist.Name;
            ImageURL = Artist.ImageURL;
            ArtistType = Artist.ArtistType;
            BiographyShort = Artist.BiographyShort;
            BiographyLong = Artist.BiographyLong;
            Status = Artist.Status;

			if (currentUser != null)
			{
				IsMyArtist	= currentUser.Id == user.Id;
				CanEdit		= (IsMyArtist && currentUser.HasPermission(Permission.EditMyArtists)) || currentUser.HasPermission(Permission.EditArtists);
			}
			else
			{
				IsMyArtist	= false;
				CanEdit		= false;
			}

            this.SlideshowImages = Artist.SlideshowImages.ToList();
            this.SocialLinks = Artist.SocialLinks.ToList();

            this.Songs		= songs;
            this.Videos		= videos;
			this.Entries	= contestEntries;
            this.Fans		= Artist.Fans.Select(p=>new FanResultViewModel(p)).ToList();

			DisplaySlideshow		= user.HasPermission(Permission.HasWebsiteSlideshow);
			DisplaySongs			= Artist.Songs != null ? Artist.Songs.Count > 0 : false;
			DisplayVideos			= user.HasPermission(Permission.CanPublishVideos);
			DisplayStatus			= user.HasPermission(Permission.HasWebsiteStatus);
			DisplayLongBiography	= user.HasPermission(Permission.HasLongBiography);
        }

        public int ArtistId { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ArtistType { get; set; }
        public string BiographyShort { get; set; }
        public string BiographyLong { get; set; }
        public string Status { get; set; }

		public bool CanEdit { get; set; }
		public bool IsMyArtist { get; set; }
		public string SettingsValidation { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; }

        public virtual ICollection<ArtistSlideshowImage> SlideshowImages { get; set; }
        //public virtual ICollection<ArtistImage> ArtistImage { get; set; }
        public virtual ICollection<SocialLink> SocialLinks { get; set; }

		public PlaylistModel Songs		{get; set;}
		public PlaylistModel Videos		{get; set;}
		public PlaylistModel Entries	{get; set;}

        public virtual ICollection<FanResultViewModel> Fans { get; set; }

		public bool DisplaySlideshow		{get; set;}
		public bool DisplaySongs			{get; set;}
		public bool DisplayVideos			{get; set;}
		public bool DisplayStatus			{get; set;}
		public bool DisplayLongBiography	{get; set;}
    }

    public class ArtistResultViewModel
    {
        public ArtistResultViewModel()
        {
        }

        public ArtistResultViewModel(Artist Artist)
        {
            ArtistId = Artist.ArtistId;
            Slug = Artist.Slug;
            Name = Artist.Name;
            ImageURL = Artist.ImageURL;
            ArtistType = Artist.ArtistType;
        }

        public int ArtistId { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ArtistType { get; set; }

        public string WebsiteURL { get { return (string.IsNullOrEmpty(Slug) ? VirtualPathUtility.ToAbsolute("~/Artist/Website/" + ArtistId.ToString()) : "/Artists/" + Slug); } }
    }

	public class ArtistImageViewModel
	{
		public ArtistImageViewModel() {}

		public int? ArtistId			{ get; set; }
		public HttpPostedFileBase file	{ get; set; }
	}

	public class ArtistIndexViewModel
	{
		public List<Artist> Artists { get; set; }

		public ArtistIndexViewModel()
		{
		}

		public ArtistIndexViewModel(IEnumerable<Artist> artists)
        {
			Artists = artists.ToList();
        }
	}

    public class SocialLinkViewModel
    {
        public SocialLinkViewModel()
        {
        }
        public SocialLinkViewModel(int id)
        {
            ArtistId = id;
        }

        public SocialLinkViewModel(SocialLink socialLink)
        {
            SocialLinkId = socialLink.SocialLinkId;
            Name = socialLink.Name;
            URL = socialLink.URL;
        }

        public int? ArtistId { get; set; }

        public int? SocialLinkId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string URL { get; set; }
    }

    public class FanResultViewModel {
        public FanResultViewModel(ApplicationUser user)
        {
            Id = user.Id;
            Slug = user.Slug;
            Name = user.Name;
            PhotoURL = user.PhotoURL;
        }

        private string Id { get; set; }
        private string Slug { get; set; }

        public string Name { get; set; }
        public string PhotoURL { get; set; }

        public string ProfileURL { get { return (string.IsNullOrEmpty(Slug) ? "/User/Details/" + Id.ToString() : "/u/" + Slug); } }
    }

	public class SongArtist
	{
		public int Id			{ get; set; }
		public string Name		{ get; set; }
		public string ImageURL	{ get; set; }
		public bool IsSelected	{ get; set; }
	}

    public class EditSongViewModel
    {
        public int SongId					{get; set;}
        [Required] public string Title		{get; set;}
        public string SubGenre				{get; set;}
        public string AlbumArtURL			{get; set;}
        public HttpPostedFileBase AlbumArt	{get; set;}
        public bool DeleteAlbumArt			{get; set;}
        public string SmoothStreamingUri	{get; set;}
		public string AudioAssetId			{get; set;}
        public HttpPostedFileBase Audio		{get; set;}
		public List<SongArtist> MyArtists	{get; set;}
		public PublishModel Publish			{get; set;}
		public bool AlreadyExists			{get; set;}

        public EditSongViewModel()
        {
			Publish = new PublishModel();
			Publish.ApplySongLimit();

			AlreadyExists = false;
        }

        public EditSongViewModel(Song song)
        {
            SongId				= song.SongId;
            Title				= song.Title;
            AlbumArtURL			= song.AlbumArtURL;
            SmoothStreamingUri	= song.SmoothStreamingUri;
			SubGenre			= song.SubGenre;
			Publish				= new PublishModel(song);
			AlreadyExists		= true;
        }

        public void UpdateEntity(Song song)
        {
            song.Title		= Title;
            song.SubGenre	= SubGenre;

            if (AlbumArt != null)
            {
                if (!string.IsNullOrWhiteSpace(song.AlbumArtURL))
                {
                    AzureUploadManager.DeleteBlob(song.AlbumArtURL);
                    song.AlbumArtURL = "";
                }

                song.AlbumArtURL = AzureUploadManager.UploadFile(AlbumArt, "AlbumArt");
            }
            else if (DeleteAlbumArt == true && !string.IsNullOrWhiteSpace(song.AlbumArtURL))
            {
                AzureUploadManager.DeleteBlob(song.AlbumArtURL);
                song.AlbumArtURL = "";
            }

            // If this is a first upload, set the audio. If the audio already exists, do not accept the new audio file.
            if (Audio != null && song.SmoothStreamingUri == null)
            {
                song.DateCreated = System.DateTime.Now;
                MediaAsset detail = AzureUploadManager.UploadMedia(Audio, "Audio");
                song.AssetId = detail.AssetId;
                song.JobId = detail.JobId;
                song.State = (JobState)detail.JobState;
                song.EncodedAssetId = detail.EncodedAssetId;
                song.SmoothStreamingUri = detail.SmoothStreamingUri;
            }

        }
    }

	public class EditVideoViewModel
	{
		public int VideoId					{get; set;}
        [Required] public string Title		{get; set;}
        public string SubGenre				{get; set;}
        public string AlbumArtURL			{get; set;}
        public HttpPostedFileBase AlbumArt	{get; set;}
        public bool DeleteAlbumArt			{get; set;}
        public string SmoothStreamingUri	{get; set;}
		public string VideoAssetId			{get; set;}
        public HttpPostedFileBase Video		{get; set;}
        public bool DeleteVideo				{get; set;}
		public List<SongArtist> MyArtists	{get; set;}
		public PublishModel Publish			{get; set;}
		public bool AlreadyExists			{get; set;}

		public EditVideoViewModel()
        {
			Publish					= new PublishModel();
			Publish.AllowPublish	= SecurityHelper.LoggedInUserHas(Permission.CanPublishVideos);
			AlreadyExists			= false;
        }

        public EditVideoViewModel(Video video)
        {
            VideoId				= video.VideoId;
            Title				= video.Title;
            AlbumArtURL			= video.AlbumArtURL;
			VideoAssetId		= video.AssetId;
            SmoothStreamingUri	= video.SmoothStreamingUri;
			SubGenre			= video.SubGenre;
			Publish				= new PublishModel(video);
			AlreadyExists		= true;
        }

		public void UpdateEntity(Video video)
        {
            video.Title		= Title;
            video.SubGenre	= SubGenre;

			if (AlbumArt != null)
            {
                if (!string.IsNullOrWhiteSpace(video.AlbumArtURL))
                {
                    AzureUploadManager.DeleteBlob(video.AlbumArtURL);
                    video.AlbumArtURL = "";
                }

                video.AlbumArtURL = AzureUploadManager.UploadFile(AlbumArt, "AlbumArt");
            }
            else if (DeleteAlbumArt == true && !string.IsNullOrWhiteSpace(video.AlbumArtURL))
            {
                AzureUploadManager.DeleteBlob(video.AlbumArtURL);
                video.AlbumArtURL = "";
            }

            // If this is a first upload, set the video. If a video already exists, do not accept the new video file.
			if (Video != null && video.SmoothStreamingUri == null)
            {
                video.DateCreated = System.DateTime.Now;
                MediaAsset detail = AzureUploadManager.UploadMedia(Video, "Video");
                video.AssetId = detail.AssetId;
                video.JobId = detail.JobId;
                video.State = (JobState)detail.JobState;
                video.EncodedAssetId = detail.EncodedAssetId;
                video.SmoothStreamingUri = detail.SmoothStreamingUri;
            }
		}
	}

	public class PublishItemModel
	{
		public int ArtistId			{get; set;}
		public string ArtistName	{get; set;}
		public string ArtistImage	{get; set;}

		public PublishItemModel()
		{

		}

		public PublishItemModel(Artist artist)
		{
			ArtistId	= artist.ArtistId;
			ArtistName	= artist.Name;
			ArtistImage = artist.ImageURL;
		}
	}

	public class PublishModel
	{
		public List<PublishItemModel> Items {get; set;}
		public int Selected					{get; set;}
		public bool AllowPublish			{get; set;}

		public void ApplySongLimit()
		{
			if (Selected < 0)
			{
				AllowPublish = false;

				var user = SecurityHelper.GetLoggedInUser();
				if (user == null || !user.HasPermission(Permission.CanPublishUnlimitedSongs))
				{
					int songCount = 0;

					foreach (Artist artist in user.Artists)
						foreach (Song song in artist.Songs)
							if (++songCount >= 5)
								return;
				}
			}

			AllowPublish = true;
		}

		public PublishModel()
		{
			ApplicationUser user = SecurityHelper.GetLoggedInUser();

			Items			= new List<PublishItemModel>();
			Selected		= -1;
			AllowPublish	= true;

			foreach (Artist artist in user.Artists)
				Items.Add(new PublishItemModel(artist));
		}

		public PublishModel(Song song)
		{
			ApplicationUser user = SecurityHelper.GetLoggedInUser();

			Items			= new List<PublishItemModel>();
			Selected		= -1;
			AllowPublish	= true;

			int i = 0;
			foreach (Artist artist in user.Artists)
			{
				Items.Add(new PublishItemModel(artist));

				if (artist.Songs.Any(p => p.SongId == song.SongId))
					Selected = i;

				++i;
			}

			ApplySongLimit();
		}

		public PublishModel(Video video)
		{
			ApplicationUser user = SecurityHelper.GetLoggedInUser();

			Items			= new List<PublishItemModel>();
			Selected		= -1;
			AllowPublish	= user != null && user.HasPermission(Permission.CanPublishVideos);

			int i = 0;
			foreach (Artist artist in user.Artists)
			{
				Items.Add(new PublishItemModel(artist));

				if (artist.Videos.Any(p => p.VideoId == video.VideoId))
					Selected = i;

				++i;
			}
		}
	}

	public class SearchItemViewModel
	{
		public string Label	{get; set;}
		public string Url	{get; set;}

		public SearchItemViewModel()
		{

		}

		public SearchItemViewModel(Artist artist)
		{
			Label	= artist.Name;
			Url		= "/Artist/Website/" + artist.ArtistId;
		}

		public SearchItemViewModel(Song song)
		{
			Label = song.Title;
		}

		public SearchItemViewModel(Video video)
		{
			Label = video.Title;
		}

		public SearchItemViewModel(Contest contest)
		{
			Label	= contest.Name;
			Url		= "/Contest/Vote/" + contest.ContestId;
		}
	}

	public class SearchResultViewModel
	{
		public List<SearchItemViewModel> Artists	{get; set;}
		public List<SearchItemViewModel> Songs		{get; set;}
		public List<SearchItemViewModel> Videos		{get; set;}
		public List<SearchItemViewModel> Contests	{get; set;}

		public SearchResultViewModel()
		{
		}

		public SearchResultViewModel(List<SearchItemViewModel> artists,
									 List<SearchItemViewModel> songs,
									 List<SearchItemViewModel> videos,
									 List<SearchItemViewModel> contests)
		{
			Artists		= artists;
			Songs		= songs;
			Videos		= videos;
			Contests	= contests;
		}
	}
}