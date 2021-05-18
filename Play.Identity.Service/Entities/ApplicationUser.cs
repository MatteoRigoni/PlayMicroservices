using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;

namespace Play.Identity.Service.Entities
{
    [CollectionName("Users")]
    public class ApplicationUser: MongoIdentityUser<Guid>
    {
        public decimal Coins { get; set; }
    }
}