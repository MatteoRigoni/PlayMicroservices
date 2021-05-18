using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private const string AdminRole = "Admin";

        private readonly IBaseRepository<InventoryItem> inventoryItemsRepository;
        private readonly IBaseRepository<CatalogItem> catalogItemsRepository;
        private readonly CatalogClient catalogClient;

        public ItemsController(
            IBaseRepository<InventoryItem> inventoryItemsRepository,
            IBaseRepository<CatalogItem> catalogItemsRepository,
            CatalogClient catalogClient)
        {
            this.inventoryItemsRepository = inventoryItemsRepository;
            this.catalogItemsRepository = catalogItemsRepository;
            this.catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest("User not specified");

            var currentUserId = User.FindFirstValue("sub");
            if (Guid.Parse(currentUserId) != userId)
                if (!User.IsInRole(AdminRole))
                    return Unauthorized();

            var inventoryItemEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);
            var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
            var catalogItemEntities = await catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemDtos);
        }

        // Syncronous version with API
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        // {
        //     if (userId == Guid.Empty) return BadRequest("User not specified");

        //     var catalogItems = await catalogClient.GetCatalogItemsAsync();
        //     var inventoryItemEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);

        //     var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
        //     {
        //         var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
        //         return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        //     });

        //     return Ok(inventoryItemDtos);
        // }

        [HttpPost]
        [Authorize(Roles = AdminRole)]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await inventoryItemsRepository.GetItemAsync(
                item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId
            );

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}