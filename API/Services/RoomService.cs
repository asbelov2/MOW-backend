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
using Data.DTO;
using System.Timers;

namespace API.Services
{
  /*
   TODO:
    Сделать валидацию регистрации
    Сделать валидацию двойного входа
    Сделать валидацию максимального кол-ва пользователей в команде
    Сделать валидацию наличия судьи в игре
    Ограничить старт игры при недостаточном кол-ве человек в командах
    Реализовать инструментарий судьи
   */
  public class RoomService
  {
    private IHubContext<GameHub> context;
    private GameService gameService;
    private readonly IServiceScopeFactory scopeFactory;
    public RoomService(
      IHubContext<GameHub> hubContext,
      GameService gameService,
      IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[Room service] Init start");
      this.context = hubContext;
      this.rooms = new List<Room>();
      this.witchTeam = new List<User>();
      this.humanTeam = new List<User>();
      this.onlineUsers = new Dictionary<Guid, string>();
      this.gameService = gameService;
      this.scopeFactory = scopeFactory;
      Debug.WriteLine("[Room service] Init end");
    }

    public void JoinWitchTeam(User user)
    {
      if (!witchTeam.Contains(user))
      {
        if (humanTeam.Contains(user))
        {
          humanTeam.Remove(user);
        }
        witchTeam.Add(user);
        gameService.GiveRole(user.Id, GameService.GameRoles.Witch);
        this.context.Clients.All.SendAsync("onTeamsUpdate", witchTeam, humanTeam).Wait();
      }
    }
    public void JoinHumanTeam(User user)
    {
      if (!humanTeam.Contains(user))
      {
        if (witchTeam.Contains(user))
        {
          witchTeam.Remove(user);
        }
        humanTeam.Add(user);
        gameService.GiveRole(user.Id, GameService.GameRoles.Human);
        this.context.Clients.All.SendAsync("onTeamsUpdate", witchTeam, humanTeam).Wait();
      }
    }

    public void UserOnline(string connectionId, User user)
    {
      Debug.WriteLine("[Room service] User online start");
      if (onlineUsers.ContainsKey(user.Id))
      {
        Debug.WriteLine("[Room service] User already online");
      }
      else
      {
        onlineUsers.Add(user.Id, connectionId);
        gameService.userRoles.Add(user.Id, GameService.GameRoles.Spectator);
      }
      Debug.WriteLine("[Room service] User online end");
    }

    public void UserOffline(Guid userId)
    {
      Debug.WriteLine("[Room service] User offline start");
      onlineUsers.Remove(userId);
      gameService.userRoles.Remove(userId);
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

    public void StartGame(Guid roomId)
    {
      this.context.Clients.Group(this.GetGroupKey(roomId)).SendAsync("onGameStarted").Wait();
      Room tmpRoom = this.rooms.Find(x => x.Id == roomId);
      PausableTimer tmpTimer = new PausableTimer(tmpRoom.HumanTime.TotalMilliseconds);
      tmpTimer.AutoReset = false;
      tmpTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.context.Clients.Group(this.GetGroupKey(roomId)).SendAsync("onHumanTimeEnd").Wait(); };
      this.gameService.humanTimers.Add(roomId, tmpTimer);
      tmpTimer = new PausableTimer(tmpRoom.WitchPrepTime.TotalMilliseconds);
      tmpTimer.AutoReset = false;
      tmpTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.context.Clients.Group(this.GetGroupKey(roomId)).SendAsync("onWitchPrepTimeEnd").Wait(); };
      this.gameService.witchPrepTimers.Add(roomId, tmpTimer);
      tmpTimer = new PausableTimer(tmpRoom.WitchAnswerTime.TotalMilliseconds);
      tmpTimer.AutoReset = false;
      tmpTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.context.Clients.Group(this.GetGroupKey(roomId)).SendAsync("onWitchAnswerTimeEnd").Wait(); };
      this.gameService.witchAnswerTimers.Add(roomId, tmpTimer);
      this.gameService.witchPrepTimers[roomId].Start();
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
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
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
          context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
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

    public void ChangeHumansAmount(Guid roomId, Guid userId, int newAmount)
    {
      Debug.WriteLine("[Room service] ChangeHumansAmount start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).Humans = newAmount;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeHumansAmount end");
    }

    public void ChangeWitchesAmount(Guid roomId, Guid userId, int newAmount)
    {
      Debug.WriteLine("[Room service] ChangeWitchesAmount start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).Witches = newAmount;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeWitchesAmount end");
    }

    public void ChangeVoiceAbility(Guid roomId, Guid userId, bool voice)
    {
      Debug.WriteLine("[Room service] ChangeVoiceAbility start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).WithVoice = voice;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeVoiceAbility end");
    }

    public void ChangeJudgeAbility(Guid roomId, Guid userId, bool judge)
    {
      Debug.WriteLine("[Room service] ChangeJudgeAbility start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).Judge = judge;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeJudgeAbility end");
    }

    public void ChangeHumanTime(Guid roomId, Guid userId, TimeSpan newTime)
    {
      Debug.WriteLine("[Room service] ChangeHumanTime start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).HumanTime = newTime;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeHumanTime end");
    }

    public void ChangeWitchPrepTime(Guid roomId, Guid userId, TimeSpan newTime)
    {
      Debug.WriteLine("[Room service] ChangeWitchPrepTime start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).WitchPrepTime = newTime;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeWitchPrepTime end");
    }

    public void ChangeWitchAnswerTime(Guid roomId, Guid userId, TimeSpan newTime)
    {
      Debug.WriteLine("[Room service] ChangeWitchAnswerTime start");
      if (this.IsHost(userId, roomId) && (this.rooms.Find(x => x.Id == roomId) != null))
      {
        this.rooms.Find(x => x.Id == roomId).WitchAnswerTime = newTime;
        context.Clients.Group(GetGroupKey(roomId)).SendAsync("onRoomChange", new RoomDTO(this.rooms.Find(x => x.Id == roomId)));
      }
      Debug.WriteLine("[Room service] ChangeWitchAnswerTime end");
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
    public List<User> witchTeam { get; }
    public List<User> humanTeam { get; }
  }
}