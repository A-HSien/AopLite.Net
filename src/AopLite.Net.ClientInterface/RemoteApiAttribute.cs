using System;

namespace AopLite.Net.ClientInterface
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteApiAttribute : Attribute
    {
        private const string scheme_default = "http";
        private const string host_default = "localhost";
        private const int port_default = 80;

        public virtual string Scheme { get; set; } = scheme_default;
        public virtual string Host { get; set; } = host_default;
        public virtual int Port { get; set; } = port_default;

        public virtual string Path { get; set; } = string.Empty;
        public virtual string Query { get; set; } = string.Empty;
        public virtual HttpMethod Method { get; set; }

        public RemoteApiAttribute(HttpMethod method = HttpMethod.Get)
        {
            Method = method;
        }

        public virtual string GetUrlPath()
        {
            var urlPath = string.Empty;
            if (Scheme != scheme_default || Host != host_default || Port != port_default)
                urlPath = $"{Scheme}://{Host}:{Port}/";

            if (Path != string.Empty) urlPath += Path;
            if (Query != string.Empty) urlPath += $"?{Query}";
            return urlPath;
        }
    }
}
