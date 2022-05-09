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
using HistoryClient.Entities;
using HistoryClient.Services;

namespace HistoryClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IOrderService _orderService;
        IItemService _itemService;
        ObservableCollection<Order> Orders;
        ObservableCollection<string> Items;
        CancellationTokenSource _tokenSource;

        public MainWindow(IOrderService orderService, IItemService itemService)
        {
            InitializeComponent();
            _orderService = orderService;
            _itemService = itemService;
            Orders = new ObservableCollection<Order>();
            Items = new ObservableCollection<string>();

            OrdersListView.ItemsSource = Orders;
            MyListBox.ItemsSource = Items;

            _tokenSource = new CancellationTokenSource();

        }
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var itemsFromTextBox = TextBorre.Text;
            var handledItems = _itemService.HandleItems(itemsFromTextBox);
            handledItems.ForEach(item => Items.Add(item));

            TextBorre.Clear();
            
        }
        private async void PlaceOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            MyTabControl.SelectedIndex = 1;

            try
            {
                var task = PlaceOrders();
                await task;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong when placing order.\n" + ex.Message);
            }
        }

        private async Task PlaceOrders()
        {
            var token = _tokenSource.Token;

            UpdateUiStart();

            var placedOrders = _orderService.SendOrder(Items.ToList());
            await foreach (var order in placedOrders)
            {
                Orders.Add(order);
            }

            StatusText.Text = "Waiting for orders to process";

            var finnishedOrders = _orderService.AwaitOrderStatus(Orders.ToList(), token);
            await foreach (var order in finnishedOrders)
            {
                var newOrder = Orders.FirstOrDefault(o => o.OrderId == order.OrderId);
                newOrder = order;
                OrdersListView.Items.Refresh();
            }

            UpdateUiFinished();
        }

        private void RemoveItemBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = MyListBox.SelectedItem;
                if (item != null)
                {
                    Items.Remove(item.ToString());
                }
            }
            catch
            {
                MessageBox.Show("No selected item to remove");
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            var orderList = Orders.ToList();
            _orderService.CreateHtmlTable(orderList);

        }

        private void UpdateUiFinished()
        {
            Spinner.Spin = false;
            Spinner.Visibility = Visibility.Collapsed;
            CancelBtn.Visibility = Visibility.Collapsed;
            StatusText.Text = "All tasks are finished";
            CopyBtn.Visibility = Visibility.Visible;
            CopyToTextBtn.Visibility = Visibility.Visible;
            ClearOrdersBtn.Visibility = Visibility.Visible;
        }

        private void UpdateUiStart()
        {
            Spinner.Spin = true;
            Spinner.Visibility = Visibility.Visible;
            CancelBtn.Visibility = Visibility.Visible;
            StatusText.Text = "Placing Orders";
        }

        private void CopyToTextBtn_Click(object sender, RoutedEventArgs e)
        {
            var orderList = Orders.ToList();
            _orderService.CopyTableToText(orderList);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _tokenSource.Cancel();
        }

        private void ClearOrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            Orders.Clear();
            ClearOrdersBtn.Visibility = Visibility.Collapsed;
            StatusText.Visibility = Visibility.Collapsed;
            CopyBtn.Visibility = Visibility.Collapsed;
            CopyToTextBtn.Visibility = Visibility.Collapsed;

        }

        private void ClearItemsBtn_Click(object sender, RoutedEventArgs e)
        {
            Items.Clear();
        }
    }
}
