using Microsoft.AspNetCore.Mvc;
using NetCoreWebApp.Domain;
using NetCoreWebApp.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NetCoreWebApp.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IHelloService _helloService;
        public UserController(IHelloService helloService)
        {
            this._helloService = helloService;
        }
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<UserInfo> Get()
        {
            return new List<UserInfo> { this._helloService.Get(1) };
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
