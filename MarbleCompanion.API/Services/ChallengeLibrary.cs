using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Services;

public static class ChallengeLibrary
{
    public record ChallengeItem(
        string Title,
        string Description,
        ChallengeType Type,
        ChallengeDifficulty Difficulty,
        ActionCategory Category,
        string MetricKey,
        int GoalValue,
        int DurationDays,
        int LPReward,
        string? SeasonTag);

    public static readonly List<ChallengeItem> Items =
    [
        // ── Transport ────────────────────────────────────────────────
        new("Car-Free Week", "Avoid using a personal car for 7 days", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Transport, "car_free_days", 7, 7, LPAwards.ChallengeMedium, null),
        new("Bike Month", "Bike to work or for errands at least 20 days this month", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Transport, "bike_trips", 20, 30, LPAwards.ChallengeHard, null),
        new("Public Transit Challenge", "Take public transit 10 times this week", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Transport, "transit_trips", 10, 7, LPAwards.ChallengeEasy, null),
        new("Walking Challenge", "Walk 50 km total this month", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Transport, "walk_km", 50, 30, LPAwards.ChallengeMedium, null),
        new("Carpool Week", "Carpool at least 5 times this week", ChallengeType.Group, ChallengeDifficulty.Easy, ActionCategory.Transport, "carpool_trips", 5, 7, LPAwards.ChallengeEasy, null),
        new("Zero Emissions Commute", "Commute without fossil fuels for 14 days", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Transport, "zero_emission_days", 14, 14, LPAwards.ChallengeHard, null),
        new("Spring Cycling Sprint", "Log 15 bike trips this spring", ChallengeType.Competitive, ChallengeDifficulty.Medium, ActionCategory.Transport, "bike_trips", 15, 30, LPAwards.ChallengeMedium, "spring"),

        // ── Food ─────────────────────────────────────────────────────
        new("Meatless Week", "Eat no meat for 7 consecutive days", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Food, "meatless_days", 7, 7, LPAwards.ChallengeMedium, null),
        new("Vegan January", "Eat fully plant-based for the month of January", ChallengeType.Group, ChallengeDifficulty.Hard, ActionCategory.Food, "vegan_days", 31, 31, LPAwards.ChallengeHard, "winter"),
        new("Local Food Week", "Buy all produce locally for a week", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Food, "local_meals", 21, 7, LPAwards.ChallengeEasy, null),
        new("Zero Food Waste", "Produce no food waste for 5 consecutive days", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Food, "zero_waste_days", 5, 7, LPAwards.ChallengeMedium, null),
        new("Cook at Home Month", "Prepare 60 homemade meals this month", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Food, "homemade_meals", 60, 30, LPAwards.ChallengeHard, null),
        new("Seasonal Eating Week", "Eat only seasonal produce for 7 days", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Food, "seasonal_days", 7, 7, LPAwards.ChallengeEasy, "autumn"),
        new("Harvest Feast Challenge", "Cook 10 meals using seasonal fall produce", ChallengeType.Group, ChallengeDifficulty.Medium, ActionCategory.Food, "seasonal_meals", 10, 14, LPAwards.ChallengeMedium, "autumn"),

        // ── Energy ───────────────────────────────────────────────────
        new("Energy Saver Week", "Reduce daily energy consumption for 7 days", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Energy, "energy_actions", 7, 7, LPAwards.ChallengeEasy, null),
        new("Cold Wash Month", "Wash all laundry in cold water for a month", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Energy, "cold_washes", 12, 30, LPAwards.ChallengeEasy, null),
        new("No AC Challenge", "Avoid air conditioning for 14 days", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Energy, "no_ac_days", 14, 14, LPAwards.ChallengeHard, "summer"),
        new("Unplug Everything", "Unplug all standby devices daily for 2 weeks", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Energy, "unplug_days", 14, 14, LPAwards.ChallengeMedium, null),
        new("Shorter Showers Sprint", "Take only 5-minute showers for 2 weeks", ChallengeType.Competitive, ChallengeDifficulty.Medium, ActionCategory.Energy, "short_showers", 14, 14, LPAwards.ChallengeMedium, null),
        new("Dark Hour", "Turn off all non-essential lights for 1 hour each evening for a week", ChallengeType.Group, ChallengeDifficulty.Easy, ActionCategory.Energy, "dark_hours", 7, 7, LPAwards.ChallengeEasy, null),
        new("Summer Efficiency Challenge", "Log 20 energy-saving actions this summer", ChallengeType.Competitive, ChallengeDifficulty.Medium, ActionCategory.Energy, "energy_actions", 20, 30, LPAwards.ChallengeMedium, "summer"),

        // ── Shopping ─────────────────────────────────────────────────
        new("No New Clothes Month", "Buy zero new clothing items for a month", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Shopping, "no_new_clothes_days", 30, 30, LPAwards.ChallengeMedium, null),
        new("Secondhand Week", "Buy only secondhand items for a week", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Shopping, "secondhand_purchases", 3, 7, LPAwards.ChallengeEasy, null),
        new("Repair Challenge", "Repair 5 items instead of replacing them", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Shopping, "repairs", 5, 30, LPAwards.ChallengeMedium, null),
        new("Buy Nothing Week", "Make no non-essential purchases for 7 days", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Shopping, "no_buy_days", 7, 7, LPAwards.ChallengeHard, null),
        new("Reusable Revolution", "Use reusable bags/bottles 30 times", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Shopping, "reusable_uses", 30, 30, LPAwards.ChallengeEasy, null),
        new("Local Business Week", "Support 5 local businesses this week", ChallengeType.Group, ChallengeDifficulty.Easy, ActionCategory.Shopping, "local_purchases", 5, 7, LPAwards.ChallengeEasy, null),

        // ── Travel ───────────────────────────────────────────────────
        new("Staycation Challenge", "Have a vacation without flying for 2 weeks", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Travel, "staycation_days", 14, 14, LPAwards.ChallengeMedium, "summer"),
        new("Eco Tourism Week", "Stay at eco-certified accommodations for a trip", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Travel, "eco_stays", 3, 14, LPAwards.ChallengeEasy, null),
        new("Train Traveler", "Take the train instead of flying for 3 trips", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Travel, "train_trips", 3, 90, LPAwards.ChallengeHard, null),
        new("Walking Explorer", "Explore a new area entirely on foot", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Travel, "walking_tours", 3, 30, LPAwards.ChallengeEasy, null),
        new("Carbon-Neutral Trip", "Offset all emissions from your next trip", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Travel, "offset_trips", 1, 30, LPAwards.ChallengeMedium, null),

        // ── Waste ────────────────────────────────────────────────────
        new("Zero Waste Week", "Produce no landfill waste for 7 days", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Waste, "zero_waste_days", 7, 7, LPAwards.ChallengeHard, null),
        new("Compost Starter", "Compost daily for 14 days straight", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Waste, "compost_days", 14, 14, LPAwards.ChallengeMedium, null),
        new("Plastic-Free Week", "Avoid all single-use plastics for 7 days", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Waste, "plastic_free_days", 7, 7, LPAwards.ChallengeMedium, null),
        new("Recycling Champion", "Recycle properly every day for a month", ChallengeType.Individual, ChallengeDifficulty.Easy, ActionCategory.Waste, "recycle_days", 30, 30, LPAwards.ChallengeEasy, null),
        new("Declutter & Donate", "Donate 10 bags of items to charity", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Waste, "donation_bags", 10, 30, LPAwards.ChallengeMedium, null),
        new("Earth Day Cleanup", "Participate in a local cleanup event during Earth week", ChallengeType.Group, ChallengeDifficulty.Easy, ActionCategory.Waste, "cleanup_events", 1, 7, LPAwards.ChallengeEasy, "spring"),
        new("E-Waste Roundup", "Recycle 5 electronic devices this month", ChallengeType.Individual, ChallengeDifficulty.Medium, ActionCategory.Waste, "ewaste_items", 5, 30, LPAwards.ChallengeMedium, null),
        new("Upcycle Artist", "Complete 3 upcycling projects", ChallengeType.Individual, ChallengeDifficulty.Hard, ActionCategory.Waste, "upcycle_projects", 3, 30, LPAwards.ChallengeHard, null),

        // ── Cross-category / Special ─────────────────────────────────
        new("New Year Green Resolution", "Log 50 eco-actions in January", ChallengeType.Competitive, ChallengeDifficulty.Hard, ActionCategory.Energy, "total_actions", 50, 31, LPAwards.ChallengeHard, "winter"),
        new("Earth Month Challenge", "Complete 30 eco-actions during April", ChallengeType.Group, ChallengeDifficulty.Medium, ActionCategory.Waste, "total_actions", 30, 30, LPAwards.ChallengeMedium, "spring"),
    ];
}
