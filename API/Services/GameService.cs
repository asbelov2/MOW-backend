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
using System.Timers;

namespace API.Services
{
  public class GameService
  {
    private IHubContext<GameHub> context;
    private readonly IServiceScopeFactory scopeFactory;


    public GameService(
      IHubContext<GameHub> hubContext,
      IServiceScopeFactory scopeFactory)
    {
      Debug.WriteLine("[Room service] Init start");
      this.context = hubContext;
      this.scopeFactory = scopeFactory;
      this.userRoles = new Dictionary<Guid, GameRoles>();
      this.humanTimers = new Dictionary<Guid, PausableTimer>();
      this.witchPrepTimers = new Dictionary<Guid, PausableTimer>();
      this.witchAnswerTimers = new Dictionary<Guid, PausableTimer>();
      Debug.WriteLine("[Room service] Init end");
    }

    public void GiveRole(Guid userId, GameRoles role)
    {
      userRoles[userId] = role;
      this.context.Clients.All.SendAsync("onUserGotRole", userId, (int)role).Wait();
    }

    public GameRoles GetRole(Guid userId)
    {
      return userRoles[userId];
    }

    public void EndPreparation(Guid roomId)
    {
      witchPrepTimers[roomId].Stop();
      humanTimers[roomId].Start();
    }

    public void AnswerRequest(Guid roomId)
    {
      humanTimers[roomId].Pause();
      witchAnswerTimers[roomId].Start();
    }

    public void AnswerGiven(Guid roomId)
    {
      witchAnswerTimers[roomId].Stop();
      humanTimers[roomId].Resume();
    }

    private string GetGroupKey(Guid roomId)
    {
      return $"room{roomId}";
    }

    public enum GameRoles
    {
      Spectator,
      Witch,
      Human,
      Judge
    }

    public Dictionary<Guid, GameRoles> userRoles { get; }
    public Dictionary<Guid, PausableTimer> humanTimers { get; }
    public Dictionary<Guid, PausableTimer> witchPrepTimers { get; }
    public Dictionary<Guid, PausableTimer> witchAnswerTimers { get; }
  }
}