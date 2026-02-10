using Azure.Core.Pipeline;
using BigElephant.Controllers;
using BigElephant.Data;
using BigElephant.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.OpenApi.Validations;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Windows.Markup;
using Xunit;
using Moq;
using System.Collections;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BigElephantTest
{
    public class UnitTest1
    {
        //
        // Test for AdminController
        //

        //
        // Test for product's logic
        //

        [Fact]
        public async Task PostNewProduct_Creates_Product()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var request = new AdminController.CreateProductRequest
            {
                Name = "Test product",
                Price = 100,
                Stock = 10,
                IsActive = true
            };

            var result = await controller.PostNewProduct(request);

            var createdResult = Assert.IsType<CreatedResult>(result);
            var product = Assert.IsType<Product>(createdResult.Value);

            Assert.Equal("Test product", product.Name);
            Assert.Equal(100, product.Price);
            Assert.Equal(10, product.Stock);
            Assert.True(product.IsActive);

            Assert.Single(db.Products);
        }

        [Fact]
        public async Task GetProducts_Returns_All_Products()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            db.Products.Add(new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true
            });

            db.Products.Add(new Product
            {
                Name = "Test product2",
                Price = 200,
                Stock = 20,
                IsActive = true
            });

            await db.SaveChangesAsync();

            var result = await controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var products = Assert.IsType<List<Product>>(okResult.Value);

            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetProductById_Returns_404_When_Not_Exists()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.GetProductById(1);

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetProductById_Returns_Product_When_Exists()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var result = await controller.GetProductById(product.Id);

            var getResult = Assert.IsType<OkObjectResult>(result);
            var getProduct = Assert.IsType<Product>(getResult.Value);

            Assert.Equal("Test product1", getProduct.Name);
            Assert.Equal(100, getProduct.Price);
            Assert.Equal(10, getProduct.Stock);
            Assert.True(getProduct.IsActive);
        }

        [Fact]
        public async Task PatchProductStock_Updates_Stock()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var changing = new AdminController.UpdateProductStockRequest { Stock = 25 };

            var result = await controller.PatchProductStock(product.Id, changing);

            Assert.IsType<OkObjectResult>(result);

            var updateProduct = await db.Products.FindAsync(product.Id);

            Assert.NotNull(updateProduct);
            Assert.Equal(updateProduct.Stock, changing.Stock);
        }
        [Fact]
        public async Task PatchProductStock_Returns_404_When_Product_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.PatchProductStock(1, new AdminController.UpdateProductStockRequest() { Stock = 1 });

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PatchProductName_Updates_Name()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var changing = new AdminController.UpdateProductNameRequest { Name = "Test product2" };

            var result = await controller.PatchProductName(product.Id, changing);

            Assert.IsType<OkObjectResult>(result);

            var updateProduct = await db.Products.FindAsync(product.Id);

            Assert.NotNull(updateProduct);
            Assert.Equal(updateProduct.Name, changing.Name);
        }

        [Fact]
        public async Task PatchProductName_Returns_404_When_Product_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.PatchProductName(1, new AdminController.UpdateProductNameRequest { Name = "hi" });

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PatchProductPrice_Updates_Price()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var changing = new AdminController.UpdateProductPriceRequest { Price = 200 };

            var result = await controller.PatchProductPrice(product.Id, changing);

            Assert.IsType<OkObjectResult>(result);

            var updateProduct = await db.Products.FindAsync(product.Id);

            Assert.NotNull(updateProduct);
            Assert.Equal(updateProduct.Price, changing.Price);
        }

        [Fact]
        public async Task PatchProductPrice_Returns_404_When_Product_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.PatchProductPrice(1, new AdminController.UpdateProductPriceRequest { Price = 200 });

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PatchProductIsActive_Updates_IsActive()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var changing = new AdminController.UpdateProductIsActiveRequest { IsActive = false };

            var result = await controller.PatchProductIsActive(product.Id, changing);

            Assert.IsType<OkObjectResult>(result);

            var updateProduct = await db.Products.FindAsync(product.Id);

            Assert.NotNull(updateProduct);
            Assert.Equal(updateProduct.IsActive, changing.IsActive);
        }

        [Fact]
        public async Task PatchProductIsActive_Returns_404_When_Product_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.PatchProductIsActive(1, new AdminController.UpdateProductIsActiveRequest { IsActive = false });

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_Sets_IsDeleted_True_And_Disables_Product()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = true,
                IsDeleted = false
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var result = await controller.DeleteProduct(product.Id);

            Assert.IsType<OkObjectResult>(result);

            var updateProduct = await db.Products.FindAsync(product.Id);

            Assert.Equal(updateProduct.IsDeleted, true);
            Assert.Equal(updateProduct.IsActive, false);
        }

        [Fact]
        public async Task DeleteProduct_Returns_404_When_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.DeleteProduct(1);

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReturnProduct_Restores_IsDeleted_False_And_Activates()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "Test product1",
                Price = 100,
                Stock = 10,
                IsActive = false,
                IsDeleted = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var result = await controller.ReturnProduct(product.Id);

            Assert.IsType<OkObjectResult>(result);

            var updateProduct = await db.Products.FindAsync(product.Id);

            Assert.Equal(updateProduct.IsDeleted, false);
            Assert.Equal(updateProduct.IsActive, true);
        }

        [Fact]
        public async Task ReturnProduct_Returns_404_When_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.ReturnProduct(1);

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        //
        // Test for order's logic
        //

        [Fact]
        public async Task GetOrderById_Returns_404_When_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.GetOrderById(1);

            var NotFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetOrderById_Returns_Order_With_Items_When_Exists()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "iPhone 15",
                Price = 999,
                Stock = 10,
                IsActive = true,
                IsDeleted = false
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = "user-1",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = 2,
                UnitPrice = product.Price
            });
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = await controller.GetOrderById(order.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var orderResult = okResult.Value;

            Assert.NotNull(orderResult);

            var type = orderResult.GetType();

            Assert.Equal(order.Id, (int)type.GetProperty("Id").GetValue(orderResult));
            Assert.Equal("Created", (string)type.GetProperty("Status").GetValue(orderResult));

            var items = (System.Collections.IEnumerable)
                type.GetProperty("items").GetValue(orderResult);

            Assert.Single(items);
        }

        [Fact]
        public async Task GetOrders_Returns_All_Orders_With_Items()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var product = new Product
            {
                Name = "iPhone 15",
                Price = 999,
                Stock = 10,
                IsActive = true,
                IsDeleted = false
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = "user-1",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = 2,
                UnitPrice = product.Price
            });
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var product1 = new Product
            {
                Name = "iPhone 16",
                Price = 1000,
                Stock = 10,
                IsActive = true,
                IsDeleted = false
            };
            db.Products.Add(product1);
            await db.SaveChangesAsync();

            var order1 = new Order
            {
                UserId = "user-2",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            order1.Items.Add(new OrderItem
            {
                ProductId = product1.Id,
                Quantity = 3,
                UnitPrice = product1.Price
            });
            db.Orders.Add(order1);
            await db.SaveChangesAsync();

            var result = await controller.GetOrders();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var ordersResult = okResult.Value;

            Assert.NotNull(ordersResult);

            var ordersEnumerable = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ordersResult);

            var ordersList = ordersEnumerable.Cast<object>().ToList();
            Assert.Equal(2, ordersList.Count);

            foreach (var o in ordersList)
            {
                var orderType = o.GetType();

                var itemsObj = orderType.GetProperty("items")!.GetValue(o);
                Assert.NotNull(itemsObj);

                var itemsEnumerable = Assert.IsAssignableFrom<System.Collections.IEnumerable>(itemsObj);
                var itemsList = itemsEnumerable.Cast<object>().ToList();

                Assert.Single(itemsList);
            }

        }

        [Fact]
        public async Task GetOrders_Returns_Empty_List_When_No_Orders()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.GetOrders();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var ordersResult = okResult.Value;

            Assert.NotNull(ordersResult);

            var ordersEnumerable = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ordersResult);
            var ordersList = ordersEnumerable.Cast<object>().ToList();

            Assert.Empty(ordersList);
        }

        [Fact]
        public async Task ChangeOrderStatus_Returns_400_When_Status_Invalid()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = await controller.ChangeOrderStatus(order.Id, new AdminController.UpdateOrderStatusRequest { Status = "aaaaaaa" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ChangeOrderStatus_Returns_404_When_Order_Not_Found()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var result = await controller.ChangeOrderStatus(1, new AdminController.UpdateOrderStatusRequest { Status = "Created" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ChangeOrderStatus_Returns_409_When_Order_Final()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = await controller.ChangeOrderStatus(order.Id, new AdminController.UpdateOrderStatusRequest { Status = "Shipped" });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task ChangeOrderStatus_Returns_409_When_Same_Status()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Shipped",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = await controller.ChangeOrderStatus(order.Id, new AdminController.UpdateOrderStatusRequest { Status = "Shipped" });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task ChangeOrderStatus_Allows_Created_To_Paid()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Paid" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<OkObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, request.Status);
        }

        [Fact]
        public async Task ChangeOrderStatus_Allows_Created_To_Cancelled()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Cancelled" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<OkObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, request.Status);
        }

        [Fact]
        public async Task ChangeOrderStatus_Denies_Created_To_Shipped()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Shipped" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<ConflictObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, "Created");
        }

        [Fact]
        public async Task ChangeOrderStatus_Allows_Paid_To_Shipped()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Paid",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Shipped" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<OkObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, request.Status);
        }

        [Fact]
        public async Task ChangeOrderStatus_Allows_Paid_To_Cancelled()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Paid",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Cancelled" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<OkObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, request.Status);
        }

        [Fact]
        public async Task ChangeOrderStatus_Denies_Paid_To_Completed()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Paid",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Completed" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<ConflictObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, "Paid");
        }

        [Fact]
        public async Task ChangeOrderStatus_Allows_Shipped_To_Completed()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Shipped",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Completed" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<OkObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, request.Status);
        }

        [Fact]
        public async Task ChangeOrderStatus_Denies_Shipped_To_Paid()
        {
            var db = TestDbContextFactory.Create();
            var controller = new AdminController(db, userManager: null!);

            var order = new Order
            {
                UserId = "user-2",
                Status = "Shipped",
                CreatedAt = DateTime.UtcNow
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var request = new AdminController.UpdateOrderStatusRequest { Status = "Paid" };

            var result = await controller.ChangeOrderStatus(order.Id, request);

            Assert.IsType<ConflictObjectResult>(result);

            var updatedOrder = await db.Orders.FindAsync(order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedOrder.Status, "Shipped");
        }

        //
        // Test for UserController
        //

        [Fact]
        public async Task GetProducts_Returns_Only_Active_And_NotDeleted()
        {
            var db = TestDbContextFactory.Create();
            var controller = new UserController(db);

            var product1 = new Product
            {
                Name = "iPhone 15",
                Price = 999,
                Stock = 10,
                IsActive = true,
                IsDeleted = false
            };
            var product2 = new Product
            {
                Name = "iPhone 16",
                Price = 999,
                Stock = 10,
                IsActive = false,
                IsDeleted = false
            };
            var product3 = new Product
            {
                Name = "iPhone 17",
                Price = 999,
                Stock = 10,
                IsActive = false,
                IsDeleted = true
            };

            db.Products.Add(product1);
            db.Products.Add(product2);
            db.Products.Add(product3);

            await db.SaveChangesAsync();

            var result = controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value).ToList();

            Assert.NotNull(listProducts);
            Assert.Equal(listProducts.Count, 1);
        }

        [Fact]
        public async Task GetProducts_Empty_When_All_Inactive_Or_Deleted()
        {
            var db = TestDbContextFactory.Create();
            var controller = new UserController(db);

            var product2 = new Product
            {
                Name = "iPhone 16",
                Price = 999,
                Stock = 10,
                IsActive = false,
                IsDeleted = false
            };
            var product3 = new Product
            {
                Name = "iPhone 17",
                Price = 999,
                Stock = 10,
                IsActive = false,
                IsDeleted = true
            };

            db.Products.Add(product2);
            db.Products.Add(product3);

            await db.SaveChangesAsync();

            var result = controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value).ToList();

            Assert.NotNull(listProducts);
            Assert.Empty(listProducts);
        }
    }
}