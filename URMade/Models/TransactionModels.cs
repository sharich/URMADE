using System;
using System.ComponentModel.DataAnnotations;

namespace URMade.Models
{
	public class Transaction
	{
		// Do not edit existing items, only add onto the list.
		public enum PurchaseType
		{
			MembershipInitial		= 1,
			MembershipManualRenewal	= 2,
			MembershipAutoRenewal	= 3,
			ContestEntryFee			= 4,
			ContestEntryMemberFee	= 5
		}

		[Key]
		int OrderId				{get; set;}
		string UserId			{get; set;}
		string TransacationId	{get; set;}
		string LocationId		{get; set;}
		PurchaseType Purchase	{get; set;}
		DateTime Date			{get; set;}
	}
}