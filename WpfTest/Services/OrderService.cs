using ItemsService;
using OrderReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfTest.Entities;

namespace WpfTest.Services
{
    public class OrderService : IOrderService
    {
        order_wsSoapClient ordersClient;

        public OrderService()
        {
            ordersClient = new order_wsSoapClient(order_wsSoapClient.EndpointConfiguration.order_wsSoap);
        }
        public async IAsyncEnumerable<Order> SendOrder(List<string> items)
        {
            //var orders = new List<Order>();
            //order_wsSoapClient ordersClient = new order_wsSoapClient(order_wsSoapClient.EndpointConfiguration.order_wsSoap);

            foreach (var item in items)
            {
                var securityType = await LookupSecurityTypeFromItem(item);

                if (securityType != "NOT FOUND")
                {
                    int orderId = await ordersClient.OrderMiscAsync("AHS_ADMIN", "BB", securityType, null, "HISTORY", item,
                    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

                    var order = new Order(item, orderId);
                    order.Message = "No existing item found, No existing item found, No existing item found";
                    //orders.Add(order);
                    yield return order;
                }
                else
                {
                    var order = new Order(item, "No existing item found", "History order requires existing item");
                    //orders.Add(order);
                    yield return order;
                }
            }

        }

        private async Task<string> LookupSecurityTypeFromItem(string item)
        {
            var itemsClient = new items_wsSoapClient(items_wsSoapClient.EndpointConfiguration.items_wsSoap);
            var result = await itemsClient.RequestItemsAsync(
                "6.0", "BB", null, new string[] { "omxs30_index" }, null, null, null);
            var matchingItem = result.items;

            if (matchingItem.Count() > 0)
            {
                var seriesType = matchingItem[0].series_type;
                var split = seriesType.Split("_");
                var securityType = split[1];

                return securityType;
            }
            else
            {
                return "NOT FOUND";
            }
        }

        private async Task<Order> CheckOrderStatus(Order order)
        {
            var orderStatus = new Status();
            for (var i = 0; i < 12; i++)
            {
                // kolla status
                orderStatus = await ordersClient.OrderStatusAsync(order.OrderId);

                if (orderStatus.Code != "PROCESSING")
                {
                    order.Message = orderStatus.Message;
                    order.Status = orderStatus.Code;
                    return order;
                }
                //sleep
                await Task.Delay(5000);
            }

            return order;
        }

        public async IAsyncEnumerable<Order> AwaitOrderStatus(IEnumerable<Order> orders)
        {
            var runningOrders = new List<Task<Order>>();

            foreach (var order in orders)
            {
                if (order.Status == "PROCESSING")
                {
                    var task = CheckOrderStatus(order);
                    runningOrders.Add(task);
                }
            }

            while (runningOrders.Count > 0)
            {
                var OrderTask = await Task.WhenAny(runningOrders);
                var finishedOrderTask = await OrderTask;
                runningOrders.Remove(OrderTask);

                yield return finishedOrderTask;
            }
        }
    }
}
