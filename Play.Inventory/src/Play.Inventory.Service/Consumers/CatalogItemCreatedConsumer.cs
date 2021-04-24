using System.Threading.Tasks;
using MassTransit;
using Play.Catalog.Common;
using Play.Catalog.Contracts;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IBaseRepository<CatalogItem> repository;

        public CatalogItemCreatedConsumer(IBaseRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;
            var item = await repository.GetItemAsync(message.ItemId);
            if (item != null) return;

            item = new CatalogItem{
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await repository.CreateAsync(item);
        }
    }
}