using UserGroupSite.Data.Interfaces;

namespace UserGroupSite.Data.Models;

public abstract class EntityBase : IEntityBase
{
    public int Id { get; set; }
}