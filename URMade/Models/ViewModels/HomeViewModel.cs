using System.Collections.Generic;

namespace URMade.Models
{
	public class HomeViewModel
	{
		public List<string> SlideshowURLs		{get; set;}
		public List<ContestViewModel> Contests	{get; set;}
		public PlaylistModel Songs				{get; set;}
		public PlaylistModel Videos				{get; set;}
		public List<FeaturedArtist> TopBands	{get; set;}
		public List<FeaturedArtist> TopWriters	{get; set;}

		public HomeViewModel(List<string> slideshowURLs, List<ContestViewModel> contests,
							 PlaylistModel songs, PlaylistModel videos,
							 List<FeaturedArtist> topBands, List<FeaturedArtist> topWriters)
		{
			SlideshowURLs		= slideshowURLs;
			Contests			= contests;
			Songs				= songs;
			Videos				= videos;
			TopBands			= topBands;
			TopWriters			= topWriters;
		}
	}
}