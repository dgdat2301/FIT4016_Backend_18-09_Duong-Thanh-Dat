using OrderManagement.Data;
using OrderManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OrderManagement.Services
{
    public class OrderService : IDisposable
    {
        private readonly OrderDbContext _context;

        public OrderService()
        {
            _context = new OrderDbContext();
        }

        // 2.1. CREATE Order
        public (bool Success, string Message) CreateOrder(Order order)
        {
            try
            {
                // Validate order data
                var validationResult = ValidateOrder(order, isUpdate: false);
                if (!validationResult.Success)
                    return validationResult;

                // Check if order number already exists
                if (_context.Orders.Any(o => o.OrderNumber == order.OrderNumber))
                    return (false, "Order number already exists. Please use a different order number.");

                // Check if customer email already exists
                if (_context.Orders.Any(o => o.CustomerEmail == order.CustomerEmail))
                    return (false, "Customer email already exists. Please use a different email.");

                // Check if product exists
                var product = _context.Products.Find(order.ProductId);
                if (product == null)
                    return (false, "Selected product does not exist.");

                // Check stock quantity
                if (order.Quantity > product.StockQuantity)
                    return (false, $"Insufficient stock. Available: {product.StockQuantity}, Requested: {order.Quantity}");

                // Generate order number if not provided
                if (string.IsNullOrEmpty(order.OrderNumber))
                {
                    order.OrderNumber = GenerateOrderNumber();
                }

                // Set timestamps
                order.CreatedAt = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                _context.Orders.Add(order);
                _context.SaveChanges();

                // Update product stock
                product.StockQuantity -= order.Quantity;
                _context.Entry(product).State = EntityState.Modified;
                _context.SaveChanges();

                return (true, "Order created successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error creating order: {ex.Message}");
            }
        }

        // 2.2. READ Orders with Pagination and Search
        public (List<Order> Orders, int TotalCount, int TotalPages) GetOrders(
            int pageNumber = 1,
            int pageSize = 10,
            string searchTerm = "")
        {
            try
            {
                IQueryable<Order> query = _context.Orders
                    .Include(o => o.Product)
                    .OrderByDescending(o => o.OrderDate);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(o =>
                        o.OrderNumber.ToLower().Contains(searchTerm) ||
                        o.CustomerName.ToLower().Contains(searchTerm) ||
                        o.CustomerEmail.ToLower().Contains(searchTerm) ||
                        o.Product.Name.ToLower().Contains(searchTerm));
                }

                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var orders = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (orders, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving orders: {ex.Message}");
                return (new List<Order>(), 0, 0);
            }
        }

        // 2.3. UPDATE Order
        public (bool Success, string Message) UpdateOrder(Order order)
        {
            try
            {
                // Validate order data
                var validationResult = ValidateOrder(order, isUpdate: true);
                if (!validationResult.Success)
                    return validationResult;

                var existingOrder = _context.Orders
                    .Include(o => o.Product)
                    .FirstOrDefault(o => o.Id == order.Id);

                if (existingOrder == null)
                    return (false, "Order not found.");

                // Check if customer email already exists (for other orders)
                if (_context.Orders.Any(o => o.CustomerEmail == order.CustomerEmail && o.Id != order.Id))
                    return (false, "Customer email already exists for another order.");

                // Calculate stock adjustment
                var quantityDifference = order.Quantity - existingOrder.Quantity;

                if (quantityDifference > 0) // Increasing quantity
                {
                    if (quantityDifference > existingOrder.Product.StockQuantity)
                        return (false, $"Insufficient stock. Available: {existingOrder.Product.StockQuantity}, Additional requested: {quantityDifference}");
                }

                // Update order properties
                existingOrder.CustomerName = order.CustomerName;
                existingOrder.CustomerEmail = order.CustomerEmail;
                existingOrder.Quantity = order.Quantity;
                existingOrder.DeliveryDate = order.DeliveryDate;
                existingOrder.UpdatedAt = DateTime.Now;

                // Update product stock
                existingOrder.Product.StockQuantity -= quantityDifference;

                _context.Entry(existingOrder).State = EntityState.Modified;
                _context.SaveChanges();

                return (true, "Order updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating order: {ex.Message}");
            }
        }

        // 2.4. DELETE Order
        public (bool Success, string Message) DeleteOrder(int orderId)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.Product)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                    return (false, "Order not found.");

                // Restore product stock
                order.Product.StockQuantity += order.Quantity;
                _context.Entry(order.Product).State = EntityState.Modified;

                _context.Orders.Remove(order);
                _context.SaveChanges();

                return (true, "Order deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting order: {ex.Message}");
            }
        }

        // Get order by ID
        public Order GetOrderById(int id)
        {
            try
            {
                return _context.Orders
                    .Include(o => o.Product)
                    .FirstOrDefault(o => o.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting order: {ex.Message}");
                return null;
            }
        }

        // Get all products for dropdown
        public List<Product> GetAllProducts()
        {
            try
            {
                return _context.Products
                    .Where(p => p.StockQuantity > 0)
                    .OrderBy(p => p.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
                return new List<Product>();
            }
        }

        // Get product by ID
        public Product GetProductById(int id)
        {
            return _context.Products.Find(id);
        }

        // Validation method
        private (bool Success, string Message) ValidateOrder(Order order, bool isUpdate)
        {
            // Validate Order Number format
            if (!isUpdate && !System.Text.RegularExpressions.Regex.IsMatch(order.OrderNumber ?? "", @"^ORD-\d{8}-\d{4}$"))
                return (false, "Order number must be in format: ORD-YYYYMMDD-XXXX");

            // Validate Customer Name
            if (string.IsNullOrWhiteSpace(order.CustomerName))
                return (false, "Customer name is required.");

            if (order.CustomerName.Length < 2 || order.CustomerName.Length > 100)
                return (false, "Customer name must be between 2 and 100 characters.");

            // Validate Customer Email
            if (string.IsNullOrWhiteSpace(order.CustomerEmail))
                return (false, "Customer email is required.");

            if (!IsValidEmail(order.CustomerEmail))
                return (false, "Invalid email format.");

            // Validate Quantity
            if (order.Quantity <= 0)
                return (false, "Quantity must be greater than 0.");

            if (order.Quantity > 1000)
                return (false, "Quantity cannot exceed 1000.");

            // Validate Product
            if (order.ProductId <= 0)
                return (false, "Product is required.");

            // Validate Order Date
            if (order.OrderDate > DateTime.Today)
                return (false, "Order date cannot be in the future.");

            // Validate Delivery Date
            if (order.DeliveryDate.HasValue && order.DeliveryDate < order.OrderDate)
                return (false, "Delivery date cannot be earlier than order date.");

            return (true, "Validation successful.");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateOrderNumber()
        {
            var today = DateTime.Today;
            var count = _context.Orders
                .Count(o => o.OrderDate.Year == today.Year &&
                           o.OrderDate.Month == today.Month &&
                           o.OrderDate.Day == today.Day);

            return $"ORD-{today:yyyyMMdd}-{(count + 1).ToString("D4")}";
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}