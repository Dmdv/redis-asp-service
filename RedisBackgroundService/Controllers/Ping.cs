using Microsoft.AspNetCore.Mvc;

namespace RedisBackgroundService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Ping : ControllerBase
    {
        // GET: api/<Ping>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET api/<Ping>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return $"value = {id}";
        }

        // POST api/<Ping>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Ping>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Ping>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
