using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace API.Services
{
  public class RoomService
  {
    private IHubContext<GameHub> context;

    private readonly IServiceScopeFactory scopeFactory;
    public RoomService(
      IHubContext<GameHub> hubContext,
      IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[Room service] Init start");
      this.context = hubContext;
      this.rooms = new List<Room>();
      this.onlineUsers = new Dictionary<Guid, string>();
      this.scopeFactory = scopeFactory;
      Debug.WriteLine("[Room service] Init end");
    }

    public void UserOnline(string connectionId, User user)
    {
      Debug.WriteLine("[Room service] User online start");
      onlineUsers.Add(user.Id, connectionId);
      Debug.WriteLine("[Room service] User online end");
    }

    public void UserOffline(Guid userId)
    {
      Debug.WriteLine("[Room service] User offline start");
      onlineUsers.Remove(userId);
      Debug.WriteLine("[Room service] User offline end");
    }

    public void LeaveUser(Guid roomId, User user)
    {
      Debug.WriteLine("[Room service] Leave user start");
      if (this.rooms.Find(x => x.Id == roomId) != null)
      {
        this.context.Groups.RemoveFromGroupAsync(onlineUsers[user.Id], this.GetGroupKey(this.rooms.Find(x => x.Id == roomId).Id)).Wait();
        this.context.Clients.Group(this.GetGroupKey(roomId)).SendAsync("onUserDisconnected", user, this.rooms.Find(x => x.Id == roomId).Users).Wait();
        this.context.Clients.Client(onlineUsers[user.Id]).SendAsync("onDisconnected").Wait();
        this.rooms.Find(x => x.Id == roomId)?.Users?.Remove(user);
      }
      Debug.WriteLine("[Room service] Leave user end");
    }

    public bool EnterUser(Guid roomId, User newUser, string password)
    {
      Debug.WriteLine("[Room service] Enter user start");
      if ((this.rooms.Find(x => x.Id==roomId)?.Password == password) && ((!this.rooms.Find(x => x.Id == roomId)?.Users?.Contains(newUser)) ?? false))
      {
        this.rooms.Find(x => x.Id == roomId)?.Users?.Add(newUser);

        this.context.Clients.Group(this.GetGroupKey(roomId))?.SendAsync("onUserConnected", newUser, this.rooms.Find(x => x.Id == roomId).Users).Wait();
        this.context.Clients.Client(onlineUsers[newUser.Id])?.SendAsync("onConnected", this.rooms.Find(x => x.Id == roomId)).Wait();
        this.context.Groups.AddToGroupAsync(onlineUsers[newUser.Id], this.GetGroupKey(roomId)).Wait();
        Debug.WriteLine("[Room service] Enter user end");
        return true;
      }
      else
      {
        Debug.WriteLine("[Room service] Enter user end");
        return false;
      }
    }

    public Guid HostRoom(
      User host, 
      string name, 
      string password, 
      int witches,
      int humans,
      bool withVoice,
      Story story,
      TimeSpan humanTime,
      TimeSpan witchPrepTime,
      TimeSpan witchAnswerTime,
      bool judge)
    {
      Debug.WriteLine("[Room service] Room host start");
      var room = new Room(host, name, password, witches, humans, withVoice, story, humanTime, witchPrepTime, witchAnswerTime, judge);
      this.rooms.Add(room);
      this.context.Groups.AddToGroupAsync(onlineUsers[host.Id], this.GetGroupKey(room.Id)).Wait();
      Debug.WriteLine("[Room service] Room host end");
      return room.Id;
    }
    public void AddStory(Guid userId, IFormFile storyFile)
    {

      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        Story story;
        using (var fileStream = storyFile.OpenReadStream())
        using (var r = new StreamReader(fileStream))
        {
          string json = r.ReadToEnd();
           story = JsonConvert.DeserializeObject<Story>(json);
        }
        User user = dbContext.Users.Find(userId);
        dbContext.Stories.Add(story);
        story.UserId = user.Id;
        user.Stories.Add(story);
        dbContext.SaveChanges();
      }
    }

    /// <summary>
    /// Sets new room name.
    /// </summary>
    /// <param name="roomId">Room ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="name">New name.</param>
    public void ChangeRoomName(Guid roomId, Guid userId, string name)
    {
      Debug.WriteLine("[Room service] change room name start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).Name = name;
      }
      Debug.WriteLine("[Room service] change room name end");
    }

    /// <summary>
    /// Sets new room host.
    /// </summary>
    /// <param name="roomId">Room ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="newHostId">New host ID.</param>
    public void ChangeHost(Guid roomId, Guid userId, Guid newHostId)
    {
      Debug.WriteLine("[Room service] change host start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        using (var scope = this.scopeFactory.CreateScope())
        {
          var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
          this.rooms.Find(x => x.Id == roomId).Host = dbContext.Users.Find(newHostId);
        }
      }
      Debug.WriteLine("[Room service] change host end");
    }

    /// <summary>
    /// Sets new password.
    /// </summary>
    /// <param name="roomId">Room ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="newPassword">New password.</param>
    public void ChangePassword(Guid roomId, Guid userId, string newPassword)
    {
      Debug.WriteLine("[Room service] change password start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).Password = newPassword;
      }
      Debug.WriteLine("[Room service] change password end");
    }
    private bool IsHost(Guid userId, Guid roomId)
    {
      return userId == this.rooms.Find(x => x.Id == roomId)?.Host?.Id;
    }

    private string GetGroupKey(Guid roomId)
    {
      return $"room{roomId}";
    }

    public List<Room> rooms { get; }
    public Dictionary<Guid, string> onlineUsers { get; }
  }
}