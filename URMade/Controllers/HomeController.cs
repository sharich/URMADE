using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using URMade.Models;

namespace URMade.Controllers
{
    public class HomeController : Controller
    {
		public ArtistRepository ArtistRepo		{get; set;}
		public SongRepository SongRepo			{get; set;}
		public VideoRepository VideoRepo		{get; set;}
		public ContestRepository ContestRepo	{get; set;}

        public HomeController(ArtistRepository artistRepo, SongRepository songRepo, VideoRepository videoRepo, ContestRepository contestRepo)
        {
            ArtistRepo	= artistRepo;
			SongRepo	= songRepo;
			VideoRepo	= videoRepo;
			ContestRepo	= contestRepo;
        }

        public HomeController()
        {

        }

        [OutputCache(Duration = 10, VaryByParam = "none", VaryByCustom = "loggedin")]
        public ActionResult Index()
        {

            List<string> SlideShow = new List<string>();

            SlideShow.Add("/Content/Home/Slides/slide3.jpg");
            SlideShow.Add("/Content/Home/Slides/slide2.jpg");
            SlideShow.Add("/Content/Home/Slides/slide1.jpg");

            return View(new HomeViewModel(SlideShow,
										  ContestRepo.GetRecentContests(3),
										  SongRepo.GetSongPlaylist(new PlaylistQuery()),
										  VideoRepo.GetVideoPlaylist(new PlaylistQuery()),
										  ArtistRepo.GetTopBands(3),
										  ArtistRepo.GetTopWriters(3)));
        }

        public ActionResult Rankings()
		{
			return View(new RankingsPlaylistModel(SongRepo.GetSongPlaylist(new PlaylistQuery()), VideoRepo.GetVideoPlaylist(new PlaylistQuery())));
		}

		[HttpPost]
        [OutputCache(Duration = 10, VaryByParam = "*")]
        public JsonResult Search(string filters, string match)
		{
			if (string.IsNullOrWhiteSpace(match))
				return Json(new SearchResultViewModel());

			var result = new SearchResultViewModel();

			if (filters.IndexOf("Artists") >= 0)
				result.Artists = ArtistRepo.Search(match, 5);

			if (filters.IndexOf("Songs") >= 0)
				result.Songs = SongRepo.Search(match, 5);

			if (filters.IndexOf("Videos") >= 0)
				result.Videos = VideoRepo.Search(match, 5);
			
			if (filters.IndexOf("Contests") >= 0)
				result.Contests = ContestRepo.Search(match, 5);

			return Json(result);
		}
    }
}