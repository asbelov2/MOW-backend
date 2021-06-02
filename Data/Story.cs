using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data
{
  public class Story : IEntity
  {
    public Story(string origin, string name, string[] characters, string[] facts, TimeSpan recomendedWitchPrepTime, TimeSpan recomendedWitchAnswerTime, TimeSpan recomendedHumanTime, string winCondition, Sizes size, Difficulties difficulty)
    {
      this.Id = Guid.NewGuid();
      this.Origin = origin;
      this.Name = name;
      this.Characters = characters;
      this.Facts = facts;
      this.RecomendedWitchPrepTime = recomendedWitchPrepTime;
      this.RecomendedWitchAnswerTime = recomendedWitchAnswerTime;
      this.RecomendedHumanTime = recomendedHumanTime;
      this.WinCondition = winCondition;
      this.Size = size;
      this.Difficulty = difficulty;
    }

    public Story(string origin, string name, string internalCharacters, string internalFacts, TimeSpan recomendedWitchPrepTime, TimeSpan recomendedWitchAnswerTime, TimeSpan recomendedHumanTime, string winCondition, int internalSize, int internalDifficulty)
    {
      this.Id = Guid.NewGuid();
      this.Origin = origin;
      this.Name = name;
      this.InternalCharacters = internalCharacters;
      this.InternalFacts = internalFacts;
      this.RecomendedWitchPrepTime = recomendedWitchPrepTime;
      this.RecomendedWitchAnswerTime = recomendedWitchAnswerTime;
      this.RecomendedHumanTime = recomendedHumanTime;
      this.WinCondition = winCondition;
      this.InternalSize = internalSize;
      this.InternalDifficulty = internalDifficulty;
    }

    [JsonConstructor]
    public Story()
    {
      this.Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Origin { get; set; }

    [NotMapped]
    public string[] Characters
    {
      get
      {
        return this.InternalCharacters.Split(',');
      }
      set
      {
        this.InternalCharacters = string.Join(",", value);
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string InternalCharacters { get; set; } = "";

    [NotMapped]
    public string[] Facts
    {
      get
      {
        return this.InternalFacts.Split(',');
      }
      set
      {
        this.InternalFacts = string.Join(",", value);
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string InternalFacts { get; set; } = "";

    public TimeSpan RecomendedWitchPrepTime { get; set; }

    public TimeSpan RecomendedWitchAnswerTime { get; set; }

    public TimeSpan RecomendedHumanTime { get; set; }

    public string WinCondition { get; set; }

    public enum Sizes
    {
      Small,
      Medium,
      Big,
      Hude
    }

    [NotMapped]
    public Sizes Size
    {
      get
      {
        return (Sizes)InternalSize;
      }
      set
      {
        this.InternalSize = (int)value;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int InternalSize { get; set; } = 0;

    public enum Difficulties
    {
      Easy,
      Medium,
      Hard
    }

    [NotMapped]
    public Difficulties Difficulty
    {
      get
      {
        return (Difficulties)InternalDifficulty;
      }
      set
      {
        this.InternalDifficulty = (int)value;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int InternalDifficulty { get; set; } = 0;

    public Guid UserId { get; set; }
  }
}
