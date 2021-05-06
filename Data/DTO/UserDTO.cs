using System;

namespace Data.DTO
{
  public class UserDTO
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDTO"/> class.
    /// </summary>
    /// <param name="nickname">User nickname.</param>
    /// <param name="id">User ID.</param>
    /// <param name="email">User email</param>
    public UserDTO(Guid id, string nickname, string email)
    {
      this.Id = id;
      this.Nickname = nickname;
      this.Email = email;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDTO"/> class.
    /// </summary>
    /// <param name="user">User.</param>
    public UserDTO(User user)
    {
      this.Id = user.Id;
      this.Nickname = user.Nickname;
      this.Email = user.Email;
    }

    /// <summary>
    /// Gets user email.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets user ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets user nickname.
    /// </summary>
    public string Nickname { get; }
  }
}