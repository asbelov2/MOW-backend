using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data
{
  public class User : IEntity
  {
    public User(string nickname, string email, string password)
    {
      this.Id = Guid.NewGuid();
      this.Nickname = nickname;
      this.Email = email;
      this.Password = password;
    }

    public Guid Id { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Story> Stories { get; set; } = new List<Story>();
  }
}
