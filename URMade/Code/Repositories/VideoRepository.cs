using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using URMade.Models;

namespace URMade
{
	public class VideoRepository
	{
		private ApplicationDbContext Context { get; set; }

        public VideoRepository(ApplicationDbContext context)
        {
            Context = context;
        }

		public List<SearchItemViewModel> Search(string match, int max)
		{
			List<Video> videos = Context.Videos.Where(p => p.Title.Contains(match)).OrderByDescending(p => p.Fans.Count).Take(max).ToList();

			if (videos == null || videos.Count < 1)
				return null;

			List<SearchItemViewModel> result = new List<SearchItemViewModel>();
			foreach (Video video in videos)
				result.Add(new SearchItemViewModel(video));

			return result;
		}

		public Video GetVideo(int id)
		{
			return Context.Videos.FirstOrDefault(p => p.VideoId == id);
		}

		public List<Video> GetUserVideos(string id)
		{
			return Context.Videos.Where(p => p.Owner.Id == id).ToList();
		}

		private void PublishVideo(Video video, PublishModel publish)
		{
			var user = SecurityHelper.GetLoggedInUser();

			if (video == null || publish == null || user == null || !user.HasPermission(Permission.CanPublishVideos))
				return;

			if (publish.Selected >= 0)
			{
				var artistId	= publish.Items[publish.Selected].ArtistId;
				var artist		= user.Artists.FirstOrDefault(p => p.ArtistId == artistId);

				if (artist != null)
					video.Artist = artist;
			}
			else
				video.Artist = null;
		}

		public void EditVideoAndSave(EditVideoViewModel model)
		{
			Video video = Context.Videos.FirstOrDefault(p => p.VideoId == model.VideoId);

            if (video == null)
            {
                video = new Video() {Owner = SecurityHelper.GetLoggedInUser()};
                Context.Videos.Add(video);
            }

            model.UpdateEntity(video);
			PublishVideo(video, model.Publish);
			Context.SaveChanges();
		}

		public void DeleteVideoAndSave(int id)
		{
			DeleteVideo(id);
			Context.SaveChanges();
        }
        public void DeleteVideo(int id)
        {
            Video video = GetVideo(id);
            DeleteVideo(video);
        }
        public void DeleteVideo(Video video)
        {
			if (video != null)
			{
				AzureUploadManager.DeleteBlob(video.AlbumArtURL);
				AzureUploadManager.DeleteOrCancelJob(video.JobId);
				AzureUploadManager.DeleteAsset(video.AssetId);
				AzureUploadManager.DeleteAsset(video.EncodedAssetId);
                Context.ContestsEntries.RemoveRange(video.ContestEntries.ToList());
                Context.Videos.Remove(video);
			}
		}

		public void UpdateQueueAndSave()
        {
            List<Video> Queue = Context.Videos.ToList();

            foreach (Video video in Queue)
            {
                MediaAsset detail = new MediaAsset() { AssetId = video.AssetId, JobId = video.JobId };
                AzureUploadManager.UpdateMediaAsset(detail);
                video.State = (URMade.Models.JobState)detail.JobState;
                video.EncodedAssetId = detail.EncodedAssetId;
                video.SmoothStreamingUri = detail.SmoothStreamingUri;
            }

            Context.SaveChanges();
        }

        public void UpdateAssetDetailAndSave(IQueryable<Video> videos)
        {
            List<Video> Videos = videos.Where(p => p.State == JobState.Queued || p.State == JobState.Scheduled || p.State == JobState.Processing || p.State == JobState.Canceling).ToList();
            if (Videos.Count() > 0)
            {
                foreach (Video video in videos.Where(p => p.State == JobState.Queued || p.State == JobState.Scheduled || p.State == JobState.Processing || p.State == JobState.Canceling))
                {
                    MediaAsset detail = new MediaAsset() { AssetId = video.AssetId, JobId = video.JobId };
                    AzureUploadManager.UpdateMediaAsset(detail);
                    video.State = (URMade.Models.JobState)detail.JobState;
                    video.EncodedAssetId = detail.EncodedAssetId;
                    video.SmoothStreamingUri = detail.SmoothStreamingUri;
                }
                Context.SaveChanges();
            }
        }

		public PlaylistModel GetVideoPlaylist(PlaylistQuery options)
		{
			return new PlaylistModel(Context.Videos, options);
		}

		public PlaylistModel GetUserVideoPlaylist(PlaylistQuery options)
		{
			IQueryable<Video> videos;

			var user = SecurityHelper.GetLoggedInUser();
			if (user != null)
			{
				options.UserId	= user.Id;
				videos			= Context.Videos.Where(p => p.Owner.Id == user.Id);
				UpdateAssetDetailAndSave(videos);
			}
			else
				videos = Context.Videos;

			return new PlaylistModel(videos, options);
		}

		public int ToggleFavoriteAndSave(int id, out bool active)
		{
			ApplicationUser user	= SecurityHelper.GetLoggedInUser();
			Video video				= GetVideo(id);

			if (user == null || video == null)
			{
				active = false;
				return -1;
			}

			Video match = user.FavoriteVideos.FirstOrDefault(p => p.VideoId == id);

			if (match == null)
			{
				active = true;
				user.FavoriteVideos.Add(video);
			}
			else
			{
				active = false;
				user.FavoriteVideos.Remove(video);
			}

			Context.SaveChanges();
			return video.Fans.Count;
		}
	}
}