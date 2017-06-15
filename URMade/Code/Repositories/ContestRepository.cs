using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using URMade.Models;
using System.Web.Mvc;

namespace URMade
{
	public class ContestRepository
	{
		private ApplicationDbContext Context {get; set;}

		public ContestRepository(ApplicationDbContext context)
		{
			Context = context;
		}

		public List<SearchItemViewModel> Search(string match, int max)
		{
			List<Contest> contests = Context.Contests.Where(p => p.Name.Contains(match)).OrderByDescending(p => p.Start).Take(max).ToList();

			if (contests == null || contests.Count < 1)
				return null;

			List<SearchItemViewModel> result = new List<SearchItemViewModel>();
			foreach (Contest contest in contests)
				result.Add(new SearchItemViewModel(contest));

			return result;
		}

        public Contest GetContest(int id)
        {
            return Context.Contests.FirstOrDefault(p => p.ContestId == id);
        }

        public ContestEntry GetContestEntry(int id)
        {
            return Context.ContestsEntries.FirstOrDefault(p => p.EntryId == id);
        }

        public List<ContestViewModel> GetRecentContests(int max)
		{
			var contests = Context.Contests.OrderByDescending(p => p.Start);
			
			if (max > 0)
				contests.Take(max);

			var result = new List<ContestViewModel>();

			foreach (Contest contest in contests)
				result.Add(new ContestViewModel(contest));

			return result;
		}

		public ContestEntry GetPreviousContestEntry(int contestId, ApplicationUser user)
		{
			var contest = GetContest(contestId);

			if (user == null || contest == null || contest.Type == ContestType.None)
				return null;

			var entry = contest.Type == ContestType.Song														?
						contest.Entries.FirstOrDefault(p => p.Song != null && p.Song.Owner.Id == user.Id)		:
						contest.Entries.FirstOrDefault(p => p.Video != null && p.Video.Owner.Id == user.Id);

			return entry;
		}

		public List<ContestEntry> GetTopContestEntries(int id, int max = 0)
		{
			var contest = GetContest(id);
			var entries = contest.Entries.OrderByDescending(p => p.Voters.Count());

			if (max > 0)
				return entries.Take(max).ToList();
			else
				return entries.ToList();
		}

		public void CreateContestAndSave(EditContestViewModel model)
		{
			Contest contest = new Contest();

			if (model.Image != null)
				model.ImageURL = AzureUploadManager.UploadFile(model.Image, "ContestImage");

			contest.Name				= model.Name;
			contest.Type				= model.Type;
			contest.Start				= DateTime.Parse(model.Start);
			contest.End					= DateTime.Parse(model.End);
			contest.HomeBannerURL		= model.ImageURL;
			contest.EntryPrice			= model.EntryPrice;
			contest.MemberEntryPrice	= model.MemberEntryPrice;
			contest.Subtitle			= model.Subtitle;
			contest.Description			= model.Description;
			contest.Rules				= model.Rules;
			contest.CSSClassName		= model.CSSClassName;

			Context.Contests.Add(contest);
			Context.SaveChanges();
		}

		public JoinContestViewModel GetJoinContestViewModel(int contestId, ApplicationUser user)
		{
			var contest = GetContest(contestId);

			if (user == null || contest == null || contest.Type == ContestType.None)
				return null;

			var entry = GetPreviousContestEntry(contestId, user);

			if (entry == null)
			{
				if (contest.Type == ContestType.Song)
					return new JoinContestViewModel(contest, user, null, Context.Songs.Any(p => p.Owner.Id == user.Id));
				else
					return new JoinContestViewModel(contest, user, null, Context.Videos.Any(p => p.Owner.Id == user.Id));
			}
			else
				return new JoinContestViewModel(contest, user, entry, true);
		}

		public ContestListViewModel GetActiveContests(string name)
		{
			var contests = Context.Contests.Where(p => p.Type != ContestType.None && p.Start < DateTime.Today && p.End > DateTime.Today);

			if (!string.IsNullOrWhiteSpace(name))
				contests = contests.Where(p => p.Name.Contains(name));

			return new ContestListViewModel(contests.OrderByDescending(p => p.Start), name);
		}

		public PlaylistModel GetContestEntryPlaylist(Contest contest, PlaylistQuery options)
		{
			return new PlaylistModel(Context.ContestsEntries, contest, options);
		}

		public ContestVotingViewModel GetContestVotingPlaylist(int contestId, PlaylistQuery options)
		{
			var contest = GetContest(contestId);
			return contest != null ? new ContestVotingViewModel(new ContestViewModel(contest), GetContestEntryPlaylist(contest, options)) : null;
		}

		public void EditContestAndSave(EditContestViewModel model)
		{
			Contest contest = GetContest(model.ContestId);

			if (model.Image != null)
			{
				if (!string.IsNullOrWhiteSpace(contest.HomeBannerURL))
					AzureUploadManager.DeleteBlob(contest.HomeBannerURL);

				model.ImageURL			= AzureUploadManager.UploadFile(model.Image, "ContestImage");
				contest.HomeBannerURL	= model.ImageURL;
			}
			else if (model.DeleteImage && !string.IsNullOrWhiteSpace(contest.HomeBannerURL))
			{
				AzureUploadManager.DeleteBlob(contest.HomeBannerURL);
				contest.HomeBannerURL = "";
			}

			contest.Name				= model.Name;
			contest.Type				= model.Type;
			contest.Start				= DateTime.Parse(model.Start);
			contest.End					= DateTime.Parse(model.End);
			contest.EntryPrice			= model.EntryPrice;
			contest.MemberEntryPrice	= model.MemberEntryPrice;
			contest.Subtitle			= model.Subtitle;
			contest.Description			= model.Description;
			contest.Rules				= model.Rules;
			contest.CSSClassName		= model.CSSClassName;

			Context.SaveChanges();
		}

		public void DeleteContestAndSave(int id)
		{
			Contest contest = GetContest(id);

			if (contest == null)
				return;

			Context.Contests.Remove(contest);
			Context.SaveChanges();
        }

		public bool AddContestEntryAndSave(JoinContestViewModel model)
		{
			Contest contest = GetContest(model.Details.ContestId);
			var user		= SecurityHelper.GetLoggedInUser();

			string chargeResult = "";

			if (contest == null || user == null)
				return false;

			decimal price	= user.IsPremiumMember() ? contest.MemberEntryPrice : contest.EntryPrice;
			var entry		= new ContestEntry();

			entry.EntryPricePaid = price;

			if (contest.Type == ContestType.Song)
			{
				entry.Song = Context.Songs.FirstOrDefault(p => p.SongId == model.SubmissionId && p.Owner.Id == user.Id);
				if (entry.Song == null)
					return false;
			}
			else if (contest.Type == ContestType.Video)
			{
				entry.Video = Context.Videos.FirstOrDefault(p => p.VideoId == model.SubmissionId && p.Owner.Id == user.Id);
				if (entry.Video == null)
					return false;
			}

			if (price > 0 && !PaymentManager.Charge(model.CardNonce, price, "USD", ref chargeResult))
				return false;

			contest.Entries.Add(entry);
			Context.SaveChanges();
			return true;
		}

		/// <summary>
		/// Cast Vote for a Contest Entry
		/// </summary>
		/// <param name="id">Contest Entry Id</param>
		/// <param name="success">returns true if vote was accepted</param>
		/// <returns></returns>
		public int VoteAndSave(int id, out bool success)
        {
            ApplicationUser user = SecurityHelper.GetLoggedInUser();
            ContestEntry contestEntry = GetContestEntry(id);

            int MyContestVotes = contestEntry.Contest.Entries.Count(p => p.Voters.Contains(user));

            success = false;
            if ( MyContestVotes < 1)
            {
                contestEntry.Voters.Add(user);
                success = true;
            }

            Context.SaveChanges();
            return contestEntry.Voters.Count;
        }
    }
}