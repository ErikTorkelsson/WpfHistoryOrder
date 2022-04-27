using HistoryClient.Entities;
using HistoryClient.Repositories;
using HistoryClient.Services;
using ItemsService;
using Moq;
using Newtonsoft.Json;
using OrderReference;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HistoryClient.Tests.UnitTests.OrderServiceTests
{
    public class LookupSecurityTypeFromItemShould
    {
        private readonly Mock<order_wsSoap> mockOrderClient;
        private readonly Mock<items_wsSoap> mockItemsClient;
        private readonly Mock<IOrderRepository> mockIOrderRepository;

        public LookupSecurityTypeFromItemShould()
        {
            mockOrderClient = new Mock<order_wsSoap>();
            mockItemsClient = new Mock<items_wsSoap>();
            mockIOrderRepository = new Mock<IOrderRepository>();
        }

        [Fact]
        public async void Do_Something()
        {
            ObservableCollection<Order> fakeOrders = new ObservableCollection<Order>();
            var sut = new OrderService(mockOrderClient.Object, mockItemsClient.Object, mockIOrderRepository.Object);
            var mockItems = new List<string>()
            {
                "Test_Item1",
                "Test_Item2"
            };
            var fakeItems = new Item[] { };

            var fakeAhsHistItems = new ahshist_items();
            fakeAhsHistItems.items = fakeItems;

            mockItemsClient.Setup(x => x.RequestItemsAsync("6.0", "BB", null, It.IsAny<string[]>(), null, null, null).Result).Returns(fakeAhsHistItems);
            mockIOrderRepository.Setup(x => x.GetAffectedRows(It.IsAny<string>(), It.IsAny<string>()).Result).Returns(0);
            mockOrderClient.Setup(x => x.OrderMiscAsync("AHS_ADMIN", "BB", It.IsAny<string>(), null, "HISTORY", It.IsAny<string>(),
                    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null).Result).Returns(1);
            //Act

            var result = sut.SendOrder(mockItems);
            await foreach (var item in result)
            {
                fakeOrders.Add(item);
            }

            //Assert
            var fakeOrdersList = fakeOrders.ToList();
            var obj1Str = JsonConvert.SerializeObject(new Order("Test_Item1", "No existing item found", "History order requires existing item"));
            var obj2Str = JsonConvert.SerializeObject(new Order("Test_Item2", "No existing item found", "History order requires existing item"));
            Assert.Collection(fakeOrdersList,
                item => Assert.Equal(obj1Str, JsonConvert.SerializeObject(item)),
                item => Assert.Equal(obj2Str, JsonConvert.SerializeObject(item))
            );
        }
    }
}
