using System;
using System.Globalization;
using System.Linq;
using OrderManagement.Models;
using OrderManagement.Services;

namespace OrderManagement.ConsoleApp
{
    class Program
    {
        private static OrderService _orderService;
        private static int _currentPage = 1;
        private const int _pageSize = 10;
        private static string _searchTerm = "";

        static void Main(string[] args)
        {
            Console.Title = "Order Management System";
            _orderService = new OrderService();

            DisplayWelcomeScreen();
            ShowMainMenu();
        }

        static void DisplayWelcomeScreen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================");
            Console.WriteLine("     ORDER MANAGEMENT SYSTEM v1.0        ");
            Console.WriteLine("==========================================");
            Console.ResetColor();
            Console.WriteLine("\nEnglish Language Application");

            // Display statistics
            var stats = _orderService.GetOrderStatistics();
            Console.WriteLine("\n=== SYSTEM STATISTICS ===");
            Console.WriteLine($"Total Orders: {stats.TotalOrders}");
            Console.WriteLine($"Pending Orders: {stats.PendingOrders}");
            Console.WriteLine($"Delivered Orders: {stats.DeliveredOrders}");
            Console.WriteLine($"Total Revenue: ${stats.TotalRevenue:F2}");

            Console.WriteLine("\nPress any key to continue to Main Menu...");
            Console.ReadKey();
        }

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=== MAIN MENU ===");
                Console.ResetColor();

                Console.WriteLine("1. 📋 List All Orders");
                Console.WriteLine("2. 🔍 Search Orders");
                Console.WriteLine("3. ➕ Create New Order");
                Console.WriteLine("4. ✏️  Update Order");
                Console.WriteLine("5. 🗑️  Delete Order");
                Console.WriteLine("6. 📦 View Products");
                Console.WriteLine("7. 📊 View Statistics");
                Console.WriteLine("8. 🚪 Exit");
                Console.Write("\nSelect an option (1-8): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _searchTerm = "";
                        ListOrders();
                        break;
                    case "2":
                        SearchOrders();
                        break;
                    case "3":
                        CreateOrder();
                        break;
                    case "4":
                        UpdateOrder();
                        break;
                    case "5":
                        DeleteOrder();
                        break;
                    case "6":
                        ViewProducts();
                        break;
                    case "7":
                        ViewStatistics();
                        break;
                    case "8":
                        ExitApplication();
                        return;
                    default:
                        DisplayError("Invalid option. Please try again.");
                        WaitForKey();
                        break;
                }
            }
        }

        // 2.2. READ: List all orders with pagination
        static void ListOrders()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=== ORDER LIST ===");
                Console.ResetColor();

                if (!string.IsNullOrEmpty(_searchTerm))
                {
                    Console.WriteLine($"Searching for: '{_searchTerm}'");
                }

                var result = _orderService.GetOrders(_currentPage, _pageSize, _searchTerm);

                if (result.Orders.Count == 0)
                {
                    Console.WriteLine("\n❌ No orders found.");
                    WaitForKey();
                    return;
                }

                // Display table header
                Console.WriteLine(new string('═', 140));
                Console.WriteLine($"║ {"Order #",-18} ║ {"Customer",-20} ║ {"Email",-25} ║ {"Product",-25} ║ {"Qty",4} ║ {"Order Date",-12} ║ {"Status",-10} ║");
                Console.WriteLine(new string('═', 140));

                // Display order data
                foreach (var order in result.Orders)
                {
                    var status = order.DeliveryDate.HasValue ? "Delivered" : "Pending";
                    var statusColor = status == "Delivered" ? ConsoleColor.Green : ConsoleColor.Yellow;

                    Console.Write($"║ {order.OrderNumber,-18} ║ ");
                    Console.Write($"{Truncate(order.CustomerName, 20),-20} ║ ");
                    Console.Write($"{Truncate(order.CustomerEmail, 25),-25} ║ ");
                    Console.Write($"{Truncate(order.Product?.Name ?? "N/A", 25),-25} ║ ");
                    Console.Write($"{order.Quantity,4} ║ ");
                    Console.Write($"{order.OrderDate:yyyy-MM-dd,-12} ║ ");

                    Console.ForegroundColor = statusColor;
                    Console.Write($"{status,-10}");
                    Console.ResetColor();
                    Console.WriteLine(" ║");
                }
                Console.WriteLine(new string('═', 140));

                // Display pagination info
                Console.WriteLine($"\n📄 Page {_currentPage} of {result.TotalPages}");
                Console.WriteLine($"📊 Total Orders: {result.TotalCount}");

                // Pagination controls
                if (result.TotalPages > 1)
                {
                    Console.WriteLine("\nNavigation: [N]ext | [P]revious | [M]ain Menu");
                    Console.Write("Enter choice: ");
                    var navChoice = Console.ReadLine()?.ToUpper();

                    switch (navChoice)
                    {
                        case "N":
                            if (_currentPage < result.TotalPages)
                                _currentPage++;
                            break;
                        case "P":
                            if (_currentPage > 1)
                                _currentPage--;
                            break;
                        case "M":
                            return;
                    }
                }
                else
                {
                    WaitForKey();
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Error displaying orders: {ex.Message}");
            }
        }

        static void SearchOrders()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== SEARCH ORDERS ===");
            Console.ResetColor();

            Console.WriteLine("\nSearch Options:");
            Console.WriteLine("1. Search by Order Number");
            Console.WriteLine("2. Search by Customer Name");
            Console.WriteLine("3. Search by Product Name");
            Console.WriteLine("4. Search by Customer Email");
            Console.WriteLine("5. Advanced Search");
            Console.Write("\nSelect search option (1-5): ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.Write("Enter Order Number: ");
                    _searchTerm = Console.ReadLine();
                    break;
                case "2":
                    Console.Write("Enter Customer Name: ");
                    _searchTerm = Console.ReadLine();
                    break;
                case "3":
                    Console.Write("Enter Product Name: ");
                    _searchTerm = Console.ReadLine();
                    break;
                case "4":
                    Console.Write("Enter Customer Email: ");
                    _searchTerm = Console.ReadLine();
                    break;
                case "5":
                    AdvancedSearch();
                    return;
                default:
                    DisplayError("Invalid option.");
                    return;
            }

            _currentPage = 1;
            ListOrders();
        }

        static void AdvancedSearch()
        {
            Console.Clear();
            Console.WriteLine("=== ADVANCED SEARCH ===");

            Console.Write("Enter keyword (optional): ");
            var keyword = Console.ReadLine();

            Console.Write("Status (All/Pending/Delivered): ");
            var status = Console.ReadLine();

            Console.Write("From Date (yyyy-MM-dd) (optional): ");
            var fromDateInput = Console.ReadLine();
            DateTime? fromDate = null;
            if (!string.IsNullOrWhiteSpace(fromDateInput) &&
                DateTime.TryParseExact(fromDateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fd))
            {
                fromDate = fd;
            }

            Console.Write("To Date (yyyy-MM-dd) (optional): ");
            var toDateInput = Console.ReadLine();
            DateTime? toDate = null;
            if (!string.IsNullOrWhiteSpace(toDateInput) &&
                DateTime.TryParseExact(toDateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime td))
            {
                toDate = td;
            }

            var orders = _orderService.SearchOrders(keyword, status, fromDate, toDate);

            if (orders.Count == 0)
            {
                Console.WriteLine("\n❌ No orders found.");
            }
            else
            {
                Console.WriteLine($"\n✅ Found {orders.Count} order(s):");
                Console.WriteLine(new string('═', 140));
                Console.WriteLine($"║ {"Order #",-18} ║ {"Customer",-20} ║ {"Product",-25} ║ {"Qty",4} ║ {"Order Date",-12} ║ {"Status",-10} ║");
                Console.WriteLine(new string('═', 140));

                foreach (var order in orders.Take(20))
                {
                    var statusText = order.DeliveryDate.HasValue ? "Delivered" : "Pending";
                    Console.WriteLine($"║ {order.OrderNumber,-18} ║ {Truncate(order.CustomerName, 20),-20} ║ " +
                                    $"{Truncate(order.Product?.Name ?? "N/A", 25),-25} ║ {order.Quantity,4} ║ " +
                                    $"{order.OrderDate:yyyy-MM-dd,-12} ║ {statusText,-10} ║");
                }
                Console.WriteLine(new string('═', 140));
            }

            WaitForKey();
        }

        static void ViewProducts()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("=== PRODUCT LIST ===");
                Console.ResetColor();

                var products = _orderService.GetAllProducts();

                if (products.Count == 0)
                {
                    Console.WriteLine("\n❌ No products found.");
                    WaitForKey();
                    return;
                }

                Console.WriteLine(new string('═', 100));
                Console.WriteLine($"║ {"ID",4} ║ {"Name",-30} ║ {"SKU",-15} ║ {"Price",10} ║ {"Stock",6} ║ {"Category",-15} ║");
                Console.WriteLine(new string('═', 100));

                foreach (var product in products)
                {
                    var stockColor = product.StockQuantity > 10 ? ConsoleColor.Green :
                                   product.StockQuantity > 0 ? ConsoleColor.Yellow : ConsoleColor.Red;

                    Console.Write($"║ {product.Id,4} ║ ");
                    Console.Write($"{Truncate(product.Name, 30),-30} ║ ");
                    Console.Write($"{product.SKU,-15} ║ ");
                    Console.Write($"${product.Price,9:F2} ║ ");

                    Console.ForegroundColor = stockColor;
                    Console.Write($"{product.StockQuantity,6}");
                    Console.ResetColor();
                    Console.WriteLine($" ║ {Truncate(product.Category, 15),-15} ║");
                }
                Console.WriteLine(new string('═', 100));

                Console.WriteLine($"\n📦 Total Products: {products.Count}");
                Console.WriteLine($"💰 Total Value: ${products.Sum(p => p.Price * p.StockQuantity):F2}");

                WaitForKey();
            }
            catch (Exception ex)
            {
                DisplayError($"Error displaying products: {ex.Message}");
            }
        }

        // 2.1. CREATE: Add new order
        static void CreateOrder()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("=== CREATE NEW ORDER ===");
                Console.ResetColor();

                // Display available products
                var products = _orderService.GetAllProducts();
                if (!products.Any())
                {
                    DisplayError("No products available. Please add products first.");
                    WaitForKey();
                    return;
                }

                Console.WriteLine("\n📦 Available Products:");
                Console.WriteLine(new string('═', 90));
                Console.WriteLine($"║ {"ID",4} ║ {"Name",-30} ║ {"Price",10} ║ {"Stock",6} ║ {"Category",-15} ║");
                Console.WriteLine(new string('═', 90));

                foreach (var product in products)
                {
                    Console.WriteLine($"║ {product.Id,4} ║ " +
                                    $"{Truncate(product.Name, 30),-30} ║ " +
                                    $"${product.Price,9:F2} ║ " +
                                    $"{product.StockQuantity,6} ║ " +
                                    $"{Truncate(product.Category, 15),-15} ║");
                }
                Console.WriteLine(new string('═', 90));

                var order = new Order();

                // Get product ID
                Console.Write("\nEnter Product ID: ");
                if (!int.TryParse(Console.ReadLine(), out int productId))
                {
                    DisplayError("Invalid Product ID.");
                    WaitForKey();
                    return;
                }
                order.ProductId = productId;

                // Get product to check stock
                var product = _orderService.GetProductById(productId);
                if (product == null)
                {
                    DisplayError("Product not found.");
                    WaitForKey();
                    return;
                }

                // Get order number
                Console.Write("Enter Order Number (format: ORD-YYYYMMDD-XXXX or press Enter to auto-generate): ");
                var orderNumberInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(orderNumberInput))
                {
                    order.OrderNumber = $"ORD-{DateTime.Today:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                    Console.WriteLine($"Generated Order Number: {order.OrderNumber}");
                }
                else
                {
                    order.OrderNumber = orderNumberInput;
                }

                // Get customer details with validation
                string customerName;
                do
                {
                    Console.Write("Enter Customer Name (2-100 characters): ");
                    customerName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(customerName) || customerName.Length < 2 || customerName.Length > 100)
                    {
                        DisplayError("Customer name must be between 2 and 100 characters.");
                    }
                } while (string.IsNullOrWhiteSpace(customerName) || customerName.Length < 2 || customerName.Length > 100);
                order.CustomerName = customerName;

                string customerEmail;
                do
                {
                    Console.Write("Enter Customer Email: ");
                    customerEmail = Console.ReadLine();
                    if (!IsValidEmail(customerEmail))
                    {
                        DisplayError("Invalid email format.");
                    }
                } while (!IsValidEmail(customerEmail));
                order.CustomerEmail = customerEmail;

                // Get quantity with stock validation
                int quantity;
                do
                {
                    Console.Write($"Enter Quantity (1-{product.StockQuantity}): ");
                    if (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0 || quantity > product.StockQuantity)
                    {
                        DisplayError($"Quantity must be between 1 and {product.StockQuantity}.");
                    }
                } while (quantity <= 0 || quantity > product.StockQuantity);
                order.Quantity = quantity;

                // Get order date
                Console.Write("Enter Order Date (yyyy-MM-dd) [Enter for today]: ");
                var dateInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(dateInput))
                {
                    order.OrderDate = DateTime.Today;
                }
                else if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd",
                         CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime orderDate))
                {
                    Console.WriteLine("⚠ Invalid date format. Using today's date.");
                    order.OrderDate = DateTime.Today;
                }
                else
                {
                    order.OrderDate = orderDate;
                }

                // Get delivery date (optional)
                Console.Write("Enter Delivery Date (yyyy-MM-dd) [Optional, press Enter to skip]: ");
                var deliveryInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(deliveryInput))
                {
                    if (DateTime.TryParseExact(deliveryInput, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deliveryDate))
                    {
                        if (deliveryDate < order.OrderDate)
                        {
                            Console.WriteLine("⚠ Delivery date cannot be earlier than order date. Skipping delivery date.");
                        }
                        else
                        {
                            order.DeliveryDate = deliveryDate;
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠ Invalid date format. Skipping delivery date.");
                    }
                }

                // Confirm creation
                Console.WriteLine("\n=== ORDER SUMMARY ===");
                Console.WriteLine($"Order Number: {order.OrderNumber}");
                Console.WriteLine($"Customer: {order.CustomerName}");
                Console.WriteLine($"Email: {order.CustomerEmail}");
                Console.WriteLine($"Product: {product.Name}");
                Console.WriteLine($"Quantity: {order.Quantity}");
                Console.WriteLine($"Order Date: {order.OrderDate:yyyy-MM-dd}");
                Console.WriteLine($"Delivery Date: {(order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("yyyy-MM-dd") : "Not set")}");
                Console.WriteLine($"Total Amount: ${(order.Quantity * product.Price):F2}");

                Console.Write("\nConfirm creation? (Y/N): ");
                var confirm = Console.ReadLine()?.ToUpper();

                if (confirm == "Y")
                {
                    // Attempt to create order
                    var result = _orderService.CreateOrder(order);
                    if (result.Success)
                    {
                        DisplaySuccess($"✅ {result.Message}");
                        Console.WriteLine($"Order Number: {order.OrderNumber}");
                        Console.WriteLine($"Remaining Stock: {product.StockQuantity - order.Quantity}");
                    }
                    else
                    {
                        DisplayError($"❌ {result.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Order creation cancelled.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Error creating order: {ex.Message}");
            }

            WaitForKey();
        }

        // 2.3. UPDATE: Modify existing order
        static void UpdateOrder()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=== UPDATE ORDER ===");
                Console.ResetColor();

                Console.Write("Enter Order Number to update: ");
                var orderNumber = Console.ReadLine();

                var order = _orderService.GetOrderByOrderNumber(orderNumber);
                if (order == null)
                {
                    DisplayError("Order not found.");
                    WaitForKey();
                    return;
                }

                // Display current information
                Console.WriteLine("\n📋 Current Order Information:");
                Console.WriteLine(new string('─', 60));
                Console.WriteLine($"Order Number: {order.OrderNumber}");
                Console.WriteLine($"Customer Name: {order.CustomerName}");
                Console.WriteLine($"Customer Email: {order.CustomerEmail}");
                Console.WriteLine($"Product: {order.Product?.Name} (ID: {order.ProductId})");
                Console.WriteLine($"Current Quantity: {order.Quantity}");
                Console.WriteLine($"Order Date: {order.OrderDate:yyyy-MM-dd}");
                Console.WriteLine($"Delivery Date: {(order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("yyyy-MM-dd") : "Not delivered")}");
                Console.WriteLine($"Status: {(order.DeliveryDate.HasValue ? "Delivered" : "Pending")}");
                Console.WriteLine(new string('─', 60));

                Console.WriteLine("\n📝 Enter new information (press Enter to keep current value):");

                // Update customer name
                Console.Write($"New Customer Name [{order.CustomerName}]: ");
                var newName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    if (newName.Length < 2 || newName.Length > 100)
                    {
                        DisplayError("Customer name must be between 2 and 100 characters.");
                    }
                    else
                    {
                        order.CustomerName = newName;
                    }
                }

                // Update customer email
                Console.Write($"New Customer Email [{order.CustomerEmail}]: ");
                var newEmail = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newEmail))
                {
                    if (!IsValidEmail(newEmail))
                    {
                        DisplayError("Invalid email format.");
                    }
                    else
                    {
                        order.CustomerEmail = newEmail;
                    }
                }

                // Update quantity
                Console.Write($"New Quantity [{order.Quantity}]: ");
                var quantityInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(quantityInput) && int.TryParse(quantityInput, out int newQuantity))
                {
                    if (newQuantity <= 0 || newQuantity > 1000)
                    {
                        DisplayError("Quantity must be between 1 and 1000.");
                    }
                    else
                    {
                        order.Quantity = newQuantity;
                    }
                }

                // Update delivery date
                Console.Write($"New Delivery Date [{(order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("yyyy-MM-dd") : "Not set")}]: ");
                var deliveryInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(deliveryInput))
                {
                    if (deliveryInput.ToLower() == "clear")
                    {
                        order.DeliveryDate = null;
                    }
                    else if (DateTime.TryParseExact(deliveryInput, "yyyy-MM-dd",
                             CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newDeliveryDate))
                    {
                        if (newDeliveryDate < order.OrderDate)
                        {
                            DisplayError("Delivery date cannot be earlier than order date.");
                        }
                        else
                        {
                            order.DeliveryDate = newDeliveryDate;
                        }
                    }
                    else
                    {
                        DisplayError("Invalid date format. Use yyyy-MM-dd or 'clear' to remove delivery date.");
                    }
                }

                // Confirm update
                Console.WriteLine("\n=== UPDATED ORDER SUMMARY ===");
                Console.WriteLine($"Customer Name: {order.CustomerName}");
                Console.WriteLine($"Customer Email: {order.CustomerEmail}");
                Console.WriteLine($"Quantity: {order.Quantity}");
                Console.WriteLine($"Delivery Date: {(order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("yyyy-MM-dd") : "Not set")}");

                Console.Write("\nConfirm update? (Y/N): ");
                var confirm = Console.ReadLine()?.ToUpper();

                if (confirm == "Y")
                {
                    var result = _orderService.UpdateOrder(order);
                    if (result.Success)
                    {
                        DisplaySuccess($"✅ {result.Message}");
                    }
                    else
                    {
                        DisplayError($"❌ {result.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Order update cancelled.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Error updating order: {ex.Message}");
            }

            WaitForKey();
        }

        // 2.4. DELETE: Remove order with confirmation
        static void DeleteOrder()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("=== DELETE ORDER ===");
                Console.ResetColor();

                Console.Write("Enter Order Number to delete: ");
                var orderNumber = Console.ReadLine();

                var order = _orderService.GetOrderByOrderNumber(orderNumber);
                if (order == null)
                {
                    DisplayError("Order not found.");
                    WaitForKey();
                    return;
                }

                // Display order information for confirmation
                Console.WriteLine("\n⚠ WARNING: This action cannot be undone!");
                Console.WriteLine(new string('═', 60));
                Console.WriteLine($"Order Number: {order.OrderNumber}");
                Console.WriteLine($"Customer: {order.CustomerName}");
                Console.WriteLine($"Product: {order.Product?.Name}");
                Console.WriteLine($"Quantity: {order.Quantity}");
                Console.WriteLine($"Order Date: {order.OrderDate:yyyy-MM-dd}");
                Console.WriteLine($"Status: {(order.DeliveryDate.HasValue ? "Delivered" : "Pending")}");
                Console.WriteLine(new string('═', 60));

                // Double confirmation
                Console.Write("\nAre you sure you want to delete this order? (YES/NO): ");
                var confirmation = Console.ReadLine()?.ToUpper();

                if (confirmation == "YES")
                {
                    Console.Write("Type 'DELETE' to confirm: ");
                    var finalConfirm = Console.ReadLine()?.ToUpper();

                    if (finalConfirm == "DELETE")
                    {
                        var result = _orderService.DeleteOrder(order.Id);
                        if (result.Success)
                        {
                            DisplaySuccess($"✅ {result.Message}");
                            Console.WriteLine($"Stock restored: {order.Quantity} units added back to {order.Product?.Name}");
                        }
                        else
                        {
                            DisplayError($"❌ {result.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Deletion cancelled.");
                    }
                }
                else
                {
                    Console.WriteLine("Deletion cancelled.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Error deleting order: {ex.Message}");
            }

            WaitForKey();
        }

        static void ViewStatistics()
        {