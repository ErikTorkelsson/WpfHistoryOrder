using System.Collections.Generic;
using System.Threading.Tasks;
using HistoryClient.Entities;

namespace HistoryClient.Services
{
    public interface IOrderService
    {
        IAsyncEnumerable<Order> AwaitOrderStatus(IEnumerable<Order> orders);
        IAsyncEnumerable<Order> SendOrder(List<string> items);
        void CopyTableToText(List<Order> orders);
        void CreateHtmlTable(List<Order> orders);
    }
}