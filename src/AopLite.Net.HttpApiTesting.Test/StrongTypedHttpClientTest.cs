using AopLite.Net.HttpApiTesting.Web;
using AopLite.Net.HttpApiTesting.Web.Controllers;
using AopLite.Net.HttpProxyClient;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AopLite.Net.HttpApiTesting.Test
{
    public class StrongTypedHttpClientTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string apiEndPoint = "api/Values";
        private readonly StrongTypedHttpClient client;


        public StrongTypedHttpClientTest(WebApplicationFactory<Startup> factory)
        {
            client = new StrongTypedHttpClient(factory.CreateClient());
        }


        [Fact]
        public async Task GetAsyncTest()
        {
            var result = await client.GetAsync<IEnumerable<DTO>>($"{apiEndPoint}");
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAsyncWithKeyTest()
        {
            var id = Guid.NewGuid();
            var result = await client.GetAsync<DTO>($"{apiEndPoint}/{id}");
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task GetAsyncWithQueryStringTest()
        {
            var time = DateTime.Now;
            var number = 12321;
            var text = "some text";
            var result = await client.GetAsync<DTO>($"{apiEndPoint}/Query?time={time.ToUniversalTime().ToString("o")}&number={number}&text={text}");
            Assert.NotNull(result);
            Assert.Equal(time, result.TimeValue);
            Assert.Equal(number, result.NumberValue);
            Assert.Equal(text, result.StringValue);
        }

        [Fact]
        public async Task PostAsJsonAsyncTest()
        {
            var data = DTO.Create(DTO.Create());
            var result = await client.PostAsJsonAsync<DTO>($"{apiEndPoint}", data);
            Assert.NotNull(result);
            Assert.Equal(data.TimeValue, result.TimeValue);
            Assert.Equal(data.NumberValue, result.NumberValue);
            Assert.Equal(data.StringValue, result.StringValue);
        }
    }
}
