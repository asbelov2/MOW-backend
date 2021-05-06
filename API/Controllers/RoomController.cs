using API.Services;
using Data;
using Data.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RoomController : ControllerBase
  {
    private RoomService roomService;
    private IWebHostEnvironment appEnvironment;
    private readonly IServiceScopeFactory scopeFactory;

    public RoomController(RoomService roomService, IWebHostEnvironment appEnvironment, IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[Room controller] Init start");
      this.roomService = roomService;
      this.appEnvironment = appEnvironment;
      this.scopeFactory = scopeFactory;
      Debug.WriteLine("[Room controller] Init end");
    }

    /// <summary>
    /// Get all rooms.
    /// </summary>
    /// <returns>All rooms.</returns>
    [Authorize]
    [HttpGet]
    public IEnumerable<RoomDTO> Get()
    {
      Debug.WriteLine("[Room controller] Get all rooms start");
      var result = new List<RoomDTO>();
      foreach (var room in this.roomService.rooms)
      {
        if (room != null)
        {
          result.Add(new RoomDTO(room));
        }
      }
      Debug.WriteLine("[Room controller] Get all rooms end");
      return result;
    }

    /// <summary>
    /// Get one room by id.
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("{id}")]
    public RoomDTO Get(Guid roomId)
    {
      Debug.WriteLine("[Room controller] Get room start");
      if (this.roomService.rooms.FirstOrDefault(x => x.Id == roomId) != null)
      {
        return new RoomDTO(this.roomService.rooms.FirstOrDefault(x => x.Id == roomId));
      }
      Debug.WriteLine("[Room controller] Get room end");
      return null;
    }

    /// <summary>
    /// Get one room by id.
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("GetStories")]
    public List<Story> GetStories()
    {
      Debug.WriteLine("[Room controller] Get stories start");
      Debug.WriteLine("[Room controller] Get stories end");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        return dbContext.Stories.ToList();
      }
    }

    /// <summary>
    /// Get one room by host id.
    /// </summary>
    /// <param name="id">Host ID.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("GetByHostId/{id}")]
    public RoomDTO GetByHostId(Guid id)
    {
      Debug.WriteLine("[Room controller] Get room by host start");
      if (this.roomService.rooms.FirstOrDefault(x => x.Host.Id == id) != null)
      {
        return new RoomDTO(this.roomService.rooms.FirstOrDefault(x => x.Host.Id == id));
      }
      Debug.WriteLine("[Room controller] Get room by host end");
      return null;
    }

    [Authorize]
    [HttpPost("GetUsersOnline")]
    public int GetUsersOnline()
    {
      Debug.WriteLine("[Room controller] Get users online start");
      Debug.WriteLine("[Room controller] Get user online end");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        return this.roomService.onlineUsers.Count;
      }
    }

    [Authorize]
    [HttpPost("UserOnline")]
    public void UserOnline([FromForm]string connectionId, [FromForm] Guid userId)
    {
      Debug.WriteLine("[Room controller] User online start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        this.roomService.UserOnline(connectionId, dbContext.Users.Find(userId));
      }
      Debug.WriteLine("[Room controller] User online end");
    }

    [Authorize]
    [HttpPost("{id}/UserOffline")]
    public void UserOffline(Guid userId)
    {
      Debug.WriteLine("[Room controller] User offline start");
      this.roomService.UserOffline(userId);
      Debug.WriteLine("[Room controller] User offline end");
    }

    /// <summary>
    /// Connecting user to room.
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="password">Room password.</param>
    [Authorize]
    [HttpPost("Connect")]
    public void Connect([FromForm] Guid roomId, [FromForm] Guid userId, [FromForm] string password)
    {
      Debug.WriteLine("[Room controller] User connect start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        this.roomService.EnterUser(roomId, dbContext.Users.FirstOrDefault(x => x.Id == userId), password);
      }
      Debug.WriteLine("[Room controller] User connect end");
    }

    /// <summary>
    /// Disconnecting user from the room.
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <param name="userId">User ID.</param>
    [Authorize]
    [HttpPost("Disconnect")]
    public void Disconnect(Guid roomId, Guid userId)
    {
      Debug.WriteLine("[Room controller] User disconnect start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        this.roomService.LeaveUser(roomId, dbContext.Users.FirstOrDefault(x => x.Id == userId));
      }
      Debug.WriteLine("[Room controller] User disconnect end");
    }

    /// <summary>
    /// Uploading story to repo
    /// </summary>
    /// <param name="userId">Id of uploader</param>
    /// <param name="story">Story object</param>
    [Authorize]
    [HttpPost("UploadStory")]
    public void UploadStory([FromForm] Guid userId,[FromForm] IFormFile storyFile)
    {
      Debug.WriteLine("[Room controller] Upload story start");
      this.roomService.AddStory(userId,storyFile);
      Debug.WriteLine("[Room controller] Upload story end");
    }

    /// <summary>
    /// Creating room.
    /// </summary>
    /// <param name="hostId">Host ID.</param>
    /// <param name="name">Room name.</param>
    /// <param name="password">Room password.</param>
    /// <param name="cardInterpretation">Room card's interpretation.</param>
    [Authorize]
    [HttpPost]
    public void Create(
      [FromForm] Guid hostId,
      [FromForm] string name,
      [FromForm] string password,
      [FromForm] int witches,
      [FromForm] int humans,
      [FromForm] bool withVoice,
      [FromForm] Guid storyId,
      [FromForm] TimeSpan humanTime,
      [FromForm] TimeSpan witchPrepTime,
      [FromForm] TimeSpan witchAnswerTime,
      [FromForm] bool judge)
    {
      Debug.WriteLine("[Room controller] Host room start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        this.roomService.HostRoom(dbContext.Users.FirstOrDefault(x => x.Id == hostId), name, password, witches, humans, withVoice, dbContext.Stories.Find(storyId), humanTime, witchPrepTime, witchAnswerTime, judge);
      }
      Debug.WriteLine("[Room controller] Host room end");
    }

    /// <summary>
    /// Sets new host
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="hostId">New host ID.</param>
    [Authorize]
    [HttpPut("{id}/SetHost")]
    public void SetHost(Guid id, Guid userId, string hostId)
    {
      Debug.WriteLine("[Room controller] New host start");
      this.roomService.ChangeHost(id, userId, Guid.Parse(hostId));
      Debug.WriteLine("[Room controller] New host end");
    }

    /// <summary>
    /// Sets new password
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="password">Room password.</param>
    [Authorize]
    [HttpPut("{id}/SetPassword")]
    public void SetPassword(Guid id, Guid userId, string password)
    {
      Debug.WriteLine("[Room controller] New password start");
      this.roomService.ChangePassword(id, userId, password);
      Debug.WriteLine("[Room controller] New password end");
    }

    /// <summary>
    /// Delete room.
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <param name="userId">User ID.</param>
    [Authorize]
    [HttpDelete("{id}")]
    public void Delete(Guid roomId, Guid userId)
    {
      Debug.WriteLine("[Room controller] Delete room start");
      if (userId == this.roomService.rooms.FirstOrDefault(x => x.Id == roomId).Host.Id)
      {
        this.roomService.rooms.Remove(this.roomService.rooms.FirstOrDefault(x => x.Id == roomId));
      }
      Debug.WriteLine("[Room controller] Delete room end");
    }
  }
}