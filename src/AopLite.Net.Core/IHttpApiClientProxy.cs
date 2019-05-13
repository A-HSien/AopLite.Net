using System.Threading.Tasks;

namespace AopLite.Net.Core
{
    public interface IHttpApiClientProxy
    {
        Task<T> InvokeHttpGetGeneric<T>(string urlPath, object[] argumentKeys, object[] arguments);
        Task InvokeHttpPost(string urlPath, object[] argumentKeys, object[] arguments);
        Task<T> InvokeHttpPostGeneric<T>(string urlPath, object[] argumentKeys, object[] arguments);
    }
}
