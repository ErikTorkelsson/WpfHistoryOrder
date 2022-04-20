using ItemsService;
using OrderReference;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfTest.Entities;
using WpfTest.Services;

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IOrderService _orderService;
        IItemService _itemService;
        ObservableCollection<Order> orders;
        ObservableCollection<string> items;

        public MainWindow(IOrderService orderService, IItemService itemService)
        {
            InitializeComponent();
            _orderService = orderService;
            _itemService = itemService;
            orders = new ObservableCollection<Order>();
            items = new ObservableCollection<string>();

            OrdersListView.ItemsSource = orders;
            MyListBox.ItemsSource = items;
        }
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var itemsFromTextBox = TextBorre.Text;
            var handledItems = _itemService.HandleItems(itemsFromTextBox);
            handledItems.ForEach(item => items.Add(item));

            TextBorre.Clear();
            
        }
        private async void PlaceOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            MyTabControl.SelectedIndex = 1;
            var task = PlaceOrders();
            await task;
        }

        private async Task PlaceOrders()
        {
            Spinner.Spin = true;
            Spinner.Visibility = Visibility.Visible;
            StatusText.Text = "Placing Orders";
            var placedOrders = _orderService.SendOrder(items.ToList());
            await foreach (var order in placedOrders)
            {
                orders.Add(order);
            }

            StatusText.Text = "Waiting for orders to process";

            var finnishedOrders = _orderService.AwaitOrderStatus(orders.ToList());
            await foreach (var order in finnishedOrders)
            {
                var newOrder = orders.FirstOrDefault(o => o.OrderId == order.OrderId);
                newOrder = order;
                OrdersListView.Items.Refresh();
            }

            Spinner.Spin = false;
            Spinner.Visibility = Visibility.Hidden;
            StatusText.Text = "All tasks are finished";
            CopyBtn.Visibility = Visibility.Visible;
            CopyToTextBtn.Visibility = Visibility.Visible;
        }

        private void RemoveItemBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = MyListBox.SelectedItem;
                if (item != null)
                {
                    items.Remove(item.ToString());
                }
            }
            catch
            {
                MessageBox.Show("No selected item to remove");
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            var orderList = orders.ToList();
            _orderService.CreateHtmlTable(orderList);

        }

        private void CopyToTextBtn_Click(object sender, RoutedEventArgs e)
        {
            var orderList = orders.ToList();
            _orderService.CopyTableToText(orderList);
        }
    }
}
