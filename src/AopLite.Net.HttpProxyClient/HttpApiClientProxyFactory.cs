using AopLite.Net.ClientInterface;
using AopLite.Net.Core;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using HttpMethod = AopLite.Net.ClientInterface.HttpMethod;

namespace AopLite.Net.HttpProxyClient
{
    public class HttpApiClientProxyFactory : ProxyFactory
    {
        private Type proxyType;

        public HttpApiClientProxyFactory(Type proxyType)
        {
            this.proxyType = proxyType;
        }


        private void createHttpMethod(TypeBuilder typeBuilder, string methodName, HttpMethod httpMethod, string urlPath, ParameterInfo[] argsInfo, Type returnType)
        {

            var argsTypes = argsInfo.Select(a => a.ParameterType).ToArray();
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                methodName
                , MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual
                , returnType
                , argsTypes
                );


            createILGeneratorContext(
                methodBuilder.GetILGenerator(),
                generator =>
                {
                    if (returnType != typeof(void))
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldstr, urlPath);

                        generator.Emit(OpCodes.Ldc_I4, argsTypes.Length);
                        generator.Emit(OpCodes.Newarr, typeof(string));
                        for (var i = 0; i < argsTypes.Length; i++)
                        {
                            generator.Emit(OpCodes.Dup);
                            generator.Emit(OpCodes.Ldc_I4, i);
                            generator.Emit(OpCodes.Ldstr, argsInfo[i].Name);
                            generator.Emit(OpCodes.Stelem_Ref);
                        }

                        generator.Emit(OpCodes.Ldc_I4, argsTypes.Length);
                        generator.Emit(OpCodes.Newarr, typeof(object));
                        for (var i = 0; i < argsTypes.Length; i++)
                        {
                            generator.Emit(OpCodes.Dup);
                            generator.Emit(OpCodes.Ldc_I4, i);
                            generator.Emit(OpCodes.Ldarg, i + 1);
                            generator.Emit(OpCodes.Box, argsTypes[i]);
                            generator.Emit(OpCodes.Stelem_Ref);
                        }


                        // call HttpClientProxy.InvokeGetMethod
                        var returnValueExist = returnType.GenericTypeArguments.Length > 0;
                        var binding = BindingFlags.Public | BindingFlags.Instance;

                        MethodInfo method;
                        if (returnValueExist)
                        {
                            method =
                            (httpMethod == HttpMethod.Get) ?
                            proxyType.GetMethod(nameof(IHttpApiClientProxy.InvokeHttpGetGeneric), binding)
                            : proxyType.GetMethod(nameof(IHttpApiClientProxy.InvokeHttpPostGeneric), binding);

                            method = method.MakeGenericMethod(returnType.GenericTypeArguments[0]);
                        }
                        else
                            method = proxyType.GetMethod(nameof(IHttpApiClientProxy.InvokeHttpPost), binding);

                        generator.Emit(OpCodes.Call, method);
                    }

                    generator.Emit(OpCodes.Ret);
                });

        }

        protected override void createMethod(MethodInfo method, TypeBuilder typeBuilder)
        {
            var parameters = method.GetParameters();
            var isRemoteHttpApiCall = this.isRemoteHttpApiCall(method, out var urlPath, out var httpMethod);
            if (isRemoteHttpApiCall)
                createHttpMethod(typeBuilder, method.Name, httpMethod, urlPath, parameters, method.ReturnType);
            else
                createDefaultMethod(typeBuilder, method.Name, parameters.Select(a => a.ParameterType).ToArray(), method.ReturnType);
        }

        private bool isRemoteHttpApiCall(MethodInfo method, out string urlPath, out HttpMethod httpMethod)
        {
            urlPath = null;
            httpMethod = HttpMethod.None;
            var attribute = method.GetCustomAttributes<RemoteApiAttribute>().FirstOrDefault(a => a is RemoteApiAttribute);
            if (attribute == null) return false;


            urlPath = attribute.GetUrlPath();
            httpMethod = attribute.Method;
            return true;
        }
    }
}
