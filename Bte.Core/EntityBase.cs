namespace Bte.Core;


/// Base class for all entities (outside the ASP.NET Core Identity system).
/// Provides an Id field.
/// 
public abstract class EntityBase
{
    public int Id { get; set; }
}
