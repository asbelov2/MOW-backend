using System;
using System.Collections.Generic;

namespace Data.DTO
{
  public class RoomDTO
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomDTO"/> class.
    /// </summary>
    /// <param name="id">Room ID.</param>
    /// <param name="host">Host.</param>
    /// <param name="name">Room name.</param>
    /// <param name="users">Collection of users.</param>
    public RoomDTO(Room room)
    {
      this.Id = room.Id;
      this.Name = room.Name;
      this.Host = new UserDTO(room.Host);
      this.Users = new List<UserDTO>();
      foreach (var user in room.Users)
      {
        this.Users.Add(new UserDTO(user));
      }
    }

    /// <summary>
    /// Gets host.
    /// </summary>
    public UserDTO Host { get; }

    /// <summary>
    /// Gets collection of users.
    /// </summary>
    public ICollection<UserDTO> Users { get; }

    /// <summary>
    /// Gets room ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets room name.
    /// </summary>
    public string Name { get; }

  }
}