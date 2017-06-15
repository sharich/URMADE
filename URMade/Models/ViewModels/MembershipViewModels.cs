using System;
using System.ComponentModel.DataAnnotations;

namespace URMade.Models
{
    public class MembershipViewModel
    {
		[Display(Name = "First Name")]
		public string FirstName				{get; set;}
		[Display(Name = "Last Name")]
		public string LastName				{get; set;}
		[Display(Name = "Email Address")]
		public string Email					{get; set;}
		[Display(Name = "Phone Number")]
		public string PhoneNumber			{get; set;}
		[Display(Name = "Name on Card")]
		public string NameOnCard			{get; set;}
		public string CardNonce				{get; set;}
		public DateTime NextPayment			{get; set;}
		public bool IsRenewal				{get; set;}
		public bool IsPaid					{get; set;}
		public bool IsPendingCancel			{get; set;}
		public bool AutomaticRenew			{get; set;}
		public bool UpdateCardInformation	{get; set;}
		public bool ToggleAutomaticRenewal	{get; set;}

		public MembershipViewModel()
		{

		}

		public MembershipViewModel(Membership membership)
		{
			if (membership != null)
			{
				NextPayment = membership.NextPayment;

				if (membership.NextPayment >= DateTime.Today)
					IsPaid = true;
				else
					IsRenewal = true;

				IsPendingCancel = membership.PendingCancel;

				AutomaticRenew = !string.IsNullOrWhiteSpace(membership.CustomerId) &&
								 !string.IsNullOrWhiteSpace(membership.CustomerCardId) &&
								 !IsPendingCancel;
			}
		}
	}
}