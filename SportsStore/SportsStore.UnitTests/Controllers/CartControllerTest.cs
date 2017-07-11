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
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests.Controllers
{
    [TestClass]
    public class CartControllerTest
    {
        [TestMethod]
        public void CanAddToCart()
        {
            //Arrange - create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1", Category = "Apples"}
            }.AsQueryable());

            //Arrange - create a Cart
            Cart cart = new Cart();

            //Arrange - create the controller
            CartController target = new CartController(mock.Object, null);

            //Act - add a product to the cart
            target.AddToCart(cart, 1, null);

            //Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductId, 1);
        }

        [TestMethod]
        public void AddingProductToCartGoesToCartScreen()
        {
            //Arrange - create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1", Category = "Apples"}
            }.AsQueryable());

            //Arrange - create a Cart
            Cart cart = new Cart();

            //Arrange - create the controller
            CartController target = new CartController(mock.Object, null);

            //Act - add a product to the cart
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            //Assert
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void CanViewCartContents()
        {
            //Arrange - create a Cart
            Cart cart = new Cart();

            //Arrange - create the controller
            CartController target = new CartController(null, null);

            //Act - call the Index action method
            CartIndexViewModel result = (CartIndexViewModel) target.Index(cart, "myUrl").ViewData.Model;

            //Assert
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void CannotCheckoutEmptyCart()
        {
            //Arrange - create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            //Arrange - create an empty cart
            Cart cart = new Cart();

            //Arrange - create a shipping details
            ShippingDetails shippingDetails = new ShippingDetails();

            //Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            //Act
            ViewResult result = target.Checkout(cart, shippingDetails);

            //Assert - check that the order hasn't been passed on the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never);

            //Assert - check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);

            //Assert - check that we are passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void CannotCheckoutInvalidShippingDetails()
        {
            //Arrange - create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            //Arrange - crea a cart with an item
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            //Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            //Arrange - add an error to the model
            target.ModelState.AddModelError("error", "error");

            //Act - try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            //Assert - check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never);

            //Assert - check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);

            //Assert - check that we are passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void CanCheckoutAndSubmitOrder()
        {
            //Arrange - create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            //Arrange - create a cart with an item
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            //Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            //Act - try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            //Assert - check that the order has been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());

            //Assert - check that the method is returning the Completed view
            Assert.AreEqual("Completed", result.ViewName);

            //Assert - check that we are passing a valid model to the view
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }
    }
}
