using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Data;

namespace API
{

  /*
      TODO: Сделать чат для комнат отдельным
   */

  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class GameHub : Hub
  {
    private RoomService roomService;
    private readonly IServiceScopeFactory scopeFactory;
    public GameHub(RoomService roomService, IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[GameHub] Init start");
      this.roomService = roomService;
      this.scopeFactory = scopeFactory;
      Debug.WriteLine("[GameHub] Init end");
    }

    public override async Task OnConnectedAsync()
    {
      Debug.WriteLine("[GameHub] onConnected start");
      await base.OnConnectedAsync();
      Debug.WriteLine("[GameHub] onConnected end");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      Debug.WriteLine("[GameHub] onDisconnected start");
      this.roomService.UserOffline(roomService.onlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key);
      await base.OnDisconnectedAsync(exception);
      Debug.WriteLine("[GameHub] onDisconnected end");
    }
    
    public async Task Send(string message, Guid roomId, User user, string color, int channel)
    {
      Debug.WriteLine("[GameHub] send start");
      switch (channel)
      {
        case 0:
          {
            await Clients.Group(GetGroupKey(roomId)).SendAsync("Receive", message, user, color);
            break;
          }
        case 1:
          {
            await Clients.Group(GetGroupKey(roomId)).SendAsync("ReceiveWitch", message, user, color);
            break;
          }
        case 2:
          {
            await Clients.Group(GetGroupKey(roomId)).SendAsync("ReceiveHuman", message, user, color);
            break;
          }
        default:
          break;
      }
      Debug.WriteLine("[GameHub] send end");
    }

    public void ChangeRoomName(Guid roomId, Guid userId, string newName)
    {
      Debug.WriteLine("[GameHub] ChangeRoomName start");
      this.roomService.ChangeRoomName(roomId,userId,newName);
      Debug.WriteLine("[GameHub] ChangeRoomName end");
    }

    public void ChangeRoomPassword(Guid roomId, Guid userId, string newPassword)
    {
      Debug.WriteLine("[GameHub] ChangeRoomPassword start");
      this.roomService.ChangePassword(roomId, userId, newPassword);
      Debug.WriteLine("[GameHub] ChangeRoomPassword end");
    }

    public void ChangeHumansAmount(Guid roomId, Guid userId, int newAmount)
    {
      Debug.WriteLine("[GameHub] ChangeHumansAmount start");
      this.roomService.ChangeHumansAmount(roomId, userId, newAmount);
      Debug.WriteLine("[GameHub] ChangeHumansAmount end");
    }

    public void ChangeWitchesAmount(Guid roomId, Guid userId, int newAmount)
    {
      Debug.WriteLine("[GameHub] ChangeWitchesAmount start");
      this.roomService.ChangeWitchesAmount(roomId, userId, newAmount);
      Debug.WriteLine("[GameHub] ChangeWitchesAmount end");
    }

    public void ChangeVoiceAbility(Guid roomId, Guid userId, bool voice)
    {
      Debug.WriteLine("[GameHub] ChangeVoiceAbility start");
      this.roomService.ChangeVoiceAbility(roomId, userId, voice);
      Debug.WriteLine("[GameHub] ChangeVoiceAbility send");
    }

    public void ChangeJudgeAbility(Guid roomId, Guid userId, bool judge)
    {
      Debug.WriteLine("[GameHub] ChangeJudgeAbility start");
      this.roomService.ChangeJudgeAbility(roomId, userId, judge);
      Debug.WriteLine("[GameHub] ChangeJudgeAbility end");
    }

    public void ChangeHumanTime(Guid roomId, Guid userId, int seconds)
    {
      Debug.WriteLine("[GameHub] ChangeHumanTime start");
      this.roomService.ChangeHumanTime(roomId, userId, TimeSpan.FromSeconds(seconds));
      Debug.WriteLine("[GameHub] ChangeHumanTime end");
    }

    public void ChangeWitchPrepTime(Guid roomId, Guid userId, int seconds)
    {
      Debug.WriteLine("[GameHub] ChangeWitchPrepTime start");
      this.roomService.ChangeWitchPrepTime(roomId, userId, TimeSpan.FromSeconds(seconds));
      Debug.WriteLine("[GameHub] ChangeWitchPrepTime end");
    }

    public void ChangeWitchAnswerTime(Guid roomId, Guid userId, int seconds)
    {
      Debug.WriteLine("[GameHub] ChangeWitchAnswerTime start");
      this.roomService.ChangeWitchAnswerTime(roomId, userId, TimeSpan.FromSeconds(seconds));
      Debug.WriteLine("[GameHub] ChangeWitchAnswerTime end");
    }

    public void ChangeHost(Guid roomId, Guid userId, Guid newHost)
    {
      Debug.WriteLine("[GameHub] ChangeRoomPassword start");
      this.roomService.ChangeHost(roomId,userId,newHost);
      Debug.WriteLine("[GameHub] ChangeRoomPassword end");
    }

    public void JoinWitchTeam(Guid userId)
    {
      Debug.WriteLine("[GameHub] JoinWitchTeam start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        roomService.JoinWitchTeam(dbContext.Users.Find(userId));
      }
      Debug.WriteLine("[GameHub] JoinWitchTeam end");
    }

    public void JoinHumanTeam(Guid userId)
    {
      Debug.WriteLine("[GameHub] JoinHumanTeam start");
      using (var scope = this.scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        roomService.JoinWitchTeam(dbContext.Users.Find(userId));
      }
      Debug.WriteLine("[GameHub] JoinHumanTeam end");
    }

    private string GetGroupKey(Guid roomId)
    {
      return $"room{roomId}";
    }
  }
}
