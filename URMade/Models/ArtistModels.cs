using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace URMade.Models
{
    public class Artist
    {
        public Artist()
        {
            //this.SlideshowImages = new HashSet<ArtistSlideshowImage>();
            this.SocialLinks = new HashSet<SocialLink>();

            this.Songs = new HashSet<Song>();
            this.Videos = new HashSet<Video>();
            this.Fans = new HashSet<ApplicationUser>();
        }

        [Key]
        public int ArtistId { get; set; }

        public string Slug { get; set; }
        public string Name { get; set; }
        public string ArtistType { get; set; }
        public string BiographyShort { get; set; }
		public string BiographyLong { get; set; }
        public string Status { get; set; }

        public string ImageURL { get; set; }

        [InverseProperty("Artists")]
        public virtual ICollection<ApplicationUser> Members { get; set; }

        public virtual ICollection<ArtistSlideshowImage> SlideshowImages { get; set; }
        //public virtual ICollection<ArtistImage> ArtistImage { get; set; }
        public virtual ICollection<SocialLink> SocialLinks { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<Video> Videos { get; set; }
        [InverseProperty("FavoriteArtists")]
        public virtual ICollection<ApplicationUser> Fans { get; set; }


    }

    //public class ArtistImage
    //{
    //    [Key]
    //    public int ArtistImageId { get; set; }
    //    public string uri { get; set; }
    //    public virtual Artist Artist { get; set; }
    //}
    public class ArtistSlideshowImage
    {
        [Key]
        public int ArtistSlideshowImageId { get; set; }
        public string ImageURL { get; set; }

        public virtual Artist Artist { get; set; }
    }

    public class Song
    {
        [Key]
        public int SongId { get; set; }
        public JobState State { get; set; }

        public string Title { get; set; }
        public string SubGenre { get; set; }

        public string AlbumArtURL { get; set; }

        public string AssetId { get; set; }
        public string JobId { get; set; }
        public string EncodedAssetId { get; set; }
        public string SmoothStreamingUri { get; set; }
		
        public int FanCount { get; set; }
        public DateTime DateCreated { get; set; }
        
        public virtual ApplicationUser Owner { get; set; }
        public virtual Artist Artist { get; set; }
        [InverseProperty("FavoriteSongs")]
        public virtual ICollection<ApplicationUser> Fans { get; set; }

        public virtual ICollection<ContestEntry> ContestEntries { get; set; }


    }
    public class Video
    {
        [Key]
        public int VideoId { get; set; }
        public JobState State { get; set; }

        public string Title { get; set; }
		public string SubGenre {get; set;}

        public string AlbumArtURL { get; set; }
        
        public string AssetId { get; set; }
        public string JobId { get; set; }
        public string EncodedAssetId { get; set; }
        public string SmoothStreamingUri { get; set; }

        public int FanCount { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public virtual Artist Artist { get; set; }
        [InverseProperty("FavoriteVideos")]
        public virtual ICollection<ApplicationUser> Fans { get; set; }

        public virtual ICollection<ContestEntry> ContestEntries { get; set; }

    }
    public class SocialLink
    {
        public int SocialLinkId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }

        public virtual Artist Artist { get; set; }
    }

    public enum JobState
    {
        Queued		= 0,
        Scheduled	= 1,
        Processing	= 2,
        Finished	= 3,
        Error		= 4,
        Canceled	= 5,
        Canceling	= 6
    }
}