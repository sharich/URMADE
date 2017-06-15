using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

namespace URMade.Models
{
	public class PlaylistItemModel
	{
		public string Title			{get; set;}
		public string AlbumArtURL	{get; set;}
		public string MediaURL		{get; set;}
		public int MediaId			{get; set;}
		public int FanCount			{get; set;}
		public bool IsFavorited		{get; set;}
		public string ArtistSlug	{get; set;}
		public string ArtistName	{get; set;}
		public bool CanEdit			{get; set;}
		public JobState State		{get; set;}

		private void FromSong(Song song)
		{
			Title		= song.Title;
			AlbumArtURL = song.AlbumArtURL;
			MediaURL	= song.SmoothStreamingUri;
			MediaId		= song.SongId;
			FanCount	= song.Fans.Count;
			State		= song.State;
			
			if (song.Artist != null)
			{
				ArtistName	= song.Artist.Name;
				ArtistSlug	= song.Artist.Slug;
			}
			else
			{
				ArtistName	= "";
				ArtistSlug	= "";
			}
		}

		private void FromVideo(Video video)
		{
			Title		= video.Title;
			AlbumArtURL = video.AlbumArtURL;
			MediaURL	= video.SmoothStreamingUri;
			MediaId		= video.VideoId;
			FanCount	= video.Fans.Count;
			State		= video.State;

			if (video.Artist != null)
			{
				ArtistName = video.Artist.Name;
				ArtistSlug = video.Artist.Slug;
			}
			else
			{
				ArtistName = "";
				ArtistSlug = "";
			}
		}

		public PlaylistItemModel(Song song)
		{
			FromSong(song);

			ApplicationUser user = SecurityHelper.GetLoggedInUser();
			IsFavorited	= user != null ? song.Fans.Any(p => p.Id == user.Id) : false;
			if (user != null)
				CanEdit = user.HasPermission(Permission.EditArtists) || user.Id == song.Owner?.Id;
			else
				CanEdit = false;
		}

		public PlaylistItemModel(Video video)
		{
			FromVideo(video);

			ApplicationUser user = SecurityHelper.GetLoggedInUser();
			IsFavorited	= user != null ? video.Fans.Any(p => p.Id == user.Id) : false;
			if (user != null)
				CanEdit = user.HasPermission(Permission.EditArtists) || user.Id == video.Owner?.Id;
			else
				CanEdit = false;
		}

		public PlaylistItemModel(ContestEntry entry)
		{
			if (entry.Song != null)
				FromSong(entry.Song);
			else if (entry.Video != null)
				FromVideo(entry.Video);

			ApplicationUser user	= SecurityHelper.GetLoggedInUser();
			MediaId					= entry.EntryId;
			IsFavorited				= user != null ? entry.Voters.Any(p => p.Id == user.Id) : false;
			FanCount				= entry.Voters.Count;
			CanEdit					= false;
		}

		public PlaylistItemModel(Artist artist, Song song)
		{
			ArtistName	= artist.Name;
			ArtistSlug	= artist.Slug;
			AlbumArtURL = artist.ImageURL;
			FanCount	= artist.Fans.Count;
			CanEdit		= false;

			if (song != null && song.State == JobState.Finished)
			{
				Title		= song.Title;
				MediaURL	= song.SmoothStreamingUri;
				MediaId		= song.SongId;
				State		= song.State;
			}
			else
				MediaId = 0;
		}
	}

	public class PlaylistQuery
	{
		public PlaylistModel.SortType Sort	{get; set;}
		public string UserId				{get; set;}
		public int ArtistId					{get; set;}
		public string Type					{get; set;}
		public string Name					{get; set;}
		public bool FavoritesOnly			{get; set;}
		public int Max						{get; set;}
		public int Page						{get; set;}

		public PlaylistQuery()
		{

		}
	}

	public class PlaylistModel
    {
		public List<PlaylistItemModel> Items {get; set;}

		// Sorting
		public enum SortType
		{
			TrendingWeekly	= 1,
			TrendingMonthly = 2,
			TrendingAllTime	= 3,
			Recent			= 4
		}

		public SortType Sort {get; set;}

		// Filtering
		public string UserId		{get; set;}
		public int ArtistId			{get; set;}
		public string Type			{get; set;}
		public string Name			{get; set;}
		public bool FavoritesOnly	{get; set;}

		// Pagination
		public int Max	{get; set;}
		public int Page	{get; set;}

		// View
		public string Controller	{get; set;}
		public string MediaType		{get; set;}
		public bool CanEdit			{get; set;}
		public bool CanFavorite		{get; set;}
		public bool IsProcessing	{get; set;}

		private List<Song> ApplyQuery(IQueryable<Song> query, ApplicationUser user)
		{
			DateTime tomorrow = DateTime.Now.AddDays(1);

			switch (Sort)
			{
				default:
				case SortType.TrendingWeekly:
					query = query.OrderByDescending(p => p.Fans.Count / (double) DbFunctions.DiffHours(p.DateCreated, tomorrow));
					break;
				case SortType.TrendingMonthly:
					query = query.OrderByDescending(p => p.Fans.Count / (double) DbFunctions.DiffHours(p.DateCreated, tomorrow));
					break;
				case SortType.TrendingAllTime:
					query = query.OrderByDescending(p => p.Fans.Count);
					break;
				case SortType.Recent:
					query = query.OrderByDescending(p => p.DateCreated);
					break;
			}

			if (!string.IsNullOrWhiteSpace(Type))
				query = query.Where(p => p.SubGenre == Type);

			if (!string.IsNullOrWhiteSpace(Name))
				query = query.Where(p => p.Title.Contains(Name));

			if (user == null || string.IsNullOrWhiteSpace(UserId) || user.Id != UserId)
				query = query.Where(p => p.State == JobState.Finished);

			if (!string.IsNullOrWhiteSpace(UserId))
				query = query.Where(p => p.Owner.Id == UserId);
			else
				query = query.Where(p => p.Artist != null);

			if (ArtistId != 0)
				query = query.Where(p => p.Artist.ArtistId == ArtistId);

			if (FavoritesOnly && user != null)
				query = query.Where(p => p.Fans.Any(f => f.Id == user.Id));

			if (Page > 0)
				query = query.Skip(Page * Max);

			if (Max > 0)
				query = query.Take(Max);

			return query.ToList();
		}

		private List<Video> ApplyQuery(IQueryable<Video> query, ApplicationUser user)
		{
			DateTime tomorrow = DateTime.Now.AddDays(1);

			switch (Sort)
			{
				default:
				case SortType.TrendingWeekly:
					query = query.OrderByDescending(p => p.Fans.Count / (double) DbFunctions.DiffHours(p.DateCreated, tomorrow));
					break;
				case SortType.TrendingMonthly:
					query = query.OrderByDescending(p => p.Fans.Count / (double) DbFunctions.DiffHours(p.DateCreated, tomorrow));
					break;
				case SortType.TrendingAllTime:
					query = query.OrderByDescending(p => p.Fans.Count);
					break;
				case SortType.Recent:
					query = query.OrderByDescending(p => p.DateCreated);
					break;
			}

			if (!string.IsNullOrWhiteSpace(Type))
				query = query.Where(p => p.SubGenre == Type);

			if (!string.IsNullOrWhiteSpace(Name))
				query = query.Where(p => p.Title.Contains(Name));

			if (user == null || string.IsNullOrWhiteSpace(UserId) || user.Id != UserId)
				query = query.Where(p => p.State == JobState.Finished);

			if (!string.IsNullOrWhiteSpace(UserId))
				query = query.Where(p => p.Owner.Id == UserId);
			else
				query = query.Where(p => p.Artist != null);

			if (ArtistId != 0)
				query = query.Where(p => p.Artist.ArtistId == ArtistId);

			if (FavoritesOnly && user != null)
				query = query.Where(p => p.Fans.Any(f => f.Id == user.Id));

			if (Page > 0)
				query = query.Skip(Page * Max);

			if (Max > 0)
				query = query.Take(Max);

			return query.ToList();
		}

		private List<Artist> ApplyQuery(IQueryable<Artist> query, ApplicationUser user)
		{
			DateTime tomorrow = DateTime.Now.AddDays(1);

			switch (Sort)
			{
				case SortType.TrendingWeekly:
					query = query.OrderByDescending(p => p.Fans.Count);
					break;
				case SortType.TrendingMonthly:
					query = query.OrderByDescending(p => p.Fans.Count);
					break;
				case SortType.TrendingAllTime:
					query = query.OrderByDescending(p => p.Fans.Count);
					break;
				case SortType.Recent:
					query = query.OrderByDescending(p => p.Fans.Count);
					break;
			}

			if (!string.IsNullOrWhiteSpace(Type))
				query = query.Where(p => p.ArtistType == Type);

			if (!string.IsNullOrWhiteSpace(Name))
				query = query.Where(p => p.Name.Contains(Name));

			if (FavoritesOnly && user != null)
				query = query.Where(p => p.Fans.Any(f => f.Id == user.Id));

			if (Page > 0)
				query = query.Skip(Page * Max);

			if (Max > 0)
				query = query.Take(Max);

			return query.ToList();
		}

		private List<ContestEntry> ApplyQuery(IQueryable<ContestEntry> query, Contest contest, ApplicationUser user)
		{
			DateTime tomorrow = DateTime.Now.AddDays(1);

			query = query.Where(p => p.Contest.ContestId == contest.ContestId);

			if (contest.Type == ContestType.Song)
			{
				switch (Sort)
				{
					case SortType.TrendingWeekly:
							query = query.OrderByDescending(p => p.Song.Fans.Count / (double) DbFunctions.DiffHours(p.Song.DateCreated, tomorrow));
						break;
					case SortType.TrendingMonthly:
							query = query.OrderByDescending(p => p.Song.Fans.Count / (double) DbFunctions.DiffHours(p.Song.DateCreated, tomorrow));
						break;
					case SortType.TrendingAllTime:
							query = query.OrderByDescending(p => p.Song.Fans.Count);
						break;
					case SortType.Recent:
							query = query.OrderByDescending(p => p.Song.DateCreated);
						break;
				}

				if (!string.IsNullOrWhiteSpace(Type))
					query = query.Where(p => p.Song.SubGenre == Type);

				if (!string.IsNullOrWhiteSpace(Name))
					query = query.Where(p => p.Song.Title == Name);

				if (Page > 0)
					query = query.Skip(Page * Max);

				if (Max > 0)
					query = query.Take(Max);
			}
			else
			{
				switch (Sort)
				{
					case SortType.TrendingWeekly:
							query = query.OrderByDescending(p => p.Video.Fans.Count / (double) DbFunctions.DiffHours(p.Video.DateCreated, tomorrow));
						break;
					case SortType.TrendingMonthly:
							query = query.OrderByDescending(p => p.Video.Fans.Count / (double) DbFunctions.DiffHours(p.Video.DateCreated, tomorrow));
						break;
					case SortType.TrendingAllTime:
							query = query.OrderByDescending(p => p.Video.Fans.Count);
						break;
					case SortType.Recent:
							query = query.OrderByDescending(p => p.Video.DateCreated);
						break;
				}

				if (!string.IsNullOrWhiteSpace(Type))
					query = query.Where(p => p.Video.SubGenre == Type);

				if (!string.IsNullOrWhiteSpace(Name))
					query = query.Where(p => p.Video.Title == Name);

				query = query.Where(p => p.Video.Artist != null);

				if (Page > 0)
					query = query.Skip(Page * Max);

				if (Max > 0)
					query = query.Take(Max);
			}
			
			return query.ToList();
		}

		private List<ContestEntry> ApplyQuery(IQueryable<ContestEntry> query, ApplicationUser user)
		{
			query = query.Where(p => p.Song != null ? p.Song.Artist.ArtistId == ArtistId : p.Video.Artist.ArtistId == ArtistId);

			if (Page > 0)
				query = query.Skip(Page * Max);

			if (Max > 0)
				query = query.Take(Max);

			return query.ToList();
		}

		private void Setup(PlaylistQuery query, ApplicationUser user)
		{
			Items = new List<PlaylistItemModel>();

			if (query != null)
			{
				UserId			= query.UserId;
				ArtistId		= query.ArtistId;
				Sort			= query.Sort;
				Type			= query.Type;
				Name			= query.Name;
				FavoritesOnly	= query.FavoritesOnly;
				Max				= query.Max;
				Page			= query.Page;
			}
			else
			{
				UserId			= null;
				ArtistId		= 0;
				Sort			= SortType.TrendingWeekly;
				Type			= null;
				Name			= null;
				FavoritesOnly	= false;
				Max				= 25;
				Page			= 0;
			}

			if (user != null)
			{
				CanFavorite = user.HasPermission(Permission.CanFavoriteAndVote);
				CanEdit		= user.Id == UserId;
			}
			else
			{
				CanFavorite = false;
				CanEdit		= false;
			}
		}

		public PlaylistModel()
		{

		}

		public PlaylistModel(IQueryable<Song> query, PlaylistQuery options = null)
		{
			var user = SecurityHelper.GetLoggedInUser();

			Setup(options, user);
			MediaType	= "Song";
			Controller	= "Song";

			var items = ApplyQuery(query, user);
			foreach (var item in items)
			{
				if (item.State != JobState.Finished)
					IsProcessing = true;

				Items.Add(new PlaylistItemModel(item));
			}
		}

		public PlaylistModel(IQueryable<Video> query, PlaylistQuery options = null)
		{
			var user = SecurityHelper.GetLoggedInUser();

			Setup(options, user);
			MediaType	= "Video";
			Controller	= "Video";

			var items = ApplyQuery(query, user);
			foreach (var item in items)
			{
				if (item.State != JobState.Finished)
					IsProcessing = true;

				Items.Add(new PlaylistItemModel(item));
			}
		}

		public PlaylistModel(IQueryable<Artist> query, PlaylistQuery options = null)
		{
			var user = SecurityHelper.GetLoggedInUser();

			Setup(options, user);
			MediaType	= "Artist";
			Controller	= "Artist";

			var items = ApplyQuery(query, user);
			foreach (var item in items)
				Items.Add(new PlaylistItemModel(item, item.Songs.Where(p => p.State == JobState.Finished).OrderByDescending(p => p.Fans.Count).FirstOrDefault()));
		}

		public PlaylistModel(IQueryable<ContestEntry> query, Contest contest, PlaylistQuery options = null)
		{
			var user = SecurityHelper.GetLoggedInUser();

			Setup(options, user);
			MediaType	= contest.Type == ContestType.Song ? "Song" : contest.Type == ContestType.Song ? "Video" : "";
			Controller	= "Contest";

			var items = ApplyQuery(query, contest, user);
			foreach (var item in items)
				Items.Add(new PlaylistItemModel(item));
		}

		public PlaylistModel(IQueryable<ContestEntry> query, PlaylistQuery options = null)
		{
			var user = SecurityHelper.GetLoggedInUser();

			Setup(options, user);
			MediaType	= "Contest";
			Controller	= "Contest";

			var items = ApplyQuery(query, user);
			foreach (var item in items)
				Items.Add(new PlaylistItemModel(item));
		}
	}

	public class RankingsPlaylistModel
	{
		public PlaylistModel Songs	{get; set;}
		public PlaylistModel Videos {get; set;}

		public RankingsPlaylistModel(PlaylistModel songs, PlaylistModel videos)
		{
			Songs				= songs;
			Songs.CanEdit		= false;
			Videos				= videos;
			Videos.CanEdit		= false;
		}
	}

	public class FeaturedArtist
	{
		public string ArtistSlug	{get; set;}
		public string Name			{get; set;}
		public string ImageUrl		{get; set;}
		public int FanCount			{get; set;}
		public string SongName		{get; set;}
		public string SongUrl		{get; set;}
		public bool IsPremium		{get; set;}

		public FeaturedArtist()
		{

		}

		public FeaturedArtist(Artist artist)
		{
			var song = artist.Songs.OrderByDescending(p => p.Fans.Count).FirstOrDefault();

			ArtistSlug	= artist.Slug;
			Name		= artist.Name;
			ImageUrl	= artist.ImageURL;
			FanCount	= artist.Fans.Count;

			if (song != null && !string.IsNullOrWhiteSpace(song.SmoothStreamingUri))
			{
				SongName	= song.Title;
				SongUrl		= song.SmoothStreamingUri;
			}
			else
			{
				SongName	= "";
				SongUrl		= "";
			}

            IsPremium = false;
            if (artist.Members.FirstOrDefault() !=  null)
				IsPremium = artist.Members.FirstOrDefault().HasPermission(Permission.CanPublishUnlimitedSongs);
		}

		public string DisplayFanCount()
		{
			float count;

			if (FanCount == 1)
				return FanCount + " LIKE";

			if (FanCount >= 1000)
			{
				if (FanCount >= 1000000)
				{
					count = FanCount / 1000000.0f;
					return count.ToString("F1") + "M LIKES";
				}
				else
				{
					count = FanCount / 1000.0f;
					return count.ToString("F1") + "K LIKES";
				}
			}
			else
				return FanCount + " LIKES";
		}

		public string DisplayMembershipType()
		{
			return IsPremium ? "PREMIUM" : "FREE";
		}
	}
}