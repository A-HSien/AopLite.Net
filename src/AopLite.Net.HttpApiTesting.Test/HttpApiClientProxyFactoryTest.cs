using AopLite.Net.ClientInterface;
using AopLite.Net.HttpApiTesting.Web;
using AopLite.Net.HttpApiTesting.Web.Controllers;
using AopLite.Net.HttpProxyClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AopLite.Net.HttpApiTesting.Test
{
    public interface IValuesController
    {
        [RemoteApi(Path = "api/Values")]
        Task<IEnumerable<DTO>> Get();

        [RemoteApi(Path = "api/Values/{id}")]
        Task<DTO> Get(Guid id);

        [RemoteApi(Path = "api/Values/Query", Query = "time={time}&number={number}&text={text}")]
        Task<DTO> Query(DateTime time, float number, string text);

        [RemoteApi(HttpMethod.Post, Path = "api/Values")]
        Task<DTO> Post(DTO data);
    }


    public class HttpApiClientProxyFactoryTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly IValuesController api;

        public HttpApiClientProxyFactoryTest(WebApplicationFactory<Startup> factory)
        {
            var proxyFactory = new HttpApiClientProxyFactory<IValuesController>();
            var services = new ServiceCollection();
            services.AddSingleton(factory.CreateClient());
            services.AddSingleton<StrongTypedHttpClient>();
            services.AddSingleton(typeof(IValuesController), proxyFactory.GetProxyType());
            var serviceProvider = services.BuildServiceProvider();
            api = serviceProvider.GetService<IValuesController>();
        }

        [Fact]
        public async Task GetTest()
        {
            var result = await api.Get();
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdTest()
        {
            var id = Guid.NewGuid();
            var result = await api.Get(id);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task QueryTest()
        {
            var time = DateTime.Now;
            var number = 12321;
            var text = "some text";
            var result = await api.Query(time, number, text);
            Assert.Equal(time, result.TimeValue);
            Assert.Equal(time, result.TimeValue);
            Assert.Equal(number, result.NumberValue);
            Assert.Equal(text, result.StringValue);
        }

        [Fact]
        public async Task CreateCustomerTest()
        {
            var data = DTO.Create(DTO.Create());
            var result = await api.Post(data);
            Assert.Equal(data.TimeValue, result.TimeValue);
            Assert.Equal(data.NumberValue, result.NumberValue);
            Assert.Equal(data.StringValue, result.StringValue);
        }
    }
}
