using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO
{
  public class StoryDTO
  {
    public StoryDTO(Story story)
    {
      this.Id = story.Id;
      this.Origin = story.Origin;
      this.Name = story.Name;
      this.Characters = story.Characters;
      this.Facts = story.Facts;
      this.RecomendedWitchPrepTime = story.RecomendedWitchPrepTime;
      this.RecomendedWitchAnswerTime = story.RecomendedWitchAnswerTime;
      this.RecomendedHumanTime = story.RecomendedHumanTime;
      this.WinCondition = story.WinCondition;
      this.Size = (int)story.Size;
      this.Difficulty = (int)story.Difficulty;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Origin { get; }

    public string[] Characters { get; }

    public string[] Facts { get; }

    public TimeSpan RecomendedWitchPrepTime { get; set; }

    public TimeSpan RecomendedWitchAnswerTime { get; set; }

    public TimeSpan RecomendedHumanTime { get; set; }

    public string WinCondition { get; set; }

    public int Size { get; }

    public int Difficulty { get; }

    public Guid UserId { get; set; }
  }
}
