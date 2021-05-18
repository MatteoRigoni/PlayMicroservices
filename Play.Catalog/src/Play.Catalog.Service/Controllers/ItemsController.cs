using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Play.Catalog.Service.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Play.Catalog.Service.Entities;
using Play.Catalog.Common;
using MassTransit;
using Play.Catalog.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private const  string AdminRole = "Admin";

        private readonly IBaseRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndPoint;

        public ItemsController(IBaseRepository<Item> itemsRepository, IPublishEndpoint publishEndPoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndPoint = publishEndPoint;
        }

        [HttpGet]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());

            return Ok(items);
        }

        [HttpGet("{id}")]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id) 
        {
            var item = await itemsRepository.GetItemAsync(id);
            if (item == null) return NotFound();
            return item.AsDto();
        }

        [HttpPost]
        [Authorize(Policies.Write)]
        public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto createItemDto)
        {
            var item = new Item {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemsRepository.CreateAsync(item);

            await publishEndPoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new {id = item.Id}, item);
        }

        [HttpPut("{id}")]
        [Authorize(Policies.Write)]
        public async Task<IActionResult> UpdateItemAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetItemAsync(id);
            if (existingItem is null) return NotFound();

            existingItem.Name = updateItemDto.Name ?? existingItem.Name;
            existingItem.Description = updateItemDto.Description ?? existingItem.Description;
            existingItem.Price = updateItemDto.Price > 0 ? updateItemDto.Price : existingItem.Price;

            await itemsRepository.UpdateAsync(existingItem);

            await publishEndPoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policies.Write)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existingItem = await itemsRepository.GetItemAsync(id);
            if (existingItem is null) return NotFound();

            await itemsRepository.RemoveAsync(existingItem.Id);

            await publishEndPoint.Publish(new CatalogItemDeleted(existingItem.Id));

            return NoContent();
        }
    }    
}