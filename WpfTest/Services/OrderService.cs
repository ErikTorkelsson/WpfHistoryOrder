using Dapper;
using ItemsService;
using OrderReference;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;
using HistoryClient.Entities;
using HistoryClient.Repositories;
using System.Runtime.CompilerServices;

namespace HistoryClient.Services
{
    public class OrderService : IOrderService
    {
        order_wsSoap _ordersClient;
        items_wsSoap _itemsClient;
        IOrderRepository _orderRepository;

        public OrderService(order_wsSoap ordersClient, items_wsSoap itemsClient, IOrderRepository orderRepository)
        {
            _ordersClient = ordersClient;     
            _itemsClient = itemsClient;
            _orderRepository = orderRepository;
        }
        public async IAsyncEnumerable<Order> SendOrder(List<string> items)
        {
            foreach (var item in items)
            {
                var securityType = await LookupSecurityTypeFromItem(item);

                if (securityType != "NOT FOUND")
                {
                    Order order;

                    var rowsPre =  await _orderRepository.GetAffectedRows("BB", item);

                    try
                    {
                        int orderId = await _ordersClient.OrderMiscAsync("AHS_ADMIN", "BB", securityType, null, "HISTORY", item,
                        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

                        order = new Order(item, orderId, rowsPre);
                    }
                    catch
                    {
                        order = new Order(item, "None" , "Item is not valid");
                    }


                    yield return order;
                }
                else
                {
                    var order = new Order(item, "No existing item", "History order requires existing item");

                    yield return order;
                }
            }

        }

        private async Task<string> LookupSecurityTypeFromItem(string item)
        {
            //var itemsClient = new items_wsSoapClient(items_wsSoapClient.EndpointConfiguration.items_wsSoap);
            var result = await _itemsClient.RequestItemsAsync(
                "6.0", "BB", null, new string[] { item }, null, null, null);
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

        private async Task<Order> CheckOrderStatus(Order order, CancellationToken cancellationToken)
        {
            var orderStatus = new Status();
            while(!cancellationToken.IsCancellationRequested)
            {
                // kolla status
                orderStatus = await _ordersClient.OrderStatusAsync(order.OrderId);

                if (orderStatus.Code == "PROCESSED")
                {
                    var rowsAfter = await _orderRepository.GetAffectedRows("BB", order.Item);
                    var affectedRows = Math.Abs(order.RowsPre - rowsAfter);
                    order.Message = $"Affected rows: {affectedRows}";
                    order.Status = orderStatus.Code;
                    return order;
                }
                else if (orderStatus.Code != "PROCESSING")
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

        public async IAsyncEnumerable<Order> AwaitOrderStatus(IEnumerable<Order> orders, [EnumeratorCancellation]
                              CancellationToken cancellationToken = default)
        {
            var runningOrders = new List<Task<Order>>();

            foreach (var order in orders)
            {
                if (order.Status == "PROCESSING")
                {
                    var task = CheckOrderStatus(order, cancellationToken);
                    runningOrders.Add(task);
                }
            }

            while (runningOrders.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var OrderTask = await Task.WhenAny(runningOrders);
                var finishedOrderTask = await OrderTask;
                runningOrders.Remove(OrderTask);

                yield return finishedOrderTask;
            }
        }

        public void CopyTableToText(List<Order> orders)
        {
            string orderResult = "";

            foreach (var order in orders)
            {
                orderResult += $"Item: {order.Item}, Status: {order.Status}, Message: {order.Message}.\n";
            }

            System.Windows.Clipboard.SetText(orderResult);
        }

        public void CreateHtmlTable(List<Order> orders)
        {
            string orderInfo = "";
            foreach (var order in orders)
            {
                orderInfo +=
                    "<tr>" +
                        $"<td>{order.Item}</td>" +
                        $"<td>{order.Status}</td>" +
                        $"<td>{order.Message}</td>" +
                    "</tr>";
            }
            string htmlFragment =
                "<html>" +
                "<style>table, th, td {border: 1px solid black;}</ style > " +
                "<body>" +
                "<table style=\"width:70%\">" +
                    "<tr>" +
                        "<th>Item</th>" +
                        "<th>Status</th>" +
                        "<th>Message</th>" +
                    "</tr>" +
                    orderInfo +
                "</table>" +
                "<body>" +
                "</html>";

            string htmlFormat = HtmlFormatHelper.CreateHtmlFormat(htmlFragment);

            // Create a DataPackage object.
            var dataPackage = new DataPackage();

            // Set the content of the DataPackage as HTML format.
            dataPackage.SetHtmlFormat(htmlFormat);

            try
            {
                // Set the DataPackage to the clipboard.
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy");
            }
        }
    }
}
