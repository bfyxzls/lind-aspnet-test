using System;
using WEBAPI.Core;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            RabbitMQSubscriber rabbitMqSubscriber = new RabbitMQSubscriber();
            rabbitMqSubscriber.SubscribeFanout<UserInfo>("test_fanout", msg =>
            {
                Console.WriteLine(msg.name);
            });
            RabbitMQPublisher rabbitMqPublisher = new RabbitMQPublisher();
            rabbitMqPublisher.PublishFanout("test_fanout","hello");
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}