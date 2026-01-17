using System;
using System.Collections.Generic;
using System.Data.Entity;
using OrderManagement.Models;

namespace OrderManagement.Data
{
    public class DatabaseInitializer : CreateDatabaseIfNotExists<OrderDbContext>
    {
        protected override void Seed(OrderDbContext context)
        {
            try
            {
                var products = GenerateSampleProducts();
                var orders = GenerateSampleOrders(products);

                context.Products.AddRange(products);
                context.Orders.AddRange(orders);

                context.SaveChanges();
                Console.WriteLine("Sample data seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data: {ex.Message}");
                throw;
            }
        }

        private List<Product> GenerateSampleProducts()
        {
            var products = new List<Product>
            {
                new Product { Name = "iPhone 15 Pro", SKU = "APP-IP15P-256", Description = "Latest Apple smartphone", Price = 1299.99m, StockQuantity = 50, Category = "Electronics" },
                new Product { Name = "Samsung Galaxy S24", SKU = "SAM-GS24-512", Description = "Android flagship phone", Price = 1199.99m, StockQuantity = 75, Category = "Electronics" },
                new Product { Name = "MacBook Air M2", SKU = "APP-MBA-M2-13", Description = "Lightweight laptop", Price = 1499.99m, StockQuantity = 30, Category = "Computers" },
                new Product { Name = "Sony WH-1000XM5", SKU = "SON-WHXM5", Description = "Noise cancelling headphones", Price = 399.99m, StockQuantity = 100, Category = "Audio" },
                new Product { Name = "Nike Air Max 270", SKU = "NIK-AM270-BLK", Description = "Comfortable running shoes", Price = 159.99m, StockQuantity = 200, Category = "Footwear" },
                new Product { Name = "Levi's 501 Jeans", SKU = "LEV-501-34", Description = "Classic denim jeans", Price = 89.99m, StockQuantity = 150, Category = "Clothing" },
                new Product { Name = "Instant Pot Duo", SKU = "INS-POT-DUO7", Description = "7-in-1 pressure cooker", Price = 129.99m, StockQuantity = 80, Category = "Kitchen" },
                new Product { Name = "Kindle Paperwhite", SKU = "AMA-KPW-11", Description = "Waterproof e-reader", Price = 149.99m, StockQuantity = 120, Category = "Electronics" },
                new Product { Name = "PlayStation 5", SKU = "SON-PS5-DISC", Description = "Gaming console", Price = 499.99m, StockQuantity = 40, Category = "Gaming" },
                new Product { Name = "Dyson V15 Detect", SKU = "DYS-V15-DET", Description = "Cordless vacuum cleaner", Price = 749.99m, StockQuantity = 60, Category = "Home" },
                new Product { Name = "Breville Barista", SKU = "BRE-BAR-EX", Description = "Espresso machine", Price = 899.99m, StockQuantity = 25, Category = "Kitchen" },
                new Product { Name = "GoPro Hero 12", SKU = "GOP-H12-BLK", Description = "Action camera", Price = 399.99m, StockQuantity = 90, Category = "Cameras" },
                new Product { Name = "Fitbit Charge 6", SKU = "FIT-CHRG6-BLK", Description = "Fitness tracker", Price = 159.99m, StockQuantity = 180, Category = "Wearables" },
                new Product { Name = "Logitech MX Keys", SKU = "LOG-MXKEYS", Description = "Wireless keyboard", Price = 99.99m, StockQuantity = 250, Category = "Computers" },
                new Product { Name = "Bose SoundLink", SKU = "BOS-SL2-BLK", Description = "Bluetooth speaker", Price = 299.99m, StockQuantity = 110, Category = "Audio" }
            };

            return products;
        }

        private List<Order> GenerateSampleOrders(List<Product> products)
        {
            var random = new Random();
            var orders = new List<Order>();
            var orderCounter = 1;
            var customerNames = new[] { "John Smith", "Emma Johnson", "Michael Brown", "Sarah Davis", "Robert Wilson", "Jennifer Taylor", "David Anderson", "Jessica Thomas", "James Martinez", "Lisa Garcia" };
            var emailDomains = new[] { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com" };

            for (int i = 0; i < 30; i++)
            {
                var product = products[random.Next(products.Count)];
                var customerName = customerNames[random.Next(customerNames.Length)];
                var nameParts = customerName.Split(' ');
                var email = $"{nameParts[0].ToLower()}.{nameParts[1].ToLower()}@{emailDomains[random.Next(emailDomains.Length)]}";

                var orderDate = DateTime.Today.AddDays(-random.Next(1, 60));
                var deliveryDate = random.Next(3) == 0 ? (DateTime?)null : orderDate.AddDays(random.Next(1, 14));

                var order = new Order
                {
                    ProductId = product.Id,
                    Product = product,
                    OrderNumber = $"ORD-{orderDate:yyyyMMdd}-{orderCounter.ToString("D4")}",
                    CustomerName = customerName,
                    Quantity = random.Next(1, Math.Min(10, product.StockQuantity)),
                    CustomerEmail = $"{customerName.Replace(" ", "").ToLower()}{i}@test.com",
                    OrderDate = orderDate,
                    DeliveryDate = deliveryDate
                };

                orders.Add(order);
                orderCounter++;
            }

            return orders;
        }
    }
}