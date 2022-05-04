using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HistoryClient.Services;
using OrderReference;
using ItemsService;
using HistoryClient.Repositories;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HistoryClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public IConfiguration configuration;
        public App()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            configuration = builder.Build();

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            string ordersUrl = configuration.GetValue<string>("ConnectionURLs:OrderReference");
            string itemsUrl = configuration.GetValue<string>("ConnectionURLs:ItemsService");

            services.AddSingleton<MainWindow>();
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<order_wsSoap>(s => new order_wsSoapClient(order_wsSoapClient.EndpointConfiguration.order_wsSoap, ordersUrl));
            services.AddSingleton<items_wsSoap>(s => new items_wsSoapClient(items_wsSoapClient.EndpointConfiguration.items_wsSoap, itemsUrl));
            services.AddSingleton<IConfiguration>(provider => configuration);
        }
        private void OnStartup(object sender, StartupEventArgs e)
        {            
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
