using System.Collections.Generic;

namespace HistoryClient.Services
{
    public interface IItemService
    {
        List<string> HandleItems(string items);
    }
}