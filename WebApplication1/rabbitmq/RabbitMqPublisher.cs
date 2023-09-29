using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;


namespace WEBAPI.Core
{
    /// <summary>
    /// RabbitMq消息生产者
    /// </summary>
    public class RabbitMQPublisher
    {
        private string _uri;
        /// <summary>
        /// 交换器
        /// </summary>
        private string exchangeName = "";
        private readonly IConnection connection;
        //private readonly IModel channel;

        /// <summary>
        /// 初始化
        /// 子类去实现相关的rabbit地址,端口和授权
        /// </summary>
        /// <param name="uri">消息服务器地址</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="exchangeName">交换器,为空表示分发模式,否则为广播模式</param>
        public RabbitMQPublisher(string uri = "amqp://localhost:5672", string userName = "guest", string password = "guest", string exchangeName = "")
        {
            _uri = uri;
            var factory = new ConnectionFactory() { Uri = new Uri(_uri) };
            if (!string.IsNullOrWhiteSpace(exchangeName))
                this.exchangeName = exchangeName;
            if (!string.IsNullOrWhiteSpace(userName))
                factory.UserName = userName;
            if (!string.IsNullOrWhiteSpace(userName))
                factory.Password = password;
            connection = factory.CreateConnection();
            //this.channel = connection.CreateModel();
        }

        /// <summary>
        /// 将消息推送到服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public void Publish<TMessage>(string queue, TMessage message)
        {
            Publish(null, queue, message);
        }

        /// <summary>
        /// 将消息推送到服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="queue"></param>
        /// <param name="message"></param>
        /// <param name="durable">是否持久化</param>
        /// <param name="arguments">消息优先级0-10（越大越高）</param>
        public void Publish<TMessage>(string exchange, string queue, TMessage message)
        {
            if (!string.IsNullOrWhiteSpace(exchange))
                exchangeName = exchange;
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue,//队列名
                                           durable: true,//是否持久化
                                           exclusive: false,//排它性
                                           autoDelete: false,//一旦客户端连接断开则自动删除queue
                                           arguments: null);//如果安装了队列优先级插件则可以设置优先级

                var json = JsonHelper.SerializeObject(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                Console.WriteLine("向服务器{0}推消息", _uri);
                channel.BasicPublish(exchange: this.exchangeName, routingKey: queue, basicProperties: null, body: bytes);
            }
        }

        public void PublishFanout<TMessage>(TMessage message)
        {
            PublishFanout(null, message);
        }

        /// <summary>
        /// 广播消息,需要在初始化时为exchangeName赋值
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        public void PublishFanout<TMessage>(string exchange, TMessage message)
        {
            if (!string.IsNullOrWhiteSpace(exchange))
                exchangeName = exchange;
            const string ROUTING_KEY = "";
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(this.exchangeName, "fanout");//广播
                var json = JsonHelper.SerializeObject(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(this.exchangeName, ROUTING_KEY, null, bytes);//不需要指定routing key，设置了fanout,指了也没有用.
                Console.WriteLine(DateTime.Now + " 向服务器{0}推消息", _uri);
            }
        }
        /// <summary>
        /// 批量将消息推送到服务器
        /// </summary>
        public void PublishBatch<TMessage>(string queue, List<TMessage> messages)
        {
            PublishBatch(null, queue, messages);
        }
        /// <summary>
        /// 批量将消息推送到服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="messages"></param>
        /// <param name="durable">是否持久化</param>
        /// <param name="arguments">消息优先级0-10（越大越高）</param>
        public void PublishBatch<TMessage>(string exchange, string queue, List<TMessage> messages)
        {
            if (!string.IsNullOrWhiteSpace(exchange))
                exchangeName = exchange;

            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                queue: queue, //队列名
                durable: true, //是否持久化
                exclusive: false, //排它性
                autoDelete: false,//一旦客户端连接断开则自动删除queue
                arguments: null);//如果安装了队列优先级插件则可以设置优先级

                var batch = channel.CreateBasicPublishBatch();

                foreach (var message in messages)
                {
                    try
                    {
                        var json = JsonHelper.SerializeObject(message);
                        var body = Encoding.UTF8.GetBytes(json);
                        batch.Add(exchange: this.exchangeName, routingKey: queue, mandatory: false, properties: null, body: body);
                        Console.WriteLine("Added message to batch: {0}", message);
                    }
                    catch
                    {
                    }
                }

                batch.Publish();

                Console.WriteLine("Published batch of messages to queue '{0}'", queue);
            }
        }

        public int MessageCount(string queue, bool durable = false)
        {
            using (var channel = connection.CreateModel())
            {
                //声明队列
                channel.QueueDeclare(
                queue: queue,
                durable: durable,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

                // 获取消息数量
                uint messageCount = channel.MessageCount(queue);
                return (int)messageCount;
            }
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }
    }
}
