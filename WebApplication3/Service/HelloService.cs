using NetCoreWebApp.Domain;

namespace NetCoreWebApp.Service
{
    public class HelloService : IHelloService
    {
        public UserInfo Get(int id)
        {
            return new UserInfo()
            {
                Id = 1,
                UserName = "lind"
            };
        }
    }
}
