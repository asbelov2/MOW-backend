using System;

namespace Data
{
  /// <summary>
  /// Interface of entity. Entity must have ID.
  /// </summary>
  public interface IEntity
  {
    /// <summary>
    /// Gets id of entity.
    /// </summary>
    public Guid Id { get; }
  }
}