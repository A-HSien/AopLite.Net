using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AopLite.Net.HttpProxyClient
{
    public abstract class HttpApiClientProxy
    {

        public readonly StrongTypedHttpClient Client;

        public Task<T> InvokeHttpGetGeneric<T>(string urlPath, object[] argumentKeys, object[] arguments)
        {
            var tt = MethodBase.GetCurrentMethod().DeclaringType;
            var ttt = GetType();

            for (var i = 0; i < argumentKeys.Length; i++)
            {
                var arg = arguments[i];
                var queryString = (arg is DateTime) ? ((DateTime)arg).ToUniversalTime().ToString("o") : arguments[i].ToString();
                urlPath = urlPath.Replace("{" + argumentKeys[i] + "}", queryString);
            }
            return Client.GetAsync<T>(urlPath);
        }

        public Task InvokeHttpPost(string urlPath, object[] argumentKeys, object[] arguments)
        {
            if (arguments.Length < 2)
                return Client.PostAsJsonAsync(urlPath, arguments[0] ?? default(object));
            else throw new NotImplementedException("HttpPost not support multiple arguments");
        }

        public Task<T> InvokeHttpPostGeneric<T>(string urlPath, object[] argumentKeys, object[] arguments)
        {
            if (arguments.Length < 2)
                return Client.PostAsJsonAsync<T>(urlPath, arguments[0] ?? default(object));
            else throw new NotImplementedException("HttpPost not support multiple arguments");
        }
    }
}
