using BigElephant;
using BigElephant.Data;
using BigElephant.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BigElephant.Controllers;


[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    private readonly UserManager<AppUser> _userManager;

    public AdminController(AppDbContext dbContext, UserManager<AppUser> userManager)
    {
        _db = dbContext;
        _userManager = userManager;
    }

    public class CreateProductRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
    }

    public class UpdateProductStockRequest
    {
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    public class UpdateProductIsActiveRequest
    {
        public bool IsActive { get; set; }
    }
    public class UpdateProductNameRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }
    }
    public class UpdateProductPriceRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }
    public class UpdateProductImageRequest
    {
        public string? ImageUrl { get; set; }
    }
    public class UpdateOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = null!;
    }

    private void LogAdminAction(string action)
    {
        var adminId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        _db.AdminLogs.Add(new AdminLog
        {
            AdminId = adminId,
            CreatedAt = DateTime.UtcNow,
            Action = action
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("product")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.Products.ToListAsync();
        return Ok(products);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("product/{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound("There isn't element with that's id");
        }

        return Ok(product);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("product/post")]
    public async Task<IActionResult> PostNewProduct([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl
        };

        _db.Products.Add(product);
        LogAdminAction($"Created product: {product.Name}, Price: {product.Price}, Stock: {product.Stock}");
        await _db.SaveChangesAsync();

        return Created("You added product", product);
    }


    [Authorize(Roles = "Admin")]
    [HttpPatch("product/{id}/stock")]
    public async Task<IActionResult> PatchProductStock(int id, [FromBody] UpdateProductStockRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("There isn't product with that's id");
        }
        product.Stock = request.Stock;
        LogAdminAction($"Changed stock of product {product.Id} to {product.Stock}");
        await _db.SaveChangesAsync();
        return Ok("You changed product's stock");
    }


    [Authorize(Roles = "Admin")]
    [HttpPatch("product/{id}/active")]
    public async Task<IActionResult> PatchProductIsActive(int id, [FromBody] UpdateProductIsActiveRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("There isn't product with that's id");
        }
        product.IsActive = request.IsActive;
        LogAdminAction($"Changed active state of product {product.Id} to {product.IsActive}");
        await _db.SaveChangesAsync();
        return Ok("You changed product's state of active");
    }


    [Authorize(Roles = "Admin")]
    [HttpPatch("product/{id}/name")]
    public async Task<IActionResult> PatchProductName(int id, [FromBody] UpdateProductNameRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("There isn't product with that's id");
        }
        product.Name = request.Name.Trim();
        LogAdminAction($"Changed name of product {product.Id} to {product.Name}");
        await _db.SaveChangesAsync();
        return Ok("You changed product's name");
    }


    [Authorize(Roles = "Admin")]
    [HttpPatch("product/{id}/price")]
    public async Task<IActionResult> PatchProductPrice(int id, [FromBody] UpdateProductPriceRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("There isn't product with that's id");
        }
        product.Price = request.Price;
        LogAdminAction($"Changed price of product {product.Id} to {product.Price}");
        await _db.SaveChangesAsync();
        return Ok("You changed product's price");
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("product/{id}/image")]
    public async Task<IActionResult> PatchProductImage(int id, UpdateProductImageRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return NotFound("There isn't product with that's id");

        product.ImageUrl = request.ImageUrl;
        LogAdminAction($"Changed image of product {product.Id}");
        await _db.SaveChangesAsync();

        return Ok("Image updated");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("product/{id}/delete")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("There isn't product with that's id");
        }
        product.IsDeleted = true;
        product.IsActive = false;
        LogAdminAction($"Soft deleted product {product.Id}");
        await _db.SaveChangesAsync();
        return Ok("You deleted product");
    }


    [Authorize(Roles = "Admin")]
    [HttpPatch("product/{id}/return")]
    public async Task<IActionResult> ReturnProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("There isn't product with that's id");
        }
        product.IsDeleted = false;
        product.IsActive = true;
        LogAdminAction($"Restored product {product.Id}");
        await _db.SaveChangesAsync();
        return Ok("You returned product");
    }


    [Authorize(Roles = "Admin")]
    [HttpPatch("order/{id}/status/")]

    public async Task<IActionResult> ChangeOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var status = request.Status.Trim();

        var listOfStatus = new OrderStatus();
        if (!listOfStatus.statuses.Contains(status))
        {
            return BadRequest("Rewrite status. Incorrect value");
        }
        var order = await _db.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound("There isn't element with that's id");
        }

        if (order.Status == "Completed" || order.Status == "Cancelled")
        {
            return Conflict($"Order is final ({order.Status}). Status cannot be changed.");
        }

        if (order.Status == status)
        {
            return Conflict("Order already has this status");
        }

        if (order.Status == "Created")
        {
            if (status != "Paid" && status != "Cancelled")
                return Conflict($"Cannot change status from {order.Status} to {status}");
        }

        else if (order.Status == "Paid")
        {
            if (status != "Shipped" && status != "Cancelled")
                return Conflict($"Cannot change status from {order.Status} to {status}");
        }

        else if (order.Status == "Shipped")
        {
            if (status != "Completed")
                return Conflict($"Cannot change status from {order.Status} to {status}");
        }

        else
        {
            return Conflict($"Cannot change status from {order.Status} to {status}");
        }
        order.Status = status;
        LogAdminAction($"Changed order {order.Id} status to {status}");
        await _db.SaveChangesAsync();
        return Ok(new { order.Id, order.Status });
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("order")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Select(o => new
            {
                o.Id,
                o.UserId,
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
        LogAdminAction("Viewed all orders");
        await _db.SaveChangesAsync();
        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.Id == id)
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
        LogAdminAction($"Viewed order {id}");
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users
            .Select(u => new
            {
                u.Id,
                u.Email
            })
            .ToListAsync();
        LogAdminAction("Viewed users list");
        await _db.SaveChangesAsync();
        return Ok(users);
    }
}
