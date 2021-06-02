using Data.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static API.Services.GameService;

namespace API.Services
{
  [Route("api/[controller]")]
  [ApiController]
  public class GameController : Controller
  {
    private RoomService roomService;
    private GameService gameService;
    private IWebHostEnvironment appEnvironment;
    private readonly IServiceScopeFactory scopeFactory;

    public GameController(RoomService roomService, GameService gameService, IWebHostEnvironment appEnvironment, IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[Game controller] Init start");
      this.roomService = roomService;
      this.gameService = gameService;
      this.appEnvironment = appEnvironment;
      this.scopeFactory = scopeFactory;
      Debug.WriteLine("[Game controller] Init end");
    }

    [Authorize]
    [HttpPost("ChangeRole")]
    public void ChangeRole(Guid userId, GameRoles newRole)
    {
      Debug.WriteLine("[Game controller] Changing role start");
      this.gameService.GiveRole(userId, newRole);
      Debug.WriteLine("[Game controller] Changing role end");
    }

    [Authorize]
    [HttpGet("GetRole/{id}")]
    public int GetRole(Guid id)
    {
      Debug.WriteLine("[Game controller] GetRole start");
      Debug.WriteLine("[Game controller] GetRole end");
      return (int)this.gameService.GetRole(id);
    }

    [Authorize]
    [HttpGet("GetFakeStory")]
    public string GetFakeStory(Guid roomId)
    {
      Debug.WriteLine("[Game controller] GetFakeStory start");
      Debug.WriteLine("[Game controller] GetFakeStory end");
      return this.roomService.rooms.FirstOrDefault(x => x.Id == roomId).FakeStory;
    }

    [Authorize]
    [HttpGet("PutFakeStory")]
    public void PutFakeStory(Guid roomId, string fakeStory)
    {
      Debug.WriteLine("[Game controller] PutFakeStory start");
      Debug.WriteLine("[Game controller] PutFakeStory end");
      this.roomService.rooms.FirstOrDefault(x => x.Id == roomId).FakeStory = fakeStory;
    }
  }
}
