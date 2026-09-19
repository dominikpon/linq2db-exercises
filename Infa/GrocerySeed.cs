using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public static class GrocerySeed
{
    public static void EnsureSeeded(GroceryDatabase db)
    {
        db.CreateTable<GroceryItem>(tableOptions: TableOptions.CreateIfNotExists);
        if (db.Groceries().Any()) return;
        db.BulkCopy(Build());
    }

    public static Guid IdOf(int index)
    {
        return Guid.Parse($"{index:00000000}-0000-4000-8000-000000000000");
    }

    public static GroceryItem[] Build()
    {
        var today = DateTime.UtcNow.Date;
        var id = 0;

        GroceryItem Item(string name, string? brand, string category, string? tags, string? barcode,
            decimal price, decimal? discount, int stock, int purchased, double weight, double? rating,
            bool organic, bool discontinued, StorageType storage, Supplier supplier,
            int createdDaysAgo, int? purchasedDaysAgo, int? bestBeforeInDays, int? prepMinutes)
        {
            return new GroceryItem
            {
                Id = IdOf(++id),
                Name = name,
                Brand = brand,
                Category = category,
                Tags = tags,
                Barcode = barcode,
                PriceDkk = price,
                DiscountPercent = discount,
                StockCount = stock,
                TimesPurchased = purchased,
                WeightKg = weight,
                RatingAvg = rating,
                IsOrganic = organic,
                IsDiscontinued = discontinued,
                Storage = storage,
                SuppliedBy = supplier,
                CreatedAtUtc = today.AddDays(-createdDaysAgo).AddHours(9),
                LastPurchasedAtUtc =
                    purchasedDaysAgo is null ? null : today.AddDays(-purchasedDaysAgo.Value).AddHours(17),
                BestBefore = bestBeforeInDays is null
                    ? null
                    : DateOnly.FromDateTime(today.AddDays(bestBeforeInDays.Value)),
                PreparationTime = prepMinutes is null ? null : TimeSpan.FromMinutes(prepMinutes.Value)
            };
        }

        return
        [
            Item("Whole Milk 1L", "Arla", "Dairy", "milk;fresh;breakfast", "5701234000011", 12.50m, null, 120, 340,
                1.03, 4.2, false, false, StorageType.Chilled, Supplier.Wholesale, 400, 1, 5, null),
            Item("Organic Whole Milk 1L", "Thise", "Dairy", "milk;organic;fresh", "5701234000028", 16.95m, 10m, 40, 210,
                1.03, 4.6, true, false, StorageType.Chilled, Supplier.LocalFarm, 380, 2, 4, null),
            Item("Salted Butter 250g", "Lurpak", "Dairy", "butter;baking", "5701234000035", 24.00m, null, 65, 190, 0.25,
                4.8, false, false, StorageType.Chilled, Supplier.Wholesale, 500, 3, 60, null),
            Item("Havarti Cheese 400g", "Arla", "Dairy", "cheese;sandwich", "5701234000042", 39.50m, 15m, 0, 88, 0.40,
                4.0, false, false, StorageType.Chilled, Supplier.Wholesale, 300, 12, -2, null),
            Item("Greek Yoghurt 500g", null, "Dairy", "yoghurt;protein;breakfast", null, 21.75m, null, 18, 61, 0.50,
                3.9, true, false, StorageType.Chilled, Supplier.Import, 210, 8, 9, null),

            Item("Rye Bread", "Schulstad", "Bakery", "bread;wholegrain", "5701234000059", 18.95m, null, 33, 402, 0.95,
                4.1, false, false, StorageType.Ambient, Supplier.Wholesale, 620, 0, 6, null),
            Item("Sourdough Loaf", "Bakerman", "Bakery", "bread;artisan;organic", "5701234000066", 34.00m, 20m, 7, 55,
                0.80, 4.7, true, false, StorageType.Ambient, Supplier.LocalFarm, 120, 4, 3, null),
            Item("Cinnamon Roll", "Bakerman", "Bakery", "sweet;snack", null, 9.50m, null, 0, 733, 0.11, 4.4, false,
                true, StorageType.Ambient, Supplier.LocalFarm, 900, 40, -5, null),

            Item("Bananas 1kg", null, "Produce", "fruit;snack;lunchbox", "5701234000073", 14.95m, null, 210, 1204, 1.00,
                3.8, false, false, StorageType.Ambient, Supplier.Import, 700, 0, 7, null),
            Item("Organic Carrots 1kg", null, "Produce", "vegetable;organic;root", "5701234000080", 11.50m, 5m, 88, 512,
                1.00, 4.3, true, false, StorageType.Ambient, Supplier.LocalFarm, 650, 1, 18, null),
            Item("Cherry Tomatoes 250g", null, "Produce", "vegetable;salad", null, 17.95m, null, 24, 143, 0.25, 4.5,
                false, false, StorageType.Chilled, Supplier.Import, 190, 2, 4, null),
            Item("Avocado", null, "Produce", "fruit;lunch", "5701234000097", 8.95m, 25m, 150, 980, 0.20, 3.5, false,
                false, StorageType.Ambient, Supplier.Import, 540, 0, 2, null),
            Item("Baby Spinach 175g", "Grøn Mark", "Produce", "vegetable;salad;organic", null, 19.50m, null, 12, 0,
                0.175, 4.1, true, false, StorageType.Chilled, Supplier.LocalFarm, 95, null, 1, null),

            Item("Chicken Breast 700g", "Danpo", "Meat", "chicken;protein;dinner", "5701234000103", 62.00m, null, 45,
                388, 0.70, 4.2, false, false, StorageType.Chilled, Supplier.Wholesale, 480, 1, 3, 25),
            Item("Minced Beef 8% 500g", null, "Meat", "beef;protein;dinner", "5701234000110", 48.95m, 10m, 60, 611,
                0.50, 4.0, false, false, StorageType.Chilled, Supplier.Wholesale, 520, 0, 2, 20),
            Item("Organic Pork Chops 600g", "Friland", "Meat", "pork;organic;dinner", null, 79.00m, null, 9, 74, 0.60,
                4.6, true, false, StorageType.Chilled, Supplier.LocalFarm, 260, 15, 6, 30),
            Item("Salmon Fillet 300g", null, "Meat", "fish;protein;omega3", "5701234000127", 89.50m, 30m, 6, 129, 0.30,
                4.9, false, false, StorageType.Frozen, Supplier.Import, 340, 9, 240, 18),

            Item("Frozen Peas 800g", "Findus", "Frozen", "vegetable;freezer;side", "5701234000134", 22.95m, null, 130,
                265, 0.80, 3.7, false, false, StorageType.Frozen, Supplier.Wholesale, 730, 20, 400, 6),
            Item("Pepperoni Pizza", "Dr. Oetker", "Frozen", "dinner;quick;freezer", "5701234000141", 32.50m, 15m, 74,
                845, 0.40, 3.4, false, false, StorageType.Frozen, Supplier.Import, 810, 3, 300, 14),
            Item("Vanilla Ice Cream 1L", "Hansen", "Frozen", "dessert;sweet;freezer", null, 44.00m, null, 21, 302, 1.00,
                4.5, true, false, StorageType.Frozen, Supplier.LocalFarm, 660, 30, 500, null),

            Item("Spaghetti 500g", "Barilla", "Pantry", "pasta;dinner;dry", "5701234000158", 13.95m, null, 300, 1520,
                0.50, 4.3, false, false, StorageType.Ambient, Supplier.Import, 950, 5, 700, 11),
            Item("Chopped Tomatoes 400g", "Mutti", "Pantry", "sauce;dinner;canned", "5701234000165", 9.95m, 5m, 420,
                1811, 0.40, 4.6, false, false, StorageType.Ambient, Supplier.Import, 980, 2, 900, null),
            Item("Olive Oil 500ml", "Zeta", "Pantry", "oil;cooking", "5701234000172", 74.50m, null, 55, 233, 0.46, 4.4,
                true, false, StorageType.Ambient, Supplier.Import, 870, 25, 600, null),
            Item("Basmati Rice 1kg", null, "Pantry", "rice;dinner;dry", null, 26.00m, null, 0, 402, 1.00, 3.9, false,
                true, StorageType.Ambient, Supplier.Import, 1000, 120, 800, 15),
            Item("Instant Coffee 200g", "Merrild", "Pantry", "coffee;breakfast;hot", "5701234000189", 59.95m, 20m, 88,
                940, 0.20, 3.6, false, false, StorageType.Ambient, Supplier.Wholesale, 1010, 1, 950, 2),

            Item("Sparkling Water 6x0.5L", "Ramlösa", "Drinks", "water;drink", "5701234000196", 29.95m, null, 190, 700,
                3.00, 4.0, false, false, StorageType.Ambient, Supplier.Wholesale, 760, 4, 1000, null),
            Item("Orange Juice 1L", "Rynkeby", "Drinks", "juice;breakfast;drink", "5701234000202", 23.50m, 10m, 47, 566,
                1.05, 4.1, false, false, StorageType.Chilled, Supplier.Wholesale, 690, 1, 12, null),
            Item("Craft IPA 0.33L", "Mikkeller", "Drinks", "beer;alcohol;weekend", null, 34.95m, null, 3, 88, 0.35, 4.7,
                false, false, StorageType.Chilled, Supplier.LocalFarm, 150, 60, 365, null),
            Item("Energy Drink 0.25L", "RedBull", "Drinks", "drink;caffeine", "5701234000219", 18.00m, null, 0, 1450,
                0.25, 2.9, false, true, StorageType.Ambient, Supplier.Import, 1100, 200, -10, null),

            Item("Dish Soap 500ml", "Neutral", "Household", "cleaning;kitchen", "5701234000226", 27.95m, null, 64, 121,
                0.50, 4.2, false, false, StorageType.Ambient, Supplier.Wholesale, 820, 45, null, null),
            Item("Toilet Paper 8-pack", "Lambi", "Household", "paper;bathroom;bulk", "5701234000233", 49.95m, 15m, 110,
                380, 2.40, 4.5, false, false, StorageType.Ambient, Supplier.Wholesale, 830, 7, null, null),
            Item("Tea Light Candles 50pcs", null, "Household", "candles;hygge", null, 39.00m, null, 5, 0, 1.80, 3.8,
                false, false, StorageType.Ambient, Supplier.Import, 430, null, null, null)
        ];
    }
}