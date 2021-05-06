using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly IServiceScopeFactory scopeFactory;
    public UserController(IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[User controller] Init start");
      this.scopeFactory = scopeFactory;
      Debug.WriteLine("[User controller] Init end");
    }

    [HttpPost("Register")]
    public void Register([FromForm] string nickname, [FromForm] string email, [FromForm] string password)
    {
      Debug.WriteLine("[User controller] Register start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        dbContext.Users.Add(new User(nickname, email, password));
        dbContext.SaveChanges();
      }
      Debug.WriteLine("[User controller] Register end");
    }

    // GET: api/<UserController>
    [HttpGet]
    public List<User> Get()
    {
      Debug.WriteLine("[User controller] Get users start");
      Debug.WriteLine("[User controller] Get users end");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        return dbContext.Users.ToList();
      }
    }

    [HttpGet("GetUserStories")]
    public List<Story> GetUserStories(Guid userId)
    {
      Debug.WriteLine("[User controller] Get user's stories start");
      Debug.WriteLine("[User controller] Get user's stories end");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        return dbContext.Stories.Where(x=>x.UserId==userId).ToList();
      }
    }

    // GET api/<UserController>/5
    [HttpGet("{id}")]
    public User Get(Guid id)
    {
      Debug.WriteLine("[User controller] Get user start");
      Debug.WriteLine("[User controller] Get user end");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        return dbContext.Users.FirstOrDefault(x => x.Id == id);
      }
    }

    // DELETE api/<UserController>/5
    [HttpDelete("{id}")]
    public void Delete(Guid id)
    {
      Debug.WriteLine("[User controller] Delete user start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        dbContext.Users.Remove(dbContext.Users.FirstOrDefault(x => x.Id == id));
      }
      Debug.WriteLine("[User controller] Delete user end");
    }
  }
}