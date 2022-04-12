using System.Collections.Generic;

namespace WpfTest.Services
{
    public interface IItemService
    {
        List<string> HandleItems(string items);
    }
}