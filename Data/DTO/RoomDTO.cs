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
      this.Host = new UserDTO(room.Host);
      this.Name = room.Name;

      this.Users = new List<UserDTO>();
      foreach (var user in room.Users)
      {
        this.Users.Add(new UserDTO(user));
      }

      this.Witches = room.Witches;
      this.Humans = room.Humans;
      this.WithVoice = room.WithVoice;
      this.Story = new StoryDTO(room.Story);
      this.HumanTime = room.HumanTime;
      this.WitchPrepTime = room.WitchPrepTime;
      this.WitchAnswerTime = room.WitchAnswerTime;
      this.Judge = room.Judge;
    }

    /// <summary>
    /// Gets room ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets room name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets host.
    /// </summary>
    public UserDTO Host { get; }

    /// <summary>
    /// Gets collection of users.
    /// </summary>
    public ICollection<UserDTO> Users { get; }

    public int Witches { get; }

    public int Humans { get; }

    public bool WithVoice { get; }

    public StoryDTO Story { get; }

    public TimeSpan HumanTime { get; }

    public TimeSpan WitchPrepTime { get; }

    public TimeSpan WitchAnswerTime { get; }

    public bool Judge { get; }
  }
}