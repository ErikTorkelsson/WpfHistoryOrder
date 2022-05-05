using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryClient.Entities
{
    public class Order
    {
        public string Item { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }


        public Order(string item , int orderId, string date)
        {
            Item = item;
            OrderId = orderId;
            Status = "PROCESSING";
            Message = "";
            Date = date;
        }
        public Order(string item, string status, string message)
        {
            Item = item;
            Status = status;
            Message = message;
        }
    }
}
