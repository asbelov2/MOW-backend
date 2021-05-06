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

namespace API
{

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
    
    public async Task Send(string message, string userName)
    {
      Debug.WriteLine("[GameHub] send start");
      await Clients.All.SendAsync("Receive", message, userName);
      Debug.WriteLine("[GameHub] send end");
    }
  }
}
