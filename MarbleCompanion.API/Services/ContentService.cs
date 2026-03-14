using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class ContentService : IContentService
{
    private readonly AppDbContext _db;
    private readonly ITreeService _treeService;

    public ContentService(AppDbContext db, ITreeService treeService)
    {
        _db = db;
        _treeService = treeService;
    }

    public async Task<List<ContentSummaryDto>> GetAllAsync()
    {
        var contents = await _db.Contents.OrderByDescending(c => c.PublishedAt).ToListAsync();
        return contents.Select(c => new ContentSummaryDto
        {
            Id = c.Id,
            Title = c.Title,
            Summary = c.BodyMarkdown.Length > 200 ? c.BodyMarkdown[..200] + "..." : c.BodyMarkdown,
            Category = Enum.TryParse<ActionCategory>(c.Category, true, out var cat) ? cat : ActionCategory.Energy,
            Difficulty = c.Difficulty,
            IsRead = false
        }).ToList();
    }

    public async Task<ContentDto> GetByIdAsync(Guid id, string userId)
    {
        var content = await _db.Contents.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Content not found.");

        // Track read and award LP on first read
        var existingRead = await _db.UserContentReads
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ContentId == id);

        if (existingRead == null)
        {
            var read = new UserContentRead
            {
                UserId = userId,
                ContentId = id,
                ReadAt = DateTime.UtcNow,
                LeafPointsAwarded = true
            };
            _db.UserContentReads.Add(read);
            await _db.SaveChangesAsync();

            await _treeService.AwardLeafPointsAsync(userId, LPAwards.ArticleRead);
        }

        return new ContentDto
        {
            Id = content.Id,
            Title = content.Title,
            Body = content.BodyMarkdown,
            Summary = content.BodyMarkdown.Length > 200 ? content.BodyMarkdown[..200] + "..." : content.BodyMarkdown,
            Category = Enum.TryParse<ActionCategory>(content.Category, true, out var cat) ? cat : ActionCategory.Energy,
            Difficulty = content.Difficulty,
            LPReward = LPAwards.ArticleRead,
            PublishedAt = content.PublishedAt
        };
    }

    public async Task BookmarkAsync(string userId, Guid contentId)
    {
        var content = await _db.Contents.FirstOrDefaultAsync(c => c.Id == contentId)
            ?? throw new KeyNotFoundException("Content not found.");

        var existing = await _db.UserContentBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ContentId == contentId);

        if (existing != null)
        {
            _db.UserContentBookmarks.Remove(existing);
        }
        else
        {
            _db.UserContentBookmarks.Add(new UserContentBookmark
            {
                UserId = userId,
                ContentId = contentId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<ContentDto> CreateAsync(CreateContentDto dto)
    {
        var content = new Content
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            BodyMarkdown = dto.Body,
            Category = dto.Category.ToString(),
            Source = "Admin",
            PublishedAt = DateTime.UtcNow,
            ReadingTimeMinutes = Math.Max(1, dto.Body.Split(' ').Length / 200),
            Difficulty = dto.Difficulty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Contents.Add(content);
        await _db.SaveChangesAsync();

        return new ContentDto
        {
            Id = content.Id,
            Title = content.Title,
            Body = content.BodyMarkdown,
            Summary = dto.Summary,
            Category = dto.Category,
            Difficulty = dto.Difficulty,
            LPReward = LPAwards.ArticleRead,
            PublishedAt = content.PublishedAt
        };
    }

    public async Task<ContentSummaryDto> GetTodaysFactAsync()
    {
        var contents = await _db.Contents.OrderBy(c => c.CreatedAt).ToListAsync();
        if (contents.Count == 0)
            throw new InvalidOperationException("No content available.");

        // Deterministic daily rotation based on days since epoch
        int daysSinceEpoch = (int)(DateTime.UtcNow - new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalDays;
        var todaysContent = contents[daysSinceEpoch % contents.Count];

        return new ContentSummaryDto
        {
            Id = todaysContent.Id,
            Title = todaysContent.Title,
            Summary = todaysContent.BodyMarkdown.Length > 200 ? todaysContent.BodyMarkdown[..200] + "..." : todaysContent.BodyMarkdown,
            Category = Enum.TryParse<ActionCategory>(todaysContent.Category, true, out var cat) ? cat : ActionCategory.Energy,
            Difficulty = todaysContent.Difficulty,
            IsRead = false
        };
    }

    public async Task SeedContentAsync()
    {
        if (await _db.Contents.AnyAsync())
            return;

        var seedContent = GetSeedContent();
        _db.Contents.AddRange(seedContent);
        await _db.SaveChangesAsync();
    }

    private static List<Content> GetSeedContent() =>
    [
        new() { Id = Guid.NewGuid(), Title = "The Greenhouse Effect Explained", BodyMarkdown = "The greenhouse effect is a natural process that warms Earth's surface. When the Sun's energy reaches Earth, some is reflected back to space and the rest is absorbed. Greenhouse gases (CO2, methane, N2O) trap heat in the atmosphere, keeping Earth ~33°C warmer than it would be otherwise. Human activities, primarily burning fossil fuels, have increased atmospheric CO2 from 280 ppm pre-industrial to over 420 ppm today. Source: NASA Climate Change (2024).", Category = "Energy", Source = "NASA", PublishedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Carbon Footprint of Food", BodyMarkdown = "Food production accounts for ~26% of global greenhouse gas emissions. Beef and dairy have the highest carbon footprints per calorie — producing 1 kg of beef emits ~60 kg CO2e, while 1 kg of tofu emits ~3 kg CO2e. Shifting to plant-rich diets could reduce food emissions by 49%. Source: Poore & Nemecek, Science (2018); IPCC AR6 (2022).", Category = "Food", Source = "IPCC", PublishedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Transportation Emissions by Mode", BodyMarkdown = "Transportation accounts for ~16.2% of global GHG emissions. Per passenger-kilometer: flights emit 255g CO2e, cars 192g, buses 105g, rail 41g, and cycling/walking ~0g. Switching one car commute to public transit saves ~2.6 tonnes CO2e/year on average. Source: IEA Transport (2023); EPA Emission Factors Hub.", Category = "Transport", Source = "EPA", PublishedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Renewable Energy Revolution", BodyMarkdown = "In 2023, renewables accounted for 30% of global electricity generation — a record high. Solar PV costs dropped 90% in the last decade. Wind and solar are now the cheapest sources of new electricity in most of the world. A single rooftop solar installation can offset ~1.5 tonnes CO2e per year. Source: IRENA Renewable Energy Statistics (2024); IEA World Energy Outlook.", Category = "Energy", Source = "IRENA", PublishedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Fast Fashion's Environmental Cost", BodyMarkdown = "The fashion industry produces 10% of global carbon emissions — more than international flights and maritime shipping combined. It's the second-largest consumer of water globally. Extending the life of clothes by just 9 months reduces carbon, water, and waste footprints by 20-30%. Buying secondhand saves an average of 25 kg CO2e per garment. Source: UNEP (2023); Ellen MacArthur Foundation.", Category = "Shopping", Source = "UNEP", PublishedAt = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "The Science of Composting", BodyMarkdown = "Organic waste in landfills generates methane (CH4), a greenhouse gas 80x more potent than CO2 over 20 years. Composting diverts this waste, reducing methane emissions and creating nutrient-rich soil. Home composting can divert ~200 kg of organic waste per year, avoiding ~100 kg CO2e in landfill emissions. Source: EPA Composting Research (2023); Project Drawdown.", Category = "Waste", Source = "EPA", PublishedAt = new DateTime(2024, 1, 25, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Carbon Sinks: Forests and Oceans", BodyMarkdown = "Forests absorb ~2.6 billion tonnes of CO2 annually — about 30% of human emissions. Oceans absorb another ~2.5 billion tonnes but are becoming more acidic as a result. Mangrove forests store 3-5x more carbon per hectare than terrestrial forests. Protecting and restoring these sinks is critical for climate stability. Source: Global Carbon Project (2023); NASA Earth Observatory.", Category = "Travel", Source = "NASA", PublishedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 5, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "LED Lighting and Energy Savings", BodyMarkdown = "LED bulbs use 75% less energy than incandescent bulbs and last 25x longer. If every US household replaced one incandescent bulb with an LED, it would save enough energy to light 3 million homes for a year. Globally, the shift to LED lighting could save 1,400 million tonnes CO2e by 2030. Source: DOE Energy Efficiency (2023).", Category = "Energy", Source = "DOE", PublishedAt = new DateTime(2024, 2, 5, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Water-Energy Nexus", BodyMarkdown = "Heating water accounts for ~17% of household energy use. Taking shorter showers (5 min vs 10 min) saves ~350 kg CO2e/year. Cold-water laundry can reduce a load's energy use by 90%. Water treatment and distribution consume ~2% of total US energy. Source: EPA WaterSense (2023); DEFRA GHG Reporting.", Category = "Energy", Source = "EPA", PublishedAt = new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "The Impact of Food Waste", BodyMarkdown = "One-third of all food produced globally is wasted, generating 8-10% of global GHG emissions. If food waste were a country, it would be the 3rd largest emitter after China and the US. Reducing household food waste by 50% could save ~1 tonne CO2e per family per year. Source: FAO (2023); IPCC Special Report on Land.", Category = "Food", Source = "FAO", PublishedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Electric Vehicles: The Full Picture", BodyMarkdown = "Over their lifetime, EVs produce 50-70% fewer emissions than petrol cars, even accounting for battery manufacturing and electricity generation. The carbon payback period for an EV is typically 1-2 years. Battery recycling technologies are advancing rapidly. Source: ICCT (2023); BloombergNEF EV Outlook.", Category = "Transport", Source = "ICCT", PublishedAt = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 5, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Circular Economy Basics", BodyMarkdown = "The circular economy aims to eliminate waste through better design, reuse, repair, and recycling. Only 8.6% of the global economy is circular. Transitioning to a circular economy could reduce global GHG emissions by 39% and cut virgin resource use by 28%. Source: Circle Economy Gap Report (2024); Ellen MacArthur Foundation.", Category = "Shopping", Source = "Ellen MacArthur Foundation", PublishedAt = new DateTime(2024, 2, 25, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Methane: The Other Greenhouse Gas", BodyMarkdown = "Methane is 80x more potent than CO2 over 20 years. Major sources: agriculture (40%), fossil fuels (35%), and waste (20%). Reducing methane emissions 45% by 2030 could avoid 0.3°C of warming. Simple actions: reduce food waste, eat less red meat, compost organics. Source: Global Methane Pledge; IPCC AR6 (2022).", Category = "Food", Source = "IPCC", PublishedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Carbon Offsetting: What Works", BodyMarkdown = "Carbon offsets fund projects that reduce or remove CO2 from the atmosphere. High-quality offsets include reforestation, direct air capture, and clean cookstoves. Look for Gold Standard or VCS certification. Offsetting should complement, not replace, direct emission reductions. Source: Gold Standard Foundation; Project Drawdown (2023).", Category = "Travel", Source = "Project Drawdown", PublishedAt = new DateTime(2024, 3, 5, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Advanced },
        new() { Id = Guid.NewGuid(), Title = "Home Insulation Impact", BodyMarkdown = "Heating and cooling account for ~50% of household energy use. Proper insulation can reduce heating/cooling energy by 30-50%. Upgrading from no insulation to recommended levels saves ~900 kg CO2e per year. Sealing air leaks is the most cost-effective improvement. Source: DOE Building Technologies (2023); DEFRA.", Category = "Energy", Source = "DOE", PublishedAt = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "The True Cost of Plastic", BodyMarkdown = "Plastic production and incineration add 850 million tonnes of CO2e to the atmosphere annually. Only 9% of all plastic ever produced has been recycled. Reducing single-use plastics and choosing reusable alternatives can save ~23 kg CO2e per person per year. Source: CIEL (2019); UNEP Plastics Report (2023).", Category = "Waste", Source = "UNEP", PublishedAt = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Sustainable Travel Tips", BodyMarkdown = "Aviation accounts for 2.5% of global CO2 emissions but 3.5% of warming when contrails are included. A round-trip transatlantic flight emits ~1.6 tonnes CO2e per passenger. Choose direct flights (takeoff/landing use the most fuel), offset when possible, and consider trains for shorter distances. Source: ICAO (2023); Atmosfair.", Category = "Travel", Source = "ICAO", PublishedAt = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Digital Carbon Footprint", BodyMarkdown = "The ICT sector produces 2-4% of global emissions. Streaming 1 hour of video emits ~36g CO2e. Storing 1 GB of data in the cloud uses ~7 kWh/year. Reducing digital clutter, choosing green hosting, and optimizing streaming quality can lower your digital footprint. Source: IEA (2023); The Shift Project.", Category = "Energy", Source = "IEA", PublishedAt = new DateTime(2024, 3, 25, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Regenerative Agriculture", BodyMarkdown = "Regenerative farming practices (cover crops, no-till, rotational grazing) can sequester 3-8 tonnes CO2e per hectare per year. If adopted on 40% of cropland globally, it could offset 15% of global emissions. Supporting regenerative farms through purchasing choices amplifies impact. Source: Rodale Institute (2023); Project Drawdown.", Category = "Food", Source = "Project Drawdown", PublishedAt = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 5, Difficulty = ContentDifficulty.Advanced },
        new() { Id = Guid.NewGuid(), Title = "Heat Pumps: The Future of Heating", BodyMarkdown = "Heat pumps are 3-5x more efficient than gas furnaces, moving heat rather than generating it. Switching from a gas furnace to a heat pump saves ~2.5 tonnes CO2e per year. Global heat pump sales grew 11% in 2023. Source: IEA Heat Pump Report (2024); DEFRA Emission Factors.", Category = "Energy", Source = "IEA", PublishedAt = new DateTime(2024, 4, 5, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Microplastics and Climate Change", BodyMarkdown = "Microplastics are found in 83% of tap water worldwide. As plastics break down, they release methane and ethylene. Ocean microplastics interfere with phytoplankton that absorb CO2. Reducing plastic use helps both pollution and climate. Source: NOAA Marine Debris (2023); Nature Geoscience.", Category = "Waste", Source = "NOAA", PublishedAt = new DateTime(2024, 4, 10, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "The Power of Trees", BodyMarkdown = "A single mature tree absorbs ~22 kg CO2 per year and produces enough oxygen for 2 people. Urban trees reduce cooling costs by 25-40% through shade. Planting 1 trillion trees could capture 205 billion tonnes of carbon. Source: ETH Zurich (2019); US Forest Service; Project Drawdown.", Category = "Travel", Source = "Project Drawdown", PublishedAt = new DateTime(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Seasonal and Local Eating", BodyMarkdown = "Imported out-of-season produce can have 5-10x the carbon footprint of local seasonal alternatives. Air-freighted food has 50x the emissions of sea-shipped food. Eating seasonally and locally can reduce food-related emissions by up to 20%. Source: DEFRA Emission Factors (2023); Environmental Science & Technology.", Category = "Food", Source = "DEFRA", PublishedAt = new DateTime(2024, 4, 20, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Smart Thermostats and Energy Savings", BodyMarkdown = "Smart thermostats save an average of 10-12% on heating and 15% on cooling costs. Lowering your thermostat by 1°C saves ~300 kg CO2e per year. Programming nighttime setbacks of 3-5°C reduces energy use by 10-15%. Source: EPA ENERGY STAR (2023); DOE.", Category = "Energy", Source = "EPA", PublishedAt = new DateTime(2024, 4, 25, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Repair vs Replace", BodyMarkdown = "Manufacturing a new smartphone emits ~70 kg CO2e. Repairing and extending its life by 2 years saves ~35 kg CO2e. The EU's Right to Repair legislation is driving manufacturers to design for longevity. Repair cafés worldwide fix thousands of items annually, diverting waste from landfills. Source: European Environment Agency (2023); iFixit.", Category = "Shopping", Source = "EEA", PublishedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Tipping Points in the Climate System", BodyMarkdown = "Scientists have identified ~16 climate tipping points that could be triggered between 1.5-2°C of warming. These include collapse of ice sheets, Amazon dieback, permafrost thaw, and coral reef die-off. Once triggered, these changes are irreversible on human timescales. Every fraction of a degree matters. Source: Nature (2022); IPCC AR6.", Category = "Energy", Source = "IPCC", PublishedAt = new DateTime(2024, 5, 5, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 5, Difficulty = ContentDifficulty.Advanced },
        new() { Id = Guid.NewGuid(), Title = "Carpooling and Ride-sharing Impact", BodyMarkdown = "The average car carries 1.5 passengers. Increasing to 3 passengers halves per-person emissions. Regular carpooling saves ~1 tonne CO2e per person per year. Ride-sharing apps make it easier than ever to find commute partners. Source: EPA GHG Equivalencies; IEA Transport.", Category = "Transport", Source = "EPA", PublishedAt = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Ocean Acidification", BodyMarkdown = "Oceans have absorbed ~30% of human-produced CO2, causing a 26% increase in acidity since pre-industrial times. This threatens coral reefs, shellfish, and marine food chains. Reducing CO2 emissions is the only way to slow acidification. Source: NOAA Ocean Acidification Program (2023); IPCC AR6.", Category = "Travel", Source = "NOAA", PublishedAt = new DateTime(2024, 5, 15, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Advanced },
        new() { Id = Guid.NewGuid(), Title = "Eco-friendly Laundry", BodyMarkdown = "Washing clothes at 30°C instead of 60°C uses 40% less energy. Line-drying instead of machine-drying saves ~150 kg CO2e per household per year. Washing full loads only saves water and energy. Using eco-certified detergents reduces water pollution. Source: DEFRA (2023); European Commission.", Category = "Energy", Source = "DEFRA", PublishedAt = new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 3, Difficulty = ContentDifficulty.Beginner },
        new() { Id = Guid.NewGuid(), Title = "Biodiversity and Climate", BodyMarkdown = "Climate change and biodiversity loss are deeply interconnected. 1 million species face extinction. Healthy ecosystems are crucial carbon sinks. Protecting biodiversity through habitat preservation, sustainable farming, and reducing pollution supports both nature and climate goals. Source: IPBES Global Assessment (2023); WWF Living Planet Report.", Category = "Travel", Source = "IPBES", PublishedAt = new DateTime(2024, 5, 25, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Intermediate },
        new() { Id = Guid.NewGuid(), Title = "Zero-Waste Living Guide", BodyMarkdown = "The average person generates ~800 kg of waste per year. Zero-waste principles: Refuse, Reduce, Reuse, Recycle, Rot. Start with reusable bags, bottles, and containers. Bulk buying reduces packaging waste by 80%. A zero-waste lifestyle can cut personal emissions by 1-2 tonnes CO2e/year. Source: EPA Waste Data (2023); Zero Waste International Alliance.", Category = "Waste", Source = "EPA", PublishedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), ReadingTimeMinutes = 4, Difficulty = ContentDifficulty.Beginner },
    ];
}
