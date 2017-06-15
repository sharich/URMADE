using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace URMade
{
    public interface IJsonSafeable
    {
        void PopulateDictionaryWithStringAttributes(Dictionary<string, string> item);
    }

    public class JsonSafeService
    {
        public static List<List<string>> Packed(IEnumerable<IJsonSafeable> items)
        {
            var result = new List<List<string>>();

            if (items != null && items.Count() > 0)
            {
                var sample = new Dictionary<string, string>();
                items.First().PopulateDictionaryWithStringAttributes(sample);
                List<string> keys = sample.Keys.ToList();
                result.Add(keys);

                foreach (var item in items)
                {
                    var dict = new Dictionary<string, string>();
                    item.PopulateDictionaryWithStringAttributes(dict);
                    result.Add(dict.Values.ToList());
                }
            }

            return result;
        }

        public static Dictionary<string, string> Dict(IJsonSafeable item)
        {
            var result = new Dictionary<string, string>();

            if (item != null)
            {
                item.PopulateDictionaryWithStringAttributes(result);
            }

            return result;
        }

        public static List<Dictionary<string, string>> Dict(IEnumerable<IJsonSafeable> items)
        {
            var result = new List<Dictionary<string, string>>();

            if (items != null && items.Count() > 0)
            {
                foreach (var item in items)
                {
                    result.Add(Dict(item));
                }
            }

            return result;
        }
    }
}