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

namespace HistoryClient.Services
{
    public class OrderService : IOrderService
    {
        order_wsSoap ordersClient;
        items_wsSoap itemsClient;

        public OrderService(order_wsSoap _ordersClient, items_wsSoap _itemsClient)
        {
            ordersClient = _ordersClient;     
            itemsClient = _itemsClient;
        }
        public async IAsyncEnumerable<Order> SendOrder(List<string> items)
        {
            foreach (var item in items)
            {
                var securityType = await LookupSecurityTypeFromItem(item);

                if (securityType != "NOT FOUND")
                {
                    var rowsPre =  await GetAffectedRows("BB", item);

                    int orderId = await ordersClient.OrderMiscAsync("AHS_ADMIN", "BB", securityType, null, "HISTORY", item,
                    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

                    var order = new Order(item, orderId, rowsPre);
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
            //var itemsClient = new items_wsSoapClient(items_wsSoapClient.EndpointConfiguration.items_wsSoap);
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

                if (orderStatus.Code == "PROCESSED")
                {
                    var rowsAfter = await GetAffectedRows("BB", order.Item);
                    var affectedRows = Math.Abs(order.RowsPre - rowsAfter);
                    order.Message = $"Affected rows: {affectedRows}";
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

        private async Task<int> GetAffectedRows(string source, string itemName)
        {
            var sql = "SELECT COUNT(1) FROM ahs_data_float WHERE source = @Source AND item = @Item";
            var parameters = new
            {
                Source = source,
                Item = itemName
            };
            await using var connection =
                new SqlConnection("Persist Security Info = False; Integrated Security = SSPI; server = hadley; database = seb_ahshist_20210201_anna");
            IEnumerable<int> result = new List<int>();
            try
            {
                await connection.OpenAsync();
                result = await connection
                    .QueryAsync<int>(sql,
                        parameters, null, 30);
            }
            catch (Exception exception)
            {
                throw;
            }
            return result.FirstOrDefault();
        }

        public void CopyTableToText(List<Order> orders)
        {
            string orderResult = "";

            foreach (var order in orders)
            {
                orderResult += $"Item:{order.Item} Status:{order.Status} Message: {order.Message},\n";
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
                "<table style=\"width:100%\">" +
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
                throw ex;
            }
        }
    }
}
