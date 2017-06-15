using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using URMade.Models;
using System;

namespace URMade
{
    public class SongRepository
    {
		private ApplicationDbContext Context { get; set; }

        public SongRepository(ApplicationDbContext context)
        {
            Context = context;
        }

		public Song GetSong(int id)
		{
			return Context.Songs.FirstOrDefault(p => p.SongId == id);
		}

		public List<Song> GetUserSongs(string id)
		{
			return Context.Songs.Where(p => p.Owner.Id == id).ToList();
		}

		private void PublishSong(Song song, PublishModel publish)
		{
			var user = SecurityHelper.GetLoggedInUser();

			if (song == null || publish == null || user == null)
				return;

			if (publish.Selected >= 0)
			{
				var artistId	= publish.Items[publish.Selected].ArtistId;
				var artist		= user.Artists.FirstOrDefault(p => p.ArtistId == artistId);

				if (artist != null)
					song.Artist = artist;
			}
			else
				song.Artist = null;
		}

		public void EditSongAndSave(EditSongViewModel model)
		{
			Song song = Context.Songs.FirstOrDefault(p => p.SongId == model.SongId);

            if (song == null)
            {
                song = new Song() {Owner = SecurityHelper.GetLoggedInUser()};
                Context.Songs.Add(song);
            }

            model.UpdateEntity(song);
			PublishSong(song, model.Publish);
			Context.SaveChanges();
		}
		
		public void DeleteSongAndSave(int id)
		{
			DeleteSong(id);
			Context.SaveChanges();
        }
        public void DeleteSong(int id)
        {
            Song song = GetSong(id);
            DeleteSong(song);
        }
        public void DeleteSong(Song song)
        {
			if (song != null)
			{
				AzureUploadManager.DeleteBlob(song.AlbumArtURL);
				AzureUploadManager.DeleteOrCancelJob(song.JobId);
				AzureUploadManager.DeleteAsset(song.AssetId);
				AzureUploadManager.DeleteAsset(song.EncodedAssetId);
                Context.ContestsEntries.RemoveRange(song.ContestEntries.ToList());
                Context.Songs.Remove(song);
			}
        }

		public List<SearchItemViewModel> Search(string match, int max)
		{
			List<Song> songs = Context.Songs.Where(p => p.Title.Contains(match)).Take(max).ToList();

			if (songs == null || songs.Count < 1)
				return null;

			List<SearchItemViewModel> result = new List<SearchItemViewModel>();
			foreach (Song song in songs)
				result.Add(new SearchItemViewModel(song));

			return result;
		}

		public void UpdateQueueAndSave()
        {
            List<Song> Queue = Context.Songs.ToList();
			
            foreach (Song song in Queue)
            {
                MediaAsset detail = new MediaAsset() { AssetId = song.AssetId, JobId = song.JobId };
                AzureUploadManager.UpdateMediaAsset(detail);
                song.State = (URMade.Models.JobState)detail.JobState;
                song.EncodedAssetId = detail.EncodedAssetId;
                song.SmoothStreamingUri = detail.SmoothStreamingUri;
            }

            Context.SaveChanges();
        }
        
        public void UpdateAssetDetailAndSave(IQueryable<Song> songs)
        {
            List<Song> Songs = songs.Where(p => p.State == JobState.Queued || p.State == JobState.Scheduled || p.State == JobState.Processing || p.State == JobState.Canceling).ToList();
            if(Songs.Count() > 0)
			{
                foreach (Song song in Songs)
                {
                    MediaAsset detail = new MediaAsset() { AssetId = song.AssetId, JobId = song.JobId };
                    AzureUploadManager.UpdateMediaAsset(detail);
                    song.State = (URMade.Models.JobState)detail.JobState;
                    song.EncodedAssetId = detail.EncodedAssetId;
                    song.SmoothStreamingUri = detail.SmoothStreamingUri;
                }

                Context.SaveChanges();
            }
        }

		public PlaylistModel GetSongPlaylist(PlaylistQuery options)
		{
			return new PlaylistModel(Context.Songs, options);
		}

		public PlaylistModel GetUserSongPlaylist(PlaylistQuery options)
		{
			IQueryable<Song> songs;

			var user = SecurityHelper.GetLoggedInUser();
			if (user != null)
			{
				options.UserId	= user.Id;
				songs			= Context.Songs.Where(p => p.Owner.Id == user.Id);
				UpdateAssetDetailAndSave(songs);
			}
			else
				songs = Context.Songs;

			return new PlaylistModel(songs, options);
		}

        public int ToggleFavoriteAndSave(int id, out bool active)
		{
			ApplicationUser user	= SecurityHelper.GetLoggedInUser();
			Song song				= GetSong(id);

			if (user == null || song == null)
			{
				active = false;
				return -1;
			}

			Song match = user.FavoriteSongs.FirstOrDefault(p => p.SongId == id);

			if (match == null)
			{
				active = true;
				user.FavoriteSongs.Add(song);
			}
			else
			{
				active = false;
				user.FavoriteSongs.Remove(match);
			}

			Context.SaveChanges();
			return song.Fans.Count;
		}
	}
}