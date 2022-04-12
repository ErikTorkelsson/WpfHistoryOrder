using System.Collections.Generic;
using System.Threading.Tasks;
using WpfTest.Entities;

namespace WpfTest.Services
{
    public interface IOrderService
    {
        IAsyncEnumerable<Order> AwaitOrderStatus(IEnumerable<Order> orders);
        IAsyncEnumerable<Order> SendOrder(List<string> items);
    }
}