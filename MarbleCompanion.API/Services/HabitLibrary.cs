using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Services;

public static class HabitLibrary
{
    public record HabitItem(
        Guid Id,
        string Name,
        string Description,
        ActionCategory Category,
        HabitFrequency Frequency,
        int LPReward,
        double EstimatedCO2ePerAction,
        string EffortLevel);

    public static readonly List<HabitItem> Items =
    [
        // ── Transport (15) ──────────────────────────────────────────
        new(Guid.Parse("10000000-0001-0000-0000-000000000001"), "Bike to Work", "Cycle instead of driving to work", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 4.6, "Medium"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000002"), "Walk to Work", "Walk instead of driving to work", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 4.6, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000003"), "Take Public Transit", "Use bus, tram, or subway for commuting", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 2.4, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000004"), "Carpool to Work", "Share a ride with a colleague", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 2.3, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000005"), "Work from Home", "Skip the commute by working remotely", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 3.2, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000006"), "Use Electric Scooter", "Use an e-scooter for short trips", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 3.8, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000007"), "Check Tire Pressure", "Maintain optimal tire pressure for fuel efficiency", ActionCategory.Transport, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.5, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000008"), "Combine Errands", "Plan trips to minimize driving", ActionCategory.Transport, HabitFrequency.Weekly, LPAwards.HabitCheckin, 3.0, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000009"), "Walk for Short Trips", "Walk instead of drive for trips under 2 km", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.5, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000010"), "No-Drive Day", "Avoid using a car for the entire day", ActionCategory.Transport, HabitFrequency.Weekly, LPAwards.HabitCheckin, 8.0, "Medium"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000011"), "Eco-Driving", "Practice fuel-efficient driving techniques", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.0, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000012"), "Charge EV Off-Peak", "Charge electric vehicle during off-peak hours", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.8, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000013"), "Take the Stairs", "Skip elevators and escalators", ActionCategory.Transport, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.1, "Low"),
        new(Guid.Parse("10000000-0001-0000-0000-000000000014"), "Bike for Errands", "Use a bicycle for running errands", ActionCategory.Transport, HabitFrequency.Weekly, LPAwards.HabitCheckin, 3.5, "Medium"),

        // ── Food (15) ────────────────────────────────────────────────
        new(Guid.Parse("10000000-0002-0000-0000-000000000001"), "Eat a Vegan Meal", "Have a fully plant-based meal", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 2.5, "Medium"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000002"), "Eat a Vegetarian Meal", "Have a meat-free meal", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.5, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000003"), "Skip Beef Today", "Avoid beef for the day", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 6.6, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000004"), "Buy Local Produce", "Shop at farmers markets or buy local", ActionCategory.Food, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.8, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000005"), "Reduce Food Waste", "Use leftovers and plan meals to avoid waste", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.9, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000006"), "Cook at Home", "Prepare a homemade meal instead of ordering", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.2, "Medium"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000007"), "Skip Dairy", "Choose plant-based dairy alternatives", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.4, "Medium"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000008"), "Eat Seasonal Food", "Choose fruits and vegetables that are in season", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.6, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000009"), "Grow Your Own Food", "Harvest something from your garden", ActionCategory.Food, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.4, "High"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000010"), "Meatless Monday", "Go fully meat-free on Mondays", ActionCategory.Food, HabitFrequency.Weekly, LPAwards.HabitCheckin, 4.0, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000011"), "Bring Lunch to Work", "Pack a homemade lunch instead of buying", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.0, "Medium"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000012"), "Use a Reusable Coffee Cup", "Bring your own cup for coffee", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.1, "Low"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000013"), "Batch Cooking", "Cook in bulk to save energy and reduce waste", ActionCategory.Food, HabitFrequency.Weekly, LPAwards.HabitCheckin, 2.0, "Medium"),
        new(Guid.Parse("10000000-0002-0000-0000-000000000014"), "Plant-Based Snacking", "Choose plant-based snacks over processed ones", ActionCategory.Food, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.3, "Low"),

        // ── Energy (15) ──────────────────────────────────────────────
        new(Guid.Parse("10000000-0003-0000-0000-000000000001"), "Adjust Thermostat", "Lower heating by 1°C or raise cooling by 1°C", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.8, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000002"), "Cold Water Wash", "Wash clothes in cold water", ActionCategory.Energy, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.6, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000003"), "Line Dry Clothes", "Air-dry laundry instead of using dryer", ActionCategory.Energy, HabitFrequency.Weekly, LPAwards.HabitCheckin, 2.4, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000004"), "Shorter Shower", "Keep showers under 5 minutes", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.7, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000005"), "Unplug Devices", "Unplug electronics when not in use", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.3, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000006"), "Turn Off Lights", "Turn off lights when leaving a room", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.1, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000007"), "Natural Ventilation", "Open windows instead of using AC", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 1.5, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000008"), "Full Dishwasher Load", "Only run dishwasher when full", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.3, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000009"), "Use Smart Power Strip", "Use smart strips to cut standby power", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.2, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000010"), "Batch Cooking Energy", "Use oven/stove efficiently by cooking multiple dishes", ActionCategory.Energy, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.5, "Medium"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000011"), "Use Microwave Over Oven", "Use microwave for reheating instead of oven", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.3, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000012"), "Close Curtains at Night", "Keep heat in by closing curtains after sunset", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.2, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000013"), "Use Laptop Over Desktop", "Choose laptop over desktop PC to save energy", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.15, "Low"),
        new(Guid.Parse("10000000-0003-0000-0000-000000000014"), "Wear a Sweater", "Dress warmly indoors instead of turning up heat", ActionCategory.Energy, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.8, "Low"),

        // ── Shopping (13) ────────────────────────────────────────────
        new(Guid.Parse("10000000-0004-0000-0000-000000000001"), "Use Reusable Bags", "Bring reusable bags when shopping", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.03, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000002"), "Use Reusable Water Bottle", "Carry a reusable bottle instead of buying plastic", ActionCategory.Shopping, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.08, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000003"), "Buy Secondhand", "Purchase pre-owned clothing or items", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 25.0, "Medium"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000004"), "Repair Instead of Replace", "Fix broken items instead of buying new", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 10.0, "High"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000005"), "Buy Local Products", "Support local businesses and reduce transport emissions", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 1.5, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000006"), "Choose Digital Receipts", "Opt for email receipts instead of paper", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.01, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000007"), "Buy in Bulk", "Purchase staples in bulk to reduce packaging", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.5, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000008"), "Borrow Don't Buy", "Borrow tools, books, or gear instead of buying", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 5.0, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000009"), "Choose Minimal Packaging", "Select products with minimal or recyclable packaging", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.2, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000010"), "Swap Clothes with Friends", "Exchange clothes instead of buying new", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 12.0, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000011"), "Buy Eco-Certified Products", "Choose products with eco-certifications", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 1.0, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000012"), "No-Buy Day", "Avoid non-essential purchases for a full day", ActionCategory.Shopping, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.5, "Low"),
        new(Guid.Parse("10000000-0004-0000-0000-000000000013"), "Research Before Buying", "Check product sustainability ratings before purchasing", ActionCategory.Shopping, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.3, "Low"),

        // ── Travel (10) ──────────────────────────────────────────────
        new(Guid.Parse("10000000-0005-0000-0000-000000000001"), "Offset a Flight", "Purchase carbon offsets for air travel", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 200.0, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000002"), "Choose Eco Hotel", "Stay at eco-certified accommodations", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 8.0, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000003"), "Explore Local", "Choose local destinations over flying abroad", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 50.0, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000004"), "Train Over Plane", "Choose rail travel over flights for short distances", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 80.0, "Medium"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000005"), "Pack Light", "Reduce luggage weight to lower flight emissions", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 5.0, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000006"), "Reuse Hotel Towels", "Opt to reuse towels during hotel stays", ActionCategory.Travel, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.3, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000007"), "Walking Tour", "Explore destinations on foot instead of by car", ActionCategory.Travel, HabitFrequency.Daily, LPAwards.HabitCheckin, 2.0, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000008"), "Choose Direct Flights", "Book direct flights to reduce fuel from extra takeoffs", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 40.0, "Low"),
        new(Guid.Parse("10000000-0005-0000-0000-000000000009"), "Support Local Tourism", "Choose local guides and businesses when traveling", ActionCategory.Travel, HabitFrequency.Weekly, LPAwards.HabitCheckin, 1.0, "Low"),

        // ── Waste (14) ───────────────────────────────────────────────
        new(Guid.Parse("10000000-0006-0000-0000-000000000001"), "Recycle Properly", "Sort and recycle household waste", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.5, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000002"), "Compost Food Scraps", "Compost fruit/vegetable scraps and coffee grounds", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.5, "Medium"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000003"), "Refuse Single-Use Plastic", "Say no to straws, utensils, and bags", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.05, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000004"), "Go Paperless", "Opt for digital documents and e-billing", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.02, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000005"), "Donate Unused Items", "Donate clothes, electronics, or household items", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 3.0, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000006"), "Upcycle Something", "Create something new from waste materials", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 2.5, "High"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000007"), "Use Cloth Napkins", "Use reusable cloth napkins instead of paper", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.03, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000008"), "Use Reusable Containers", "Pack food in reusable containers", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.05, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000009"), "Recycle E-Waste", "Take old electronics to recycling centers", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 2.0, "Medium"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000010"), "Use Reusable Produce Bags", "Bring mesh bags for fruits and vegetables", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.02, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000011"), "Pick Up Litter", "Collect and properly dispose of litter in your area", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.1, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000012"), "Use Beeswax Wraps", "Replace plastic wrap with beeswax wraps", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.02, "Low"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000013"), "DIY Cleaning Products", "Make cleaning products from natural ingredients", ActionCategory.Waste, HabitFrequency.Weekly, LPAwards.HabitCheckin, 0.5, "Medium"),
        new(Guid.Parse("10000000-0006-0000-0000-000000000014"), "Zero-Waste Bathroom", "Use bar soap, bamboo toothbrush, safety razor", ActionCategory.Waste, HabitFrequency.Daily, LPAwards.HabitCheckin, 0.1, "Medium"),
    ];
}
