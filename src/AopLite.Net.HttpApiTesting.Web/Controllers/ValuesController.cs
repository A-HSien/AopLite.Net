using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AopLite.Net.HttpApiTesting.Web.Controllers
{
    public class DTO
    {
        public Guid Id { get; set; }
        public DateTime TimeValue { get; set; }
        public string StringValue { get; set; }
        public float NumberValue { get; set; }

        public DTO SubObject { get; set; }

        private static Random ren = new Random();
        public static DTO Create(DTO subObject = null) => new DTO
        {
            Id = Guid.NewGuid(),
            TimeValue = DateTime.Today,
            StringValue = "some text - " + ren.Next(),
            NumberValue = ren.Next(),
            SubObject = subObject
        };
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private static IEnumerable<DTO> data = new[] {
            DTO.Create( DTO.Create() ),
            DTO.Create( DTO.Create() ),
            DTO.Create( DTO.Create() )
        };

        // GET api/values
        [HttpGet]
        public IEnumerable<DTO> Get()
        {
            return data;
        }

        // GET api/values/{id}
        [HttpGet("{id}")]
        public DTO Get(Guid id)
        {
            var data = DTO.Create(DTO.Create());
            data.Id = id;
            return data;
        }

        // GET api/values/Query?time=time&number=number&text=text
        [HttpGet("Query")]
        public DTO Query(DateTime time, float number, string text)
        {
            var data = DTO.Create(DTO.Create());
            data.TimeValue = time;
            data.NumberValue = number;
            data.StringValue = text;
            return data;
        }

        // POST api/values
        [HttpPost]
        public DTO Post([FromBody] DTO value)
        {
            return value;
        }
    }
}
