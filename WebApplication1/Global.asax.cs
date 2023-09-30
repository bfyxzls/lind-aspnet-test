using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ConsoleApp1;
using WEBAPI.Core;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Task.Run(() =>
            {
                RabbitMQSubscriber rabbitMqSubscriber = new RabbitMQSubscriber();
                rabbitMqSubscriber.SubscribeFanout<UserInfo>("test_fanout", msg =>
                {
                    Console.WriteLine(msg.name);
                });
            });
            
        }
    }
}