using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HistoryClient.Services;
using Xunit;

namespace HistoryClient.Tests.UnitTests.ItemServiceTests
{
    public class HandleItemsShould
    {
        [Theory]
        [InlineData("Test_Item1 \n Test_Item2 \n Test_Item3")]
        [InlineData("Test _Item1 \n Test _Item2 \n Test _Item3")]
        [InlineData(" Test Item1 \n  Test Item2 \n  Test Item3")]
        [InlineData("Test Item1  \n Test Item2  \n Test Item3 ")]
        [InlineData("   Test Item1 \n   Test Item2 \n   Test Item3")]
        [InlineData("   Test    Item1 \n   Test     Item2 \n   Test     Item3")]
        [InlineData("Test_Item1,Test_Item2,Test_Item3")]
        [InlineData(",Test_Item1,Test_Item2,Test_Item3,")]
        [InlineData(" Test_Item1,  Test_Item2,   Test_Item3")]
        [InlineData("   Test_Item1  ,Test_Item2 ,   Test_Item3")]
        [InlineData("Test _Item1,Test_ Item2, Test _Item3")]
        [InlineData("   Test _Item1,Test_   Item2, Test _Item3" )]
        public void Do_Something(string mockItems)
        {
            //Arrange
            var sut = new ItemService();
            var itemsToAssert = new List<string>()
            {
                "Test_Item1",
                "Test_Item2",
                "Test_Item3"
            };

            //Act
            var result = sut.HandleItems(mockItems);

            //Assert
            Assert.Equal(itemsToAssert, result);
        }
      
    }
}
