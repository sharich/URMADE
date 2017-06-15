

namespace URMade.Models
{
    public class SelectOption
    {
        public int SelectOptionId { get; set; }

        public string OptionGroup { get; set; }

        public string Value { get; set; }
        public string Text { get; set; }

        public int SortOrder { get; set; }
    }
}