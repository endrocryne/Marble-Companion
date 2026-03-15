using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class EmissionFactorSeedService
{
    private readonly AppDbContext _db;

    public EmissionFactorSeedService(AppDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        if (await _db.EmissionFactors.AnyAsync())
            return;

        _db.EmissionFactors.AddRange(GetFactors());
        await _db.SaveChangesAsync();
    }

    private static List<EmissionFactor> GetFactors() =>
    [
        // ── Transport ────────────────────────────────────────────────
        new() { Category = "Transport", ActionKey = "bike_commute", FactorKgCO2ePerUnit = 4.6m, Unit = "trip", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "walk_commute", FactorKgCO2ePerUnit = 4.6m, Unit = "trip", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "public_transit", FactorKgCO2ePerUnit = 2.4m, Unit = "trip", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "carpool", FactorKgCO2ePerUnit = 2.3m, Unit = "trip", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "ev_charge", FactorKgCO2ePerUnit = 1.8m, Unit = "charge", Source = "ICCT", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "work_from_home", FactorKgCO2ePerUnit = 3.2m, Unit = "day", Source = "IEA Transport", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "train_trip", FactorKgCO2ePerUnit = 5.1m, Unit = "trip", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "e_scooter", FactorKgCO2ePerUnit = 3.8m, Unit = "trip", Source = "IEA Transport", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "avoid_short_flight", FactorKgCO2ePerUnit = 150.0m, Unit = "flight", Source = "ICAO Carbon Calculator", SourceYear = 2023 },
        new() { Category = "Transport", ActionKey = "tire_pressure_check", FactorKgCO2ePerUnit = 0.5m, Unit = "check", Source = "EPA Fuel Economy", SourceYear = 2023 },

        // ── Food ─────────────────────────────────────────────────────
        new() { Category = "Food", ActionKey = "vegan_meal", FactorKgCO2ePerUnit = 2.5m, Unit = "meal", Source = "Poore & Nemecek, Science", SourceYear = 2018 },
        new() { Category = "Food", ActionKey = "vegetarian_meal", FactorKgCO2ePerUnit = 1.5m, Unit = "meal", Source = "Poore & Nemecek, Science", SourceYear = 2018 },
        new() { Category = "Food", ActionKey = "skip_beef", FactorKgCO2ePerUnit = 6.6m, Unit = "serving", Source = "IPCC AR6", SourceYear = 2022 },
        new() { Category = "Food", ActionKey = "local_produce", FactorKgCO2ePerUnit = 0.8m, Unit = "meal", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Food", ActionKey = "reduce_food_waste", FactorKgCO2ePerUnit = 0.9m, Unit = "day", Source = "FAO", SourceYear = 2023 },
        new() { Category = "Food", ActionKey = "homemade_meal", FactorKgCO2ePerUnit = 1.2m, Unit = "meal", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Food", ActionKey = "skip_dairy", FactorKgCO2ePerUnit = 1.4m, Unit = "serving", Source = "Poore & Nemecek, Science", SourceYear = 2018 },
        new() { Category = "Food", ActionKey = "seasonal_eating", FactorKgCO2ePerUnit = 0.6m, Unit = "meal", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Food", ActionKey = "grow_own_food", FactorKgCO2ePerUnit = 0.4m, Unit = "harvest", Source = "Project Drawdown", SourceYear = 2023 },

        // ── Energy ───────────────────────────────────────────────────
        new() { Category = "Energy", ActionKey = "thermostat_adjust", FactorKgCO2ePerUnit = 0.8m, Unit = "day", Source = "EPA ENERGY STAR", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "led_switch", FactorKgCO2ePerUnit = 0.1m, Unit = "bulb-day", Source = "DOE Energy Efficiency", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "cold_wash", FactorKgCO2ePerUnit = 0.6m, Unit = "load", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "line_dry", FactorKgCO2ePerUnit = 2.4m, Unit = "load", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "shorter_shower", FactorKgCO2ePerUnit = 0.7m, Unit = "shower", Source = "EPA WaterSense", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "unplug_devices", FactorKgCO2ePerUnit = 0.3m, Unit = "day", Source = "DOE Building Technologies", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "green_energy_switch", FactorKgCO2ePerUnit = 5.0m, Unit = "month", Source = "EPA eGRID", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "smart_power_strip", FactorKgCO2ePerUnit = 0.2m, Unit = "day", Source = "DOE Energy Efficiency", SourceYear = 2023 },
        new() { Category = "Energy", ActionKey = "natural_ventilation", FactorKgCO2ePerUnit = 1.5m, Unit = "day", Source = "IPCC AR6", SourceYear = 2022 },

        // ── Shopping ─────────────────────────────────────────────────
        new() { Category = "Shopping", ActionKey = "secondhand_clothing", FactorKgCO2ePerUnit = 25.0m, Unit = "garment", Source = "UNEP Fashion Report", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "reusable_bag", FactorKgCO2ePerUnit = 0.03m, Unit = "use", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "reusable_bottle", FactorKgCO2ePerUnit = 0.08m, Unit = "use", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "repair_item", FactorKgCO2ePerUnit = 10.0m, Unit = "item", Source = "European Environment Agency", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "buy_local", FactorKgCO2ePerUnit = 1.5m, Unit = "purchase", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "digital_receipt", FactorKgCO2ePerUnit = 0.01m, Unit = "receipt", Source = "EPA", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "bulk_buying", FactorKgCO2ePerUnit = 0.5m, Unit = "trip", Source = "Zero Waste International", SourceYear = 2023 },
        new() { Category = "Shopping", ActionKey = "borrow_not_buy", FactorKgCO2ePerUnit = 5.0m, Unit = "item", Source = "Ellen MacArthur Foundation", SourceYear = 2023 },

        // ── Travel ───────────────────────────────────────────────────
        new() { Category = "Travel", ActionKey = "offset_flight", FactorKgCO2ePerUnit = 200.0m, Unit = "flight", Source = "Gold Standard Foundation", SourceYear = 2023 },
        new() { Category = "Travel", ActionKey = "eco_hotel", FactorKgCO2ePerUnit = 8.0m, Unit = "night", Source = "DEFRA GHG Reporting", SourceYear = 2023 },
        new() { Category = "Travel", ActionKey = "staycation", FactorKgCO2ePerUnit = 50.0m, Unit = "trip", Source = "ICAO Carbon Calculator", SourceYear = 2023 },
        new() { Category = "Travel", ActionKey = "direct_flight", FactorKgCO2ePerUnit = 40.0m, Unit = "flight", Source = "ICAO Carbon Calculator", SourceYear = 2023 },
        new() { Category = "Travel", ActionKey = "train_over_plane", FactorKgCO2ePerUnit = 80.0m, Unit = "trip", Source = "IEA Transport", SourceYear = 2023 },
        new() { Category = "Travel", ActionKey = "pack_light", FactorKgCO2ePerUnit = 5.0m, Unit = "flight", Source = "IATA", SourceYear = 2023 },

        // ── Waste ────────────────────────────────────────────────────
        new() { Category = "Waste", ActionKey = "recycle", FactorKgCO2ePerUnit = 0.5m, Unit = "kg", Source = "EPA WARM Model", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "compost", FactorKgCO2ePerUnit = 0.5m, Unit = "kg", Source = "EPA Composting Research", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "refuse_single_use", FactorKgCO2ePerUnit = 0.05m, Unit = "item", Source = "UNEP Plastics Report", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "e_waste_recycle", FactorKgCO2ePerUnit = 2.0m, Unit = "device", Source = "EPA E-Waste", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "donate_items", FactorKgCO2ePerUnit = 3.0m, Unit = "bag", Source = "EPA WARM Model", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "paperless_billing", FactorKgCO2ePerUnit = 0.02m, Unit = "bill", Source = "EPA", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "upcycle_project", FactorKgCO2ePerUnit = 2.5m, Unit = "project", Source = "Ellen MacArthur Foundation", SourceYear = 2023 },
        new() { Category = "Waste", ActionKey = "cloth_napkins", FactorKgCO2ePerUnit = 0.03m, Unit = "use", Source = "EPA GHG Equivalencies", SourceYear = 2023 },
    ];
}
