using Azure.Core;
using BigElephant;
using BigElephant.Data;
using BigElephant.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Threading.Channels;
using System.Xml.Linq;

namespace BigElephant.Controllers;
public class ListUsersProducts
{
    [Required]
    [MinLength(1)]
    public List<UsersProducts> Items { get; set; }
}

public class UsersProducts
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext dbContext)
    {
        _db = dbContext;
    }

    [Authorize]
    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        return Ok(_db.Products.Where(item => item.IsActive == true && item.IsDeleted == false));
    }

    [Authorize]
    [HttpGet("myOrders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.Status,
                items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    productName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    [Authorize]
    [HttpGet("myOrders/{id}")]
    public async Task<IActionResult> GetMyOrdersById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var order = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.Status,
                items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    productName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound("There isn't element with that's id");

        return Ok(order);
    }

    [Authorize]
    [HttpPost("create/order")]
    public async Task<IActionResult> PostOrder([FromBody] ListUsersProducts userData)
    {
        if (userData?.Items == null || userData.Items.Count == 0)
        {
            return BadRequest("Items is required");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var userOrder = new Order
        {
            UserId = userId,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        for (int i = 0; i < userData.Items.Count; i++)
        {
            var item = await _db.Products.FindAsync(userData.Items[i].ProductId);
            if (item == null)
            {
                return NotFound($"There isn't product with id = {userData.Items[i].ProductId}");
            }
            if (!item.IsActive)
            {
                return Conflict("Product is inactive");
            }
            if (userData.Items[i].Quantity <= 0)
            {
                return BadRequest("Rewrite field Quantity. It must be positive");
            }
            if (userData.Items[i].Quantity > item.Stock)
            {
                return Conflict("There aren't so many items");
            }
            item.Stock -= userData.Items[i].Quantity;
            var product = new OrderItem
            {
                ProductId = item.Id,
                Quantity = userData.Items[i].Quantity,
                UnitPrice = item.Price
            };
            userOrder.Items.Add(product);
        }
        _db.Orders.Add(userOrder);
        await _db.SaveChangesAsync();
        return Created("You created order", new { orderId = userOrder.Id });
    }

    [Authorize]
    [HttpPatch("myOrder/{id}/status")]
    public async Task<IActionResult> ChangeOrderStatus(int id)
    {
        var order = _db.Orders.FirstOrDefault(or => or.Id == id && or.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (order == null)
        {
            return NotFound("There isn't element with that's id");
        }
        if (order.Status == "Completed")
        {
            return Conflict("Order is completed. You can not cancel it");
        }
        if (order.Status == "Shipped")
        {
            return Conflict("Order is shipped. You can not cancel it");
        }
        if (order.Status == "Cancelled")
        {
            return Ok("Order is already cancelled");
        }
        order.Status = "Cancelled";
        var items = _db.OrderItems.Where(orI => orI.OrderId == id).ToList();
        foreach (var item in items)
        {
            var product = _db.Products.FirstOrDefault(pr => pr.Id == item.ProductId);
            if (product == null)
            {
                return NotFound($"There isn't product with id {item.ProductId}");
            }
            product.Stock += item.Quantity;
        }
        await _db.SaveChangesAsync();
        return Ok("You cancelled that order");
    }
}
