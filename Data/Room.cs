using System;
using System.Collections.Generic;

namespace Data
{
  public class Room : IEntity
  {
    public Room(
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
      this.Id = Guid.NewGuid();
      this.Password = password;
      this.Name = name;
      this.Host = host;
      this.Users = new List<User>();
      this.Witches = witches;
      this.Humans = humans;
      this.WithVoice = withVoice;
      this.Story = story;
      this.HumanTime = humanTime;
      this.WitchPrepTime = witchPrepTime;
      this.WitchAnswerTime = witchAnswerTime;
      this.Judge = judge;
    }

    /// <summary>
    /// Host.
    /// </summary>
    public User Host { get; set; }

    public int Humans { get; set; }
    public int Witches { get; set; }

    public TimeSpan HumanTime { get; set; }
    public TimeSpan WitchAnswerTime { get; set; }
    public TimeSpan WitchPrepTime { get; set; }

    /// <summary>
    /// Room ID.
    /// </summary>
    public Guid Id { get; }

    public bool Judge { get; set; }

    /// <summary>
    /// Room name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Password.
    /// </summary>
    public string Password { get; set; }

    public Story Story { get; set; }

    public String FakeStory { get; set; } = "";

    /// <summary>
    /// Collection of users in room.
    /// </summary>
    public ICollection<User> Users { get; }


    public bool WithVoice { get; set; }
    public bool IsStarted { get; set; }
  }
}