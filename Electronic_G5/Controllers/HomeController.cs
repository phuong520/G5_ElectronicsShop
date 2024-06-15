﻿using Electronic_G5.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Electronic_G5.Controllers
{
    public class HomeController : Controller
    {
        private ElectronicDb db = new ElectronicDb();



        [AllowAnonymous]

        public ActionResult Index(string filter, string search)
        {
            System.Diagnostics.Debug.WriteLine($"Filter: {filter}, Search: {search}");
            List<ProductViewModel> products = new List<ProductViewModel>();
            var query = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                decimal searchPrice;
                bool isPrice = decimal.TryParse(search, out searchPrice);

                if (isPrice)
                {
                    query = query.Where(p => p.price == searchPrice);
                }
                else
                {
                    query = query.Where(p => p.product_name.Contains(search));
                }
            }

            switch (filter)
            {
                case "latest":
                    query = query.OrderByDescending(p => p.created_at);
                    break;

                case "bestselling":
                    // Hiển thị danh sách sản phẩm bán chạy nhất
                    var topSalesProducts = db.Order_Items.GroupBy(o => o.product_id).Select(g => new
                    {
                        product_id = g.Key,
                        order_quantity = g.Sum(oi => oi.quantity),
                        product = g.FirstOrDefault().Product
                    }).OrderByDescending(g => g.order_quantity).Select(g => g.product);

                    query = query.Where(p => topSalesProducts.Any(tsp => tsp.product_id == p.product_id));
                    break;
                //sort


                default:
                    break;
            }
            System.Diagnostics.Debug.WriteLine("Query:" + query);
            var productList = query.ToList(); // Chuyển đổi thành danh sách trước

            products = productList.Select(p => new ProductViewModel
            {
                Product = p,
                Images = db.Images.Where(i => i.product_id == p.product_id).ToList()
            }).ToList();
            //hiển thị lên view
            var viewModel = new HomeViewModel
            {
                Products = products,

            };
            return View(viewModel);

        }

        public ActionResult ChiTietSP(int? product_id)
        {
            if (product_id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest); ;
            var product = db.Products.FirstOrDefault(
                p => p.product_id == product_id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }
        
    }
}