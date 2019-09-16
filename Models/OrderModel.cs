using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Login_process_test.Models;

namespace Login_process_test.Models
{
    public class OrderModel
    {
        private AppDbContext _db;
        public OrderModel(AppDbContext ctx)
        {
            _db = ctx;
        }
        //public int AddOrder(Dictionary<string, object> products, ApplicationUser user)
        public List<int> AddOrder(Dictionary<string, object> products, ApplicationUser user)
        {
            int orderId = -1;
            int isBackOrdered = -1;
            using (_db)
            {
                // we need a transaction as multiple entities involved
                using (var _trans = _db.Database.BeginTransaction())
                {
                    try
                    {
                        Order order = new Order();
                        order.UserId = user.Id;
                        //order.UserId = user.UserName;
                        order.OrderDate = System.DateTime.Now;
                        order.OrderAmount = 0;

                        // calculate the totals and then add the tray row to the table
                        foreach (var key in products.Keys)
                        {
                            ProductViewModel pro =
                            JsonConvert.DeserializeObject<ProductViewModel>(Convert.ToString(products[key]));
                            if (pro.Qty > 0)
                            {
                                order.OrderAmount += (pro.MSRP * pro.Qty);
                            }
                        }
                        _db.Orders.Add(order);
                        _db.SaveChanges();

                        // Add each item to the orderitems table
                        foreach (var key in products.Keys)
                        {
                            ProductViewModel pro =
                            JsonConvert.DeserializeObject<ProductViewModel>(Convert.ToString(products[key]));
                            if (pro.Qty > 0)
                            {
                                OrderLineItem orderItem = new OrderLineItem();
                                var productItem = _db.Products.First(a => a.Id == pro.Id);

                                orderItem.OrderId = order.Id;
                                orderItem.ProductId = pro.Id;
                                orderItem.SellingPrice = pro.MSRP;

                                if (pro.Qty <= pro.QTYONHAND)
                                {
                                    productItem.QtyOnHand = pro.QTYONHAND - pro.Qty;

                                    orderItem.QtyOrdered = pro.Qty;
                                    orderItem.QtySold = pro.Qty;
                                    orderItem.QtyBackOrdered = 0;
                                }
                                else if (pro.Qty > pro.QTYONHAND)
                                {
                                    productItem.QtyOnHand = 0;
                                    productItem.QtyOnBackOrder = pro.QTYBACKORDER + (pro.Qty - pro.QTYONHAND);

                                    orderItem.QtyOrdered = pro.Qty;
                                    orderItem.QtySold = pro.QTYONHAND;
                                    orderItem.QtyBackOrdered = pro.Qty - pro.QTYONHAND;

                                    isBackOrdered = 1;
                                }
                                else
                                {
                                }
                                _db.OrderLineItems.Add(orderItem);
                                _db.SaveChanges();
                                _db.Products.Update(productItem);
                                _db.SaveChanges();
                            }
                        }
                        // test trans by uncommenting out these 3 lines
                        //int x = 1;
                        //int y = 0;
                        //x = x / y;
                        _trans.Commit();
                        orderId = order.Id;
                    }
                    catch (Exception ex)
                    {
                        orderId = -1;
                        Console.WriteLine(ex.Message);
                        _trans.Rollback();
                    }
                }
            }

            List<int> orderStatusList = new List<int>();
            orderStatusList.Add(orderId);
            orderStatusList.Add(isBackOrdered);

            //return orderId;
            return orderStatusList;
        }

        public List<Order> GetAll(string user)
        {
            return _db.Orders.Where(order => order.UserId == user).ToList<Order>();
        }

        public List<OrderViewModel> GetOrderDetails(int tid, string uid)
        {
            List<OrderViewModel> allDetails = new List<OrderViewModel>();
            // LINQ way of doing INNER JOINS
            var results = from o in _db.Set<Order>()
                          join oi in _db.Set<OrderLineItem>() on o.Id equals oi.OrderId
                          join p in _db.Set<Product>() on oi.ProductId equals p.Id
                          where (o.UserId == uid && o.Id == tid)
                          select new OrderViewModel
                          {
                              OrderId = o.Id,
                              OrderAmount = oi.QtySold * oi.SellingPrice,
                              UserId = uid,
                              ProductName = oi.Product.ProductName,
                              MSRP = oi.SellingPrice,
                              QtyOrdered = oi.QtyOrdered,
                              QtySold = oi.QtySold,
                              QtyBackOrdered = oi.QtyBackOrdered,
                              OrderDate = o.OrderDate.ToString("yyyy/MM/dd - hh:mm tt")
                          };
            allDetails = results.ToList<OrderViewModel>();
            return allDetails;
        }
    }
}
