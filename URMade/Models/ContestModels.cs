using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace URMade.Models
{
	public enum ContestType
	{
		None	= 0,
		Song	= 1,
		Video	= 2
	}

	public class Contest
	{
		[Key]
        public int ContestId			{get; set;}
		public string Name				{get; set;}
		public ContestType Type			{get; set;}
		public DateTime Start			{get; set;}
		public DateTime End				{get; set;}
		public string HomeBannerURL		{get; set;}
        public decimal EntryPrice       {get; set;}
        public decimal MemberEntryPrice {get; set;}
		public string ImageURL			{get; set;}
		public string Subtitle			{get; set;}
		public string Description		{get; set;}
		public string Rules				{get; set;}
		public string CSSClassName		{get; set;}
        
        public virtual ICollection<ContestEntry> Entries { get; set; }
    }

    public class ContestEntry
    {
        [Key]
        public int EntryId { get; set; }
        public virtual Contest Contest { get; set; }

        public decimal EntryPricePaid { get; set; }

        public virtual Song Song { get; set; }
        public virtual Video Video { get; set; }
        
        public virtual ICollection<ApplicationUser> Voters { get; set; }
    }
}