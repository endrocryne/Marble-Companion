using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ML;

public record SuggestionItem(
    string Id,
    string Title,
    string Description,
    ActionCategory Category,
    SuggestionImpact Impact,
    SuggestionEffort Effort,
    decimal EstimatedCO2eSavingKgPerYear);

public static class SuggestionLibrary
{
    private static readonly List<SuggestionItem> _suggestions = BuildSuggestions();

    public static IReadOnlyList<SuggestionItem> All => _suggestions;

    public static IEnumerable<SuggestionItem> ByCategory(ActionCategory category)
        => _suggestions.Where(s => s.Category == category);

    public static SuggestionItem? ById(string id)
        => _suggestions.FirstOrDefault(s => s.Id == id);

    private static List<SuggestionItem> BuildSuggestions() =>
    [
        // ═══════════════════════════════════════════════════════
        // TRANSPORT (30 suggestions)
        // ═══════════════════════════════════════════════════════
        new("TR001", "Bike to work", "Replace your daily car commute with cycling for distances under 10 km.", ActionCategory.Transport, SuggestionImpact.High, SuggestionEffort.Medium, 1200m),
        new("TR002", "Use public transit", "Switch from solo car trips to bus or metro for your daily commute.", ActionCategory.Transport, SuggestionImpact.High, SuggestionEffort.Low, 900m),
        new("TR003", "Carpool with coworkers", "Share your commute with at least one colleague to halve per-person emissions.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Medium, 600m),
        new("TR004", "Walk short errands", "Walk instead of driving for trips under 2 km.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 200m),
        new("TR005", "Switch to an EV", "Replace your petrol/diesel vehicle with an electric vehicle.", ActionCategory.Transport, SuggestionImpact.High, SuggestionEffort.High, 2000m),
        new("TR006", "Maintain tire pressure", "Keep tires properly inflated to improve fuel efficiency by up to 3%.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 100m),
        new("TR007", "Adopt eco-driving habits", "Accelerate gently, maintain steady speed, and anticipate traffic flow.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 300m),
        new("TR008", "Work from home one day a week", "Eliminate one commute day per week to reduce annual transport emissions.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 250m),
        new("TR009", "Use an e-scooter for short trips", "Replace short car trips with an electric scooter.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Medium, 350m),
        new("TR010", "Combine errands into one trip", "Plan your errands to minimize the number of separate car trips.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 150m),
        new("TR011", "Take the train instead of flying domestically", "Choose rail over air for domestic trips under 800 km.", ActionCategory.Transport, SuggestionImpact.High, SuggestionEffort.Medium, 500m),
        new("TR012", "Use a car-sharing service", "Use shared vehicles instead of owning a second car.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Medium, 400m),
        new("TR013", "Avoid idling your engine", "Turn off your engine when waiting more than 30 seconds.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 80m),
        new("TR014", "Use cruise control on highways", "Maintain a constant speed to reduce fuel consumption on long drives.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 120m),
        new("TR015", "Remove roof racks when not in use", "Reduce aerodynamic drag by removing unused roof cargo carriers.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 60m),
        new("TR016", "Downsize your vehicle", "Trade your SUV for a smaller, more fuel-efficient car.", ActionCategory.Transport, SuggestionImpact.High, SuggestionEffort.High, 1500m),
        new("TR017", "Use a cargo bike for shopping", "Replace car trips to the store with a cargo bicycle.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Medium, 350m),
        new("TR018", "Take a bus for intercity travel", "Choose coach/bus over driving alone for intercity trips.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 300m),
        new("TR019", "Plan routes to avoid congestion", "Use navigation apps to avoid heavy traffic and reduce idle time.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 90m),
        new("TR020", "Switch to a hybrid vehicle", "Upgrade to a hybrid if a full EV isn't feasible yet.", ActionCategory.Transport, SuggestionImpact.High, SuggestionEffort.High, 1000m),
        new("TR021", "Use bike-share programs", "Use city bike-share stations for short urban trips.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 180m),
        new("TR022", "Teleconference instead of driving to meetings", "Replace in-person meetings with video calls when possible.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 200m),
        new("TR023", "Service your car regularly", "Regular maintenance keeps engine efficiency optimal.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 130m),
        new("TR024", "Avoid drive-throughs", "Park and walk in instead of idling in drive-through lanes.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 40m),
        new("TR025", "Use lightweight motor oil", "Switch to low-viscosity motor oil for better fuel economy.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Low, 50m),
        new("TR026", "Fly economy class", "Economy seats have a lower per-passenger carbon footprint than business class.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 400m),
        new("TR027", "Offset your flights", "Purchase verified carbon offsets for unavoidable air travel.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Low, 300m),
        new("TR028", "Walk children to school", "Walk or cycle your kids to school instead of driving.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Medium, 250m),
        new("TR029", "Use ferry instead of flying to nearby islands", "Choose sea transport over short-hop flights where available.", ActionCategory.Transport, SuggestionImpact.Medium, SuggestionEffort.Medium, 200m),
        new("TR030", "Advocate for bike lanes in your community", "Support infrastructure that encourages cycling.", ActionCategory.Transport, SuggestionImpact.Low, SuggestionEffort.Medium, 50m),

        // ═══════════════════════════════════════════════════════
        // FOOD (30 suggestions)
        // ═══════════════════════════════════════════════════════
        new("FD001", "Eat plant-based one day a week", "Replace all animal products with plant-based meals one day per week.", ActionCategory.Food, SuggestionImpact.High, SuggestionEffort.Low, 200m),
        new("FD002", "Reduce beef consumption by half", "Cut your beef intake in half and substitute with poultry, fish, or legumes.", ActionCategory.Food, SuggestionImpact.High, SuggestionEffort.Medium, 600m),
        new("FD003", "Buy seasonal produce", "Choose fruits and vegetables that are in season locally.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("FD004", "Reduce food waste by meal planning", "Plan weekly meals and shop with a list to avoid overbuying.", ActionCategory.Food, SuggestionImpact.High, SuggestionEffort.Medium, 300m),
        new("FD005", "Start composting at home", "Compost food scraps instead of sending them to landfill.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 150m),
        new("FD006", "Choose local produce", "Buy from local farmers' markets to reduce transportation emissions.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("FD007", "Switch from dairy milk to oat milk", "Replace cow's milk with oat milk in coffee, cereal, and cooking.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 130m),
        new("FD008", "Grow your own herbs", "Grow basil, mint, parsley and other herbs at home.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 20m),
        new("FD009", "Eat less lamb", "Lamb has one of the highest carbon footprints per calorie; reduce intake.", ActionCategory.Food, SuggestionImpact.High, SuggestionEffort.Medium, 350m),
        new("FD010", "Reduce cheese consumption", "Cut cheese intake by a third; cheese is emissions-intensive.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 150m),
        new("FD011", "Adopt a fully vegan diet", "Eliminate all animal products for the greatest dietary carbon reduction.", ActionCategory.Food, SuggestionImpact.High, SuggestionEffort.High, 900m),
        new("FD012", "Use a slow cooker", "Slow cookers use less energy than ovens for the same meals.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 50m),
        new("FD013", "Buy in bulk to reduce packaging", "Purchase staples like rice, pasta, and beans in bulk.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 30m),
        new("FD014", "Freeze leftovers instead of discarding", "Portion and freeze surplus meals for later consumption.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 80m),
        new("FD015", "Replace one meat meal daily with legumes", "Swap one daily meat dish for lentils, chickpeas, or beans.", ActionCategory.Food, SuggestionImpact.High, SuggestionEffort.Medium, 500m),
        new("FD016", "Avoid air-freighted foods", "Skip fresh produce that's been air-freighted (e.g., out-of-season berries).", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 120m),
        new("FD017", "Drink tap water instead of bottled", "Avoid bottled water and use a reusable bottle with tap or filtered water.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 30m),
        new("FD018", "Eat more whole grains", "Choose whole grains over processed foods for lower production emissions.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 40m),
        new("FD019", "Reduce coffee consumption", "Cut from three cups to one per day; coffee production has a notable footprint.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Medium, 60m),
        new("FD020", "Cook from scratch more often", "Avoid processed/pre-packaged meals which have higher embodied emissions.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 100m),
        new("FD021", "Use FIFO in your fridge", "First in, first out: use older items first to prevent spoilage.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 80m),
        new("FD022", "Eat smaller portions", "Reducing portion sizes cuts waste and total food demand.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 120m),
        new("FD023", "Replace rice with lower-emission grains", "Swap some rice meals for pasta, couscous, or quinoa (rice paddies emit methane).", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("FD024", "Choose sustainable seafood", "Consult guides like MSC to pick sustainably caught or farmed fish.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 80m),
        new("FD025", "Avoid single-use food packaging", "Bring reusable containers for takeout and lunch.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 25m),
        new("FD026", "Start a kitchen garden", "Grow tomatoes, lettuce, and peppers in your backyard or balcony.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 70m),
        new("FD027", "Use a pressure cooker", "Pressure cookers reduce cooking time and energy by up to 70%.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 40m),
        new("FD028", "Eat ugly produce", "Buy imperfect fruits and vegetables to reduce farm-level waste.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Low, 35m),
        new("FD029", "Join a community-supported agriculture (CSA) program", "Get weekly local produce boxes from nearby farms.", ActionCategory.Food, SuggestionImpact.Medium, SuggestionEffort.Medium, 90m),
        new("FD030", "Reduce alcohol consumption", "Beer and wine production have significant carbon and water footprints.", ActionCategory.Food, SuggestionImpact.Low, SuggestionEffort.Medium, 60m),

        // ═══════════════════════════════════════════════════════
        // ENERGY (28 suggestions)
        // ═══════════════════════════════════════════════════════
        new("EN001", "Switch to LED bulbs", "Replace all incandescent and CFL bulbs with LEDs.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 80m),
        new("EN002", "Install a smart thermostat", "Optimize heating and cooling schedules automatically.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.Medium, 500m),
        new("EN003", "Lower thermostat by 2°C in winter", "Reduce your heating setpoint for significant energy savings.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.Low, 400m),
        new("EN004", "Air-dry your laundry", "Skip the tumble dryer and hang clothes to dry.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 150m),
        new("EN005", "Unplug devices when not in use", "Eliminate phantom/standby power draw from electronics.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Low, 50m),
        new("EN006", "Switch to a green energy tariff", "Choose a 100% renewable electricity provider.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.Low, 1500m),
        new("EN007", "Install solar panels", "Generate your own clean electricity from rooftop solar.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 2500m),
        new("EN008", "Insulate your attic", "Add or upgrade attic insulation to reduce heat loss.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 800m),
        new("EN009", "Seal drafts around windows and doors", "Use weatherstripping and caulk to prevent air leaks.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Medium, 200m),
        new("EN010", "Wash laundry in cold water", "Use cold water cycles; most detergents work fine at 30°C.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("EN011", "Install a heat pump", "Replace gas boiler with an air- or ground-source heat pump.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 1800m),
        new("EN012", "Use a smart power strip", "Automatically cut power to devices in standby mode.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Low, 60m),
        new("EN013", "Upgrade to energy-efficient appliances", "Replace old appliances with A+++ rated models when they wear out.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 400m),
        new("EN014", "Install double/triple glazing", "Upgrade windows to reduce heat loss significantly.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 600m),
        new("EN015", "Use a fan instead of air conditioning", "Ceiling or portable fans use far less energy than AC.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 200m),
        new("EN016", "Set water heater to 60°C", "Lower your hot water temperature to save energy safely.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 120m),
        new("EN017", "Take shorter showers", "Reduce shower time by 2 minutes to save hot water energy.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 150m),
        new("EN018", "Install a low-flow showerhead", "Reduce water flow while maintaining pressure.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("EN019", "Use natural light during the day", "Open blinds and curtains instead of turning on lights.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Low, 30m),
        new("EN020", "Run dishwasher only when full", "Wait until the dishwasher is completely full before running it.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Low, 40m),
        new("EN021", "Install motion-sensor lights", "Automate outdoor and hallway lighting to avoid waste.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Medium, 30m),
        new("EN022", "Install cavity wall insulation", "Insulate hollow walls to prevent heat escaping.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 700m),
        new("EN023", "Use a programmable timer for heating", "Schedule heating only when you're home and awake.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Low, 250m),
        new("EN024", "Install reflective foil behind radiators", "Reflect heat back into the room instead of losing it through walls.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Low, 50m),
        new("EN025", "Cook with lids on pots", "Covering pots reduces cooking time and energy use by up to 25%.", ActionCategory.Energy, SuggestionImpact.Low, SuggestionEffort.Low, 20m),
        new("EN026", "Use an induction hob", "Induction cooking is 90% efficient vs 40% for gas.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Medium, 150m),
        new("EN027", "Install a home battery system", "Store solar energy for evening use to reduce grid reliance.", ActionCategory.Energy, SuggestionImpact.High, SuggestionEffort.High, 600m),
        new("EN028", "Get a home energy audit", "Identify the biggest energy waste areas in your home.", ActionCategory.Energy, SuggestionImpact.Medium, SuggestionEffort.Medium, 200m),

        // ═══════════════════════════════════════════════════════
        // SHOPPING (25 suggestions)
        // ═══════════════════════════════════════════════════════
        new("SH001", "Buy second-hand clothing", "Shop at thrift stores or online resale platforms instead of buying new.", ActionCategory.Shopping, SuggestionImpact.High, SuggestionEffort.Low, 300m),
        new("SH002", "Repair instead of replacing", "Fix broken items before buying replacements.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Medium, 200m),
        new("SH003", "Choose durable products", "Invest in quality items that last longer, even if more expensive upfront.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Medium, 150m),
        new("SH004", "Avoid fast fashion", "Buy fewer, better-quality clothing items each year.", ActionCategory.Shopping, SuggestionImpact.High, SuggestionEffort.Medium, 400m),
        new("SH005", "Use reusable shopping bags", "Bring your own bags instead of using single-use plastic bags.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 10m),
        new("SH006", "Buy refurbished electronics", "Choose certified refurbished phones, laptops, and gadgets.", ActionCategory.Shopping, SuggestionImpact.High, SuggestionEffort.Low, 250m),
        new("SH007", "Keep your phone for 4+ years", "Extend the life of your smartphone instead of upgrading yearly.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Low, 80m),
        new("SH008", "Borrow or rent rarely-used items", "Use libraries, tool libraries, or rental services for occasional needs.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("SH009", "Choose products with minimal packaging", "Prefer items with recyclable or no packaging.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 30m),
        new("SH010", "Buy concentrated cleaning products", "Use refillable or concentrated cleaners to reduce packaging and transport.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 20m),
        new("SH011", "Use a safety razor instead of disposable", "Switch to a reusable metal safety razor.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("SH012", "Choose bamboo or recycled toilet paper", "Pick sustainable alternatives to virgin-wood toilet paper.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 10m),
        new("SH013", "Swap paper towels for cloth towels", "Use washable cloth towels in the kitchen.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 15m),
        new("SH014", "Buy local handmade goods", "Support local artisans to reduce shipping and factory emissions.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Medium, 40m),
        new("SH015", "Use a menstrual cup or reusable pads", "Reduce disposable hygiene product waste.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 10m),
        new("SH016", "Choose eco-certified products", "Look for labels like FSC, Fair Trade, or B Corp.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Low, 50m),
        new("SH017", "Consolidate online orders", "Avoid multiple small deliveries; batch your orders.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 30m),
        new("SH018", "Sell or donate unwanted items", "Keep items in circulation rather than sending to landfill.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Low, 80m),
        new("SH019", "Use rechargeable batteries", "Replace disposable batteries with rechargeables.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 10m),
        new("SH020", "Choose digital over physical media", "Stream or download instead of buying CDs, DVDs, or books when possible.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 15m),
        new("SH021", "Buy furniture second-hand", "Shop for used furniture to avoid new manufacturing emissions.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Medium, 120m),
        new("SH022", "Use bar soap instead of liquid soap", "Bar soap has lower packaging and production emissions.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("SH023", "Choose natural fiber clothing", "Opt for organic cotton, linen, or hemp over synthetic fabrics.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.Medium, 60m),
        new("SH024", "Unsubscribe from marketing emails", "Reduce impulse purchases driven by promotional emails.", ActionCategory.Shopping, SuggestionImpact.Low, SuggestionEffort.Low, 20m),
        new("SH025", "Do a no-spend month challenge", "Challenge yourself to buy only essentials for one month.", ActionCategory.Shopping, SuggestionImpact.Medium, SuggestionEffort.High, 100m),

        // ═══════════════════════════════════════════════════════
        // TRAVEL (20 suggestions)
        // ═══════════════════════════════════════════════════════
        new("TV001", "Take a staycation", "Explore your own region instead of flying abroad for vacation.", ActionCategory.Travel, SuggestionImpact.High, SuggestionEffort.Low, 1000m),
        new("TV002", "Choose train over plane for European trips", "Take overnight trains instead of short-haul flights.", ActionCategory.Travel, SuggestionImpact.High, SuggestionEffort.Medium, 800m),
        new("TV003", "Stay in eco-certified accommodations", "Book hotels or hostels with verified sustainability certifications.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("TV004", "Take fewer but longer trips", "Replace three short holidays with one longer trip per year.", ActionCategory.Travel, SuggestionImpact.High, SuggestionEffort.Medium, 600m),
        new("TV005", "Use public transport at your destination", "Explore via buses, trams, and metro rather than renting a car.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Low, 150m),
        new("TV006", "Pack light", "Lighter luggage means less fuel burned per passenger.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.Low, 20m),
        new("TV007", "Choose direct flights", "Avoid layovers; takeoffs and landings are the most fuel-intensive phases.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Low, 200m),
        new("TV008", "Travel by coach/bus for medium distances", "Intercity coaches produce far less CO2 per passenger-km than planes.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Medium, 300m),
        new("TV009", "Cycle tour instead of road trip", "Plan a cycling holiday for a zero-emission adventure.", ActionCategory.Travel, SuggestionImpact.High, SuggestionEffort.High, 400m),
        new("TV010", "Reuse hotel towels", "Decline daily towel changes to save water and energy.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("TV011", "Eat local food while traveling", "Choose restaurants serving local cuisine over imported food chains.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.Low, 30m),
        new("TV012", "Skip the cruise", "Cruise ships are extremely carbon-intensive per passenger day.", ActionCategory.Travel, SuggestionImpact.High, SuggestionEffort.Medium, 1200m),
        new("TV013", "Use a sail or electric boat tour", "Choose low-emission boat experiences.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Medium, 80m),
        new("TV014", "Carry a reusable water bottle while traveling", "Avoid buying plastic bottles at tourist destinations.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("TV015", "Volunteer for conservation while traveling", "Participate in local environmental projects at your destination.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.High, 30m),
        new("TV016", "Choose camping over resort stays", "Camping has a much smaller carbon footprint than resort hotels.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Medium, 150m),
        new("TV017", "Fly with newer, more fuel-efficient airlines", "Some carriers operate newer fleets that emit 20% less per seat.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Low, 200m),
        new("TV018", "Use home-exchange for vacations", "Swap homes instead of booking hotel rooms.", ActionCategory.Travel, SuggestionImpact.Medium, SuggestionEffort.Medium, 80m),
        new("TV019", "Visit national parks instead of theme parks", "Nature-based tourism generally has lower emissions.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.Low, 50m),
        new("TV020", "Plan travel during off-peak seasons", "Fewer crowds means less infrastructure strain and sometimes shorter routes.", ActionCategory.Travel, SuggestionImpact.Low, SuggestionEffort.Low, 40m),

        // ═══════════════════════════════════════════════════════
        // WASTE (22 suggestions)
        // ═══════════════════════════════════════════════════════
        new("WA001", "Recycle correctly", "Learn your local recycling rules and sort waste properly.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Low, 100m),
        new("WA002", "Start a home compost bin", "Divert organic waste from landfill by composting at home.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Medium, 200m),
        new("WA003", "Use a reusable coffee cup", "Bring your own cup to coffee shops to avoid disposable cups.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 10m),
        new("WA004", "Refuse unnecessary packaging", "Ask shops not to bag items you can carry by hand.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 15m),
        new("WA005", "Bring reusable containers for takeout", "Use your own containers when getting takeaway food.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 10m),
        new("WA006", "Recycle electronics properly", "Take old electronics to e-waste recycling centers.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Low, 40m),
        new("WA007", "Use beeswax wraps instead of cling film", "Replace single-use plastic wrap with reusable beeswax wraps.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("WA008", "Buy in bulk with your own containers", "Use zero-waste shops to refill containers.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Medium, 50m),
        new("WA009", "Switch to a bamboo toothbrush", "Replace plastic toothbrushes with biodegradable bamboo ones.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 3m),
        new("WA010", "Use cloth napkins at home", "Replace paper napkins with washable cloth versions.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("WA011", "Repair clothing instead of discarding", "Learn basic sewing or take clothes to a tailor.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Medium, 70m),
        new("WA012", "Donate old clothes", "Give wearable clothing to charity instead of throwing it away.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Low, 50m),
        new("WA013", "Use a bidet or washlet", "Reduce toilet paper waste with a bidet attachment.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Medium, 15m),
        new("WA014", "Avoid single-use cutlery", "Carry a reusable cutlery set for eating on the go.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("WA015", "Return packaging to manufacturers", "Use manufacturer take-back programs for packaging.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Medium, 20m),
        new("WA016", "Set up a textile recycling bin", "Collect worn-out fabrics separately for textile recycling.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 15m),
        new("WA017", "Use reusable produce bags", "Bring mesh bags for fruits and vegetables at the store.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("WA018", "Participate in community cleanups", "Join local litter pickup events to improve waste awareness.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Medium, 10m),
        new("WA019", "Choose products in recyclable packaging", "Prefer glass, aluminum, or cardboard over mixed plastics.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Low, 40m),
        new("WA020", "Go paperless for bills and statements", "Switch all bills, bank statements, and subscriptions to digital.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 20m),
        new("WA021", "Recycle cooking oil properly", "Take used cooking oil to a recycling point; never pour down drains.", ActionCategory.Waste, SuggestionImpact.Low, SuggestionEffort.Low, 5m),
        new("WA022", "Use a worm farm for apartment composting", "Vermicomposting works great in small spaces.", ActionCategory.Waste, SuggestionImpact.Medium, SuggestionEffort.Medium, 100m),
    ];
}
