using System.Threading.Tasks;
using MassTransit;
using Play.Catalog.Common;
using Play.Catalog.Contracts;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IBaseRepository<CatalogItem> repository;

        public CatalogItemDeletedConsumer(IBaseRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;
            var item = await repository.GetItemAsync(message.ItemId);
            if (item == null) return;

            await repository.RemoveAsync(item.Id);
        }
    }
}