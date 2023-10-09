using System;
using WEBAPI.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var rabbitMqSubscriber = new RabbitMQSubscriber();
            rabbitMqSubscriber.SubscribeFanout<UserInfo>("test_fanout", msg => { Console.WriteLine(msg.name); });
            var rabbitMqPublisher = new RabbitMQPublisher();
            rabbitMqPublisher.PublishFanout("test_fanout", "hello");
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}