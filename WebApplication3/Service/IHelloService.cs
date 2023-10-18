using NetCoreWebApp.Domain;

namespace NetCoreWebApp.Service
{
    public interface IHelloService
    {
        UserInfo Get(int id);
    }
}
