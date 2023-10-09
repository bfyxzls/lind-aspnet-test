using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WEBAPI.Core
{
    /// <summary>
    ///     RabbitMQ消息消费者
    /// </summary>
    public class RabbitMQSubscriber
    {
        private readonly IConnection connection;
        private string exchangeName;

        private string queueName;
        //private readonly IModel channel;

        /// <summary>
        ///     初始化消费者
        /// </summary>
        /// <param name="uri">消息服务器地址</param>
        /// <param name="queueName">队列名</param>
        /// <param name="userName">用户</param>
        /// <param name="password">密码</param>
        /// <param name="exchangeName">交换机,有值表示广播模式</param>
        public RabbitMQSubscriber(string uri = "amqp://localhost:5672", string userName = "guest",
            string password = "guest", string exchangeName = "", string queue = "")
        {
            var factory = new ConnectionFactory { Uri = new Uri(uri) };
            if (!string.IsNullOrWhiteSpace(exchangeName))
                this.exchangeName = exchangeName;
            if (!string.IsNullOrWhiteSpace(userName))
                factory.UserName = userName;
            if (!string.IsNullOrWhiteSpace(password))
                factory.Password = password;
            if (!string.IsNullOrWhiteSpace(queue))
                queueName = queue;
            connection = factory.CreateConnection();
            //this.channel = connection.CreateModel();
        }

        /// <summary>
        ///     触发消费行为
        /// </summary>
        public void Subscribe<TMessage>(string queue, Func<TMessage, bool> callback = null)
        {
            Subscribe(null, queue, callback);
        }

        /// <summary>
        ///     触发消费行为
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="callback">回调方法，以及是否确认消息已被消费</param>
        public void Subscribe<TMessage>(string exchange, string queue, Func<TMessage, bool> callback = null)
        {
            if (!string.IsNullOrWhiteSpace(exchange))
                exchangeName = exchange;

            // 使用自定义的队队
            if (!string.IsNullOrWhiteSpace(queue))
                queueName = queue;

            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, "topic"); //广播

                channel.QueueDeclare(
                    queueName,
                    true, //持久化
                    false, //独占,只能被一个consumer使用
                    false, //自己删除,在最后一个consumer完成后删除它
                    null);
                channel.QueueBind(queueName, exchangeName, queueName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var json = Encoding.UTF8.GetString(e.Body.ToArray());
                    var result = callback(JsonHelper.DeserializeDefaultObject<TMessage>(json));
                    if (result)
                        channel.BasicAck(e.DeliveryTag, false); //手动确认消息已被消费
                };
                // 启动消费者
                channel.BasicConsume(
                    queueName,
                    false, // 禁用自动消息确认，需要手动确认消息
                    consumer); // 消费者对象
                queueName = null;
                Console.WriteLine(" [*] Waiting for messages." + "To exit press CTRL+C");
                Console.ReadKey();
            }
        }

        /// <summary>
        ///     批量消费消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="queue">队列名</param>
        /// <param name="batchSize">批量大小</param>
        /// <param name="callback">处理批量消息的回调函数</param>
        /// <param name="durable">持久化，需要与生产者一致</param>
        public void ConsumeBatch<TMessage>(string queue, int batchSize, Action<List<TMessage>> callback)
        {
            queueName = queue;

            using (var channel = connection.CreateModel())
            {
                // 声明队列
                channel.QueueDeclare(
                    queueName,
                    true,
                    false,
                    false,
                    null
                );

                var messages = new List<TMessage>();

                // 批量获取消息
                for (var i = 0; i < batchSize; i++)
                {
                    var result = channel.BasicGet(queueName, true);

                    if (result == null)
                        continue;

                    var json = Encoding.UTF8.GetString(result.Body.ToArray());
                    var message = JsonHelper.DeserializeDefaultObject<TMessage>(json);
                    if (message == null)
                        Console.WriteLine(json);
                    else
                        messages.Add(message);
                }

                if (messages.Count > 0)
                    // 处理批量消息
                    callback(messages);
            }
        }

        /// <summary>
        ///     广播模式
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="exchange"></param>
        /// <param name="callback"></param>
        public void SubscribeFanout<TMessage>(string exchange, Action<TMessage> callback = null)
        {
            if (!string.IsNullOrWhiteSpace(exchange))
                exchangeName = exchange;

            using (var channel = connection.CreateModel())
            {
                //广播模式
                channel.ExchangeDeclare(exchangeName, "fanout"); //广播
                var queueOk =
                    channel.QueueDeclare(); //每当Consumer连接时，我们需要一个新的，空的queue,如果在声明queue时不指定,那么RabbitMQ会随机为我们选择这个名字
                queueName = queueOk.QueueName; //得到RabbitMQ帮我们取了名字
                channel.QueueBind(queueName, exchangeName, string.Empty); //不需要指定routing key，设置了fanout,指了也没有用.

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var json = Encoding.UTF8.GetString(e.Body.ToArray());
                    callback(JsonHelper.DeserializeDefaultObject<TMessage>(json));

                    channel.BasicAck(e.DeliveryTag, false); //手动确认消息已被消费
                };
                // 启动消费者
                channel.BasicConsume(
                    queueName,
                    false,
                    consumer);
                queueName = null;
                Console.WriteLine(" [*] Waiting for messages." + "To exit press CTRL+C");
                Console.ReadKey();
            }
        }

        public string PeekMessage(string queue)
        {
            using (var channel = connection.CreateModel())
            {
                // 声明队列
                channel.QueueDeclare(
                    queue,
                    true,
                    false,
                    false,
                    null
                );

                // 获取消息
                var result = channel.BasicGet(queue, false);
                if (result == null)
                    return null;
                var json = Encoding.UTF8.GetString(result.Body.ToArray());
                return json;
            }
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }
    }
}