using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SportsStore.Domain.Abstract;
using SportsStore.WebUI.Models;

namespace SportsStore.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private IProductRepository _repository;
        public int PageSize { get; set; } = 4;

        public ProductController(IProductRepository repository)
        {
            _repository = repository;
        }

        public ViewResult List(int page = 1)
        {
            ProductsListViewModel model = new ProductsListViewModel
            {
                Products = _repository.Products.OrderBy(p => p.ProductId).Skip((page - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = _repository.Products.Count()
                }
            };

            return View(model);
        }
    }
}