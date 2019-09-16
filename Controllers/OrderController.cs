using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using Login_process_test.Utils;
using Login_process_test.Models;
using System.Collections.Generic;

namespace Login_process_test.Controllers
{
    public class OrderController : Controller
    {
        AppDbContext _db;
        public OrderController(AppDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Clear out
        public ActionResult ClearCart()
        {
            HttpContext.Session.Remove(SessionVariables.Cart);
            HttpContext.Session.SetString(SessionVariables.Message, "Cart Cleared");
            return Redirect("/Home");
        }

        // Get Order
        [Route("[action]")]
        public IActionResult GetOrders()
        {
            OrderModel model = new OrderModel(_db);
            ApplicationUser user = HttpContext.Session.Get<ApplicationUser>(SessionVariables.User);
            return Ok(model.GetAll(user.Id));
        }

        // Get Order Details
        [Route("[action]/{tid:int}")]
        public IActionResult GetOrderDetails(int tid)
        {
            OrderModel model = new OrderModel(_db);
            ApplicationUser user = HttpContext.Session.Get<ApplicationUser>(SessionVariables.User);
            return Ok(model.GetOrderDetails(tid, user.Id));
        }

        // Add Order Here
        public ActionResult AddOrder()
        {
            OrderModel model = new OrderModel(_db);
            List<int> retValList;
            string retMessage = "";

            try
            {
                Dictionary<string, object> cartItems = HttpContext.Session.Get<Dictionary<string, object>>(SessionVariables.Cart);

                retValList = model.AddOrder(cartItems, HttpContext.Session.Get<ApplicationUser>(SessionVariables.User));
                int orderId = retValList[0];
                int isBackOrdered = retValList[1];

                // Order Added
                if (orderId > 0 && isBackOrdered < 0) // Order Added
                {
                    retMessage = "Order " + orderId + " Created!";
                }
                //If backordered
                else if (orderId > 0 && isBackOrdered > 0)
                {
                    retMessage = "Order " + orderId + " Created! Some goods were backordered!";
                }
                else // Not added
                {
                    retMessage = "Order not added, try again later";
                }
            }
            catch (Exception ex)
            {
                retMessage = "Order was not created, try again later! - " + ex.Message;
            }
            HttpContext.Session.Remove(SessionVariables.Cart); // Clear out current cart once persisted
            HttpContext.Session.SetString(SessionVariables.Message, retMessage);

            return Redirect("/Home");
        }

        public IActionResult List()
        {
            if (HttpContext.Session.Get<ApplicationUser>(SessionVariables.User) == null)
            {
                return Redirect("/Login");
            }
            return View("List");
        }
    }
}
