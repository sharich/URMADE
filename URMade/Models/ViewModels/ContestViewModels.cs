using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Collections.Generic;

namespace URMade.Models
{
	public class ContestViewModel
	{
		public int ContestId		{get; set;}
		public string Name			{get; set;}
		public ContestType Type		{get; set;}
		public DateTime Start		{get; set;}
		public DateTime End			{get; set;}
		public string ImageURL		{get; set;}
		public string Subtitle		{get; set;}
		public string Description	{get; set;}
		public string[] Rules		{get; set;}
		public string CSSClassName	{get; set;}

		public ContestViewModel()
		{

		}

		public ContestViewModel(Contest contest)
		{
			ContestId		= contest.ContestId;
			Name			= contest.Name;
			Type			= contest.Type;
			Start			= contest.Start;
			End				= contest.End;
			ImageURL		= contest.HomeBannerURL;
			Subtitle		= contest.Subtitle;
			Description		= contest.Description;
			Rules			= contest.Rules?.Split('\n');
			CSSClassName	= contest.CSSClassName;
		}

		public string DisplayType()
		{
			if (Type == ContestType.None)
				return "Private";

			return Type.ToString();
		}
	}

	public class ContestListViewModel
	{
		public List<ContestViewModel> Contests	{get; set;}
		public string TitleMatch				{get; set;}

		public ContestListViewModel()
		{

		}

		public ContestListViewModel(IEnumerable<Contest> contests, string titleMatch)
		{
			Contests	= new List<ContestViewModel>();
			TitleMatch	= titleMatch;

			foreach (var contest in contests)
				Contests.Add(new ContestViewModel(contest));
		}
	}

	public class EditContestViewModel
	{
		public int ContestId			{get; set;}
		[Required]public string Name	{get; set;}
		public ContestType Type			{get; set;}
		[Required] public string Start	{get; set;}
		[Required]public string End		{get; set;}
		public string ImageURL			{get; set;}
		public HttpPostedFileBase Image {get; set;}
		public bool DeleteImage			{get; set;}
		public decimal EntryPrice		{get; set;}
		public decimal MemberEntryPrice {get; set;}
		public string Subtitle			{get; set;}
		public string Description		{get; set;}
		public string Rules				{get; set;}
		public string CSSClassName		{get; set;}

		public EditContestViewModel()
		{

		}

		public EditContestViewModel(Contest contest)
		{
			if (contest == null)
				return;

			ContestId			= contest.ContestId;
			Name				= contest.Name;
			Type				= contest.Type;
			Start				= contest.Start.ToString("yyyy-MM-dd");
			End					= contest.End.ToString("yyyy-MM-dd");
			ImageURL			= contest.HomeBannerURL;
			Image				= null;
			EntryPrice			= contest.EntryPrice;
			MemberEntryPrice	= contest.MemberEntryPrice;
			Subtitle			= contest.Subtitle;
			Description			= contest.Description;
			Rules				= contest.Rules;
			CSSClassName		= contest.CSSClassName;
		}
	}

	public class JoinContestViewModel
	{
		public ContestViewModel Details		{get; set;}
		public ContestEntry PreviousEntry	{get; set;}
		public bool AvailableEntry			{get; set;}
		public int SubmissionId				{get; set;}
		public decimal Price				{get; set;}
		public string CardNonce				{get; set;}

		[Required]
		public bool AgreedToRules			{get; set;}

		public JoinContestViewModel()
		{

		}

		public JoinContestViewModel(Contest contest, ApplicationUser user, ContestEntry previousEntry, bool availableEntry)
		{
			Details			= new ContestViewModel(contest);
			PreviousEntry	= previousEntry;
			AvailableEntry	= availableEntry;
			SubmissionId	= 0;
			Price			= user.IsPremiumMember() ? contest.MemberEntryPrice : contest.EntryPrice;
			CardNonce		= null;
		}
	}

	public class ContestVotingViewModel
	{
		public ContestViewModel Details {get; set;}
		public PlaylistModel Playlist	{get; set;}

		public ContestVotingViewModel()
		{

		}

		public ContestVotingViewModel(ContestViewModel details, PlaylistModel playlist)
		{
			Details		= details;
			Playlist	= playlist;
		}
	}
}