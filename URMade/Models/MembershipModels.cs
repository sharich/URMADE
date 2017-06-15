using System;
using System.ComponentModel.DataAnnotations;

namespace URMade.Models
{
    public class Membership
    {
		[Key]
		public int MemberId				{get; set;}
		public string UserId			{get; set;}
		public string CustomerId		{get; set;}
		public string CustomerCardId	{get; set;}
		public DateTime NextPayment		{get; set;}
		public bool PendingCancel		{get; set;}
	}
}