using System;

namespace Play.Catalog.Common
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}