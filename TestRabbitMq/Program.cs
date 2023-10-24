using System;
using System.Security.Cryptography;
using System.Text;
using TestRabbitMq;
using WEBAPI.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // var rabbitMqSubscriber = new RabbitMQSubscriber();
            // rabbitMqSubscriber.SubscribeFanout<UserInfo>("test_fanout", msg => { Console.WriteLine(msg.name); });
            // var rabbitMqPublisher = new RabbitMQPublisher();
            // rabbitMqPublisher.PublishFanout("test_fanout", "hello");

            string secretKey = "ABCDEFHGIJKLMNOPQRST234UVWXYZ567"; // 这是你的共享密钥
            int timeStep = 30; // TOTP时间步长，以秒为单位
            int totpLength = 8; // TOTP生成的密码长度

            string totp3 = Totp3.GenerateTOTP(secretKey, timeStep, totpLength);
            Console.WriteLine("totp3=" + totp3);
            Console.ReadKey();
        }

        static string GenerateTOTP(string secretKey, ulong timeCounter, int totpLength)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] counterBytes = BitConverter.GetBytes(timeCounter);
            Array.Reverse(counterBytes); // 反转字节数组以匹配HMAC-SHA1规范

            HMACSHA1 hmac = new HMACSHA1(keyBytes);
            byte[] hash = hmac.ComputeHash(counterBytes);
            int offset = hash[hash.Length - 1] & 0x0F; // 获取动态截取的索引
            int binary = ((hash[offset] & 0x7F) << 24) |
                         ((hash[offset + 1] & 0xFF) << 16) |
                         ((hash[offset + 2] & 0xFF) << 8) |
                         (hash[offset + 3] & 0xFF);

            int otp = binary % (int)Math.Pow(10, totpLength);
            return otp.ToString("D" + totpLength);
        }
    }
}
