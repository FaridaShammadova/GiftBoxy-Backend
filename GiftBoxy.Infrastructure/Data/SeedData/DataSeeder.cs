//using GiftBoxy.Domain.Entities;
//using GiftBoxy.Infrastructure.Data.SeedData.DTOs;
//using Microsoft.AspNetCore.Identity;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace GiftBoxy.Infrastructure.Data.SeedData
//{
//    public static class DataSeeder
//    {
//        public static void Seed(AppDbContext context)
//        {
//            SeedCategories(context);
//            SeedUsers(context);
//            SeedSellerProfiles(context);

//            SeedProducts(context);

//            SeedWishlistItems(context);

//            SeedCarts(context);

//            SeedConversations(context);

//            SeedCartItems(context);

//            SeedMessages(context);
//        }

//        private static string ReadJsonFile(string fileName)
//        {
//            var assembly = typeof(DataSeeder).Assembly;
//            var resourceName = assembly.GetManifestResourceNames()
//                .FirstOrDefault(x => x.EndsWith(fileName));

//            if (resourceName is null)
//                throw new FileNotFoundException($"{fileName} embedded resource tapılmadı.");

//            using var stream = assembly.GetManifestResourceStream(resourceName)!;
//            using var reader = new StreamReader(stream);
//            return reader.ReadToEnd();
//        }

//        #region Products
//        private static void SeedProducts(AppDbContext context)
//        {
//            if (context.Products.Any())
//                return;

//            var jsonPath = Path.Combine(
//                AppContext.BaseDirectory,
//                "Infrastructure",
//                "Data",
//                "SeedData",
//                "JsonFiles",
//                "products.json"
//            );

//            var json = ReadJsonFile("products.json");

//            var products = JsonSerializer.Deserialize<List<ProductSeedDto>>(
//                 json,
//                 new JsonSerializerOptions
//                 {
//                     PropertyNameCaseInsensitive = true
//                 });

//            if (products is null)
//                return;

//            foreach (var p in products)
//            {
//                var product = new Product
//                {
//                    Id = p.Id,
//                    Title = p.Title,
//                    Slug = p.Slug,
//                    CategoryId = p.CategoryId,
//                    Price = p.Price,
//                    OldPrice = p.OldPrice,
//                    Rating = p.Rating,
//                    StockCount = p.Stock,
//                    SellerProfileId = p.SellerProfileId,
//                    IsFeatured = p.IsFeatured,
//                    IsBestSeller = p.IsBestSeller,
//                    IsPersonalized = p.IsPersonalized,
//                    IsNew = p.IsNew,
//                    Badge = p.Badge,
//                    Description = p.Description,
//                    BudgetRange = p.BudgetRange
//                };

//                context.Products.Add(product);

//                // IMAGES
//                foreach (var img in p.Images)
//                {
//                    context.ProductImages.Add(new ProductImage
//                    {
//                        Product = product,
//                        ImageUrl = img
//                    });
//                }

//                // RECIPIENT TAGS
//                foreach (var tag in p.RecipientTags)
//                {
//                    context.ProductRecipientTags.Add(new ProductRecipientTag
//                    {
//                        Product = product,
//                        Name = tag
//                    });
//                }

//                // OCCASION TAGS
//                foreach (var tag in p.OccasionTags)
//                {
//                    context.ProductOccasionTags.Add(new ProductOccasionTag
//                    {
//                        Product = product,
//                        Name = tag
//                    });
//                }

//                // INTEREST TAGS
//                foreach (var tag in p.InterestTags)
//                {
//                    context.ProductInterestTags.Add(new ProductInterestTag
//                    {
//                        Product = product,
//                        Name = tag
//                    });
//                }
//            }

//            context.SaveChanges();
//        }
//        #endregion

//        #region Categories
//        private static void SeedCategories(AppDbContext context)
//        {
//            if (context.Categories.Any())
//                return;

//            var jsonPath = Path.Combine(
//                AppContext.BaseDirectory,
//                 "Infrastructure",
//                 "Data",
//                 "SeedData",
//                 "JsonFiles",
//                 "categories.json"
//             );

//            var json = ReadJsonFile("categories.json");

//            var categories = JsonSerializer.Deserialize<List<CategorySeedDto>>(
//                json,
//                new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//            if (categories is null)
//                return;

//            foreach (var c in categories)
//            {
//                var category = new Category
//                {
//                    Id = c.Id,
//                    Name = c.Name,
//                    Slug = c.Slug,
//                    Icon = c.Icon
//                };

//                context.Categories.Add(category);
//            }
//            context.SaveChanges();
//        }
//        #endregion

//        #region SellerProfiles
//        private static void SeedSellerProfiles(AppDbContext context)
//        {
//            if (context.SellerProfiles.Any())
//                return;

//            var jsonPath = Path.Combine(
//                 AppContext.BaseDirectory,
//                 "Infrastructure",
//                 "Data",
//                 "SeedData",
//                 "JsonFiles",
//                 "sellerProfiles.json"
//             );

//            var json = ReadJsonFile("sellerProfiles.json");

//            var sellerProfiles = JsonSerializer.Deserialize<List<SellerProfileSeedDto>>(
//                json,
//                new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//            if (sellerProfiles is null)
//                return;

//            foreach (var s in sellerProfiles)
//            {
//                var sellerProfile = new SellerProfile
//                {
//                    Id = s.Id,
//                    StoreName = s.StoreName,
//                    ShopUrl = s.ShopUrl,
//                    Avatar = s.AvatarUrl,
//                    Rating = s.Rating,
//                    TotalSales = s.TotalSales,
//                    Followers = s.Followers,
//                    Location = s.Location,
//                    Bio = s.Bio,
//                    UserId = s.UserId
//                };

//                context.SellerProfiles.Add(sellerProfile);
//            }

//            context.SaveChanges();
//        }
//        #endregion

//        #region Users
//        private static void SeedUsers(AppDbContext context)
//        {
//            if (context.Users.Any())
//                return;

//            var jsonPath = Path.Combine(
//                AppContext.BaseDirectory,
//                "Infrastructure",
//                "Data",
//                "SeedData",
//                "JsonFiles",
//                "users.json"
//            );

//            var json = ReadJsonFile("users.json");

//            var users = JsonSerializer.Deserialize<List<UserSeedDto>>(
//                json,
//                new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true,
//                    Converters =
//                    {
//                new JsonStringEnumConverter()
//                    }
//                });

//            if (users is null)
//                return;

//            var passwordHasher = new PasswordHasher<AppUser>();

//            foreach (var u in users)
//            {
//                var user = new AppUser
//                {
//                    Id = u.Id.ToString(),
//                    Role = u.Role,
//                    Name = u.Name,
//                    Email = u.Email,
//                    Avatar = u.Avatar
//                };

//                user.PasswordHash = passwordHasher.HashPassword(user, u.Password);

//                context.Users.Add(user);
//            }

//            context.SaveChanges();
//        }
//        #endregion

//        #region Carts
//        private static void SeedCarts(AppDbContext context)
//        {
//            if (context.Carts.Any())
//                return;

//            var jsonPath = Path.Combine(
//                AppContext.BaseDirectory,
//                "Infrastructure",
//                "Data",
//                "SeedData",
//                "JsonFiles",
//                "carts.json"
//            );

//            var json = ReadJsonFile("carts.json");

//            var carts = JsonSerializer.Deserialize<List<CartSeedDto>>(
//             json,
//             new JsonSerializerOptions
//             {
//                 PropertyNameCaseInsensitive = true
//             });

//            if (carts is null)
//                return;

//            foreach (var c in carts)
//            {
//                var cart = new Cart
//                {
//                    Id = c.Id,
//                    UserId = c.UserId
//                };

//                context.Carts.Add(cart);
//            }

//            context.SaveChanges();
//        }
//        #endregion

//        #region CartItems
//        private static void SeedCartItems(AppDbContext context)
//        {
//            if (context.CartItems.Any())
//                return;

//            var jsonPath = Path.Combine(
//                 AppContext.BaseDirectory,
//                 "Infrastructure",
//                 "Data",
//                 "SeedData",
//                 "JsonFiles",
//                 "cartItems.json"
//             );

//            var json = ReadJsonFile("cartItems.json");

//            var cartItems = JsonSerializer.Deserialize<List<CartItemSeedDto>>(
//                   json,
//                   new JsonSerializerOptions
//                   {
//                       PropertyNameCaseInsensitive = true
//                   });

//            if (cartItems is null)
//                return;

//            foreach (var c in cartItems)
//            {
//                var cartItem = new CartItem
//                {
//                    Id = c.Id,
//                    CartId = c.CartId,
//                    ProductId = c.ProductId,
//                    Quantity = c.Quantity
//                };

//                context.CartItems.Add(cartItem);
//            }
//            context.SaveChanges();
//        }
//        #endregion

//        #region Messages
//        private static void SeedMessages(AppDbContext context)
//        {
//            if (context.Messages.Any())
//                return;

//            var jsonPath = Path.Combine(
//                 AppContext.BaseDirectory,
//                 "Infrastructure",
//                 "Data",
//                 "SeedData",
//                 "JsonFiles",
//                 "messages.json"
//             );

//            var json = ReadJsonFile("messages.json");

//            var messages = JsonSerializer.Deserialize<List<MessageSeedDto>>(
//                json,
//                new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//            if (messages is null)
//                return;

//            foreach (var m in messages)
//            {
//                var message = new Message
//                {
//                    Id = m.Id,
//                    Text = m.Text,
//                    IsRead = m.IsRead,
//                    SenderId = m.SenderId,
//                    ConversationId = m.ConversationId,

//                    SentAt = DateTime.Parse(m.SentAt)
//                };

//                context.Messages.Add(message);
//            }
//            context.SaveChanges();
//        }
//        #endregion

//        #region Conversations
//        private static void SeedConversations(AppDbContext context)
//        {
//            if (context.Conversations.Any())
//                return;

//            var jsonPath = Path.Combine(
//                 AppContext.BaseDirectory,
//                 "Infrastructure",
//                 "Data",
//                 "SeedData",
//                 "JsonFiles",
//                 "conversations.json"
//             );

//            var json = ReadJsonFile("conversations.json");

//            var conversations = JsonSerializer.Deserialize<List<ConversationSeedDto>>(json);

//            if (conversations is null)
//                return;

//            foreach (var c in conversations)
//            {
//                var conversation = new Conversation
//                {
//                    Id = c.Id,
//                    BuyerId = c.BuyerId,
//                    SellerId = c.SellerId
//                };

//                context.Conversations.Add(conversation);
//            }
//            context.SaveChanges();
//        }
//        #endregion

//        #region WishlistItems
//        private static void SeedWishlistItems(AppDbContext context)
//        {
//            if (context.WishlistItems.Any())
//                return;

//            var json = ReadJsonFile("users.json");
//            var users = JsonSerializer.Deserialize<List<UserSeedDto>>(json, new JsonSerializerOptions
//            {
//                PropertyNameCaseInsensitive = true,
//                Converters = { new JsonStringEnumConverter() }
//            });

//            if (users is null) return;

//            foreach (var u in users)
//            {
//                if (u.Wishlist == null || !u.Wishlist.Any())
//                    continue;

//                var wishlist = context.Wishlists.FirstOrDefault(w => w.UserId == u.Id.ToString());
//                if (wishlist is null) continue;

//                foreach (var productId in u.Wishlist)
//                {
//                    context.WishlistItems.Add(new WishlistItem
//                    {
//                        Wishlist = wishlist,
//                        ProductId = productId
//                    });
//                }
//            }

//            context.SaveChanges();
//        }
//        #endregion
//    }
//}