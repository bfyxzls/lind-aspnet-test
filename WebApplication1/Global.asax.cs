using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ConsoleApp1;
using WEBAPI.Core;

namespace WebApplication1
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            Task.Run(() =>
            {
                var rabbitMqSubscriber = new RabbitMQSubscriber();
                rabbitMqSubscriber.SubscribeFanout<UserInfo>("test_fanout",
                    msg => { Console.WriteLine("fanout:" + msg.name); });
            });

            Task.Run(() =>
            {
                var rabbitMqSubscriber = new RabbitMQSubscriber(exchangeName:"topicEx");
                rabbitMqSubscriber.Subscribe<UserInfo>( "test_queue", msg =>
                {
                    Console.WriteLine("queue:" + msg.name);
                    return true;
                });
            });
        }
    }
}