using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.HtmlHelpers;
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests.Controllers
{
    [TestClass]
    public class ProductControllerTest
    {
        [TestMethod]
        public void CanPaginate()
        {
            //arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1"},
                new Product {ProductId = 2, Name = "P2"},
                new Product {ProductId = 3, Name = "P3"},
                new Product {ProductId = 4, Name = "P4"},
                new Product {ProductId = 5, Name = "P5"}
            }.AsQueryable());

            var controller = new ProductController(mock.Object) {PageSize = 3};

            //act
            ProductsListViewModel result = (ProductsListViewModel) controller.List(2).Model;

            //assert
            Product[] prodArray = result.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");
        }

        [TestMethod]
        public void CanGeneratePageLinks()
        {
            //arrange - define an HTML helper - we need to do this
            //in order to apply the extension method
            HtmlHelper myHelper = null;

            //arrange - create PaginInfo data
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            //arrange - set up the delegate using a lambda expression
            Func<int,string> pageUrlDelegate = i => "Page" + i;

            //act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //assert
            var s = @"<a href=""Page1"">1</a><a class=""selected"" href=""Page2"">2</a><a href=""Page3"">3</a>";
            Assert.AreEqual(result.ToString(), s);
        }

        [TestMethod]
        public void CanSendPaginationViewModel()
        {
            //arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1"},
                new Product {ProductId = 2, Name = "P2"},
                new Product {ProductId = 3, Name = "P3"},
                new Product {ProductId = 4, Name = "P4"},
                new Product {ProductId = 5, Name = "P5"}
            }.AsQueryable());

            //arrange
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //act
            ProductsListViewModel result = (ProductsListViewModel) controller.List(2).Model;

            //assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }
    }
}
