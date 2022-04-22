using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryClient.Services
{
    public class ItemService : IItemService
    {
        public List<string> HandleItems(string items)
        {
            var HandledItems = new List<string>();

            string[] separatedItems;

            if (items.Contains(','))
            {
                separatedItems = items.Split(',');
            }
            else
            {
                separatedItems = items.Split("\n");
            }

            foreach (var textString in separatedItems)
            {
                if (!string.IsNullOrWhiteSpace(textString))
                {
                    var concatedString = ConcatString(textString);
                    HandledItems.Add(concatedString);
                }
            }

            HandledItems = HandledItems.Distinct().ToList();

            return HandledItems;
        }

        private static string ConcatString(string _string)
        {
            var trimmed = _string.Trim();
            trimmed = trimmed.Replace("\r", "");
            var index = trimmed.IndexOf(" ");
            if (index != -1 && !trimmed.Contains("_"))
            {
                StringBuilder sb = new StringBuilder(trimmed);
                sb[index] = '_';
                trimmed = sb.ToString();
                trimmed = trimmed.Replace(" ", "");
            }
            else if (index != -1 && trimmed.Contains("_"))
            {
                trimmed = trimmed.Replace(" ", "");
            }

            return trimmed;
        }
    }
}
