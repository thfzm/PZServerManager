using PZServerManager.Models;

namespace PZServerManager.Services;

public static class SandboxSchema
{
    public const string CatPopulation = "Population";
    public const string CatZombieLore = "Zombie Lore";
    public const string CatZombieConfig = "Zombie Config";
    public const string CatLoot = "Loot";
    public const string CatSurvival = "Survival";
    public const string CatTime = "Time";
    public const string CatWeather = "Weather";
    public const string CatVehicles = "Vehicles";
    public const string CatCharacter = "Character";
    public const string CatWorld = "World Events";
    public const string CatMisc = "Misc";

    public static readonly string[] CategoryOrder =
    {
        CatPopulation, CatZombieLore, CatZombieConfig, CatLoot,
        CatSurvival, CatTime, CatWeather, CatVehicles,
        CatCharacter, CatWorld, CatMisc,
    };

    private static IReadOnlyList<SandboxChoice> Cs(params (int v, string l)[] items)
        => items.Select(t => new SandboxChoice(t.v, t.l)).ToArray();

    private static readonly IReadOnlyList<SandboxChoice> Population =
        Cs((1, "Insane"), (2, "Very High"), (3, "High"), (4, "Normal"), (5, "Low"));

    private static readonly IReadOnlyList<SandboxChoice> Loot =
        Cs((1, "Extremely Rare"), (2, "Rare"), (3, "Normal"), (4, "Common"), (5, "Abundant"));

    public static readonly IReadOnlyList<SandboxFieldDef> AllFields = new SandboxFieldDef[]
    {
        // ---- Population ----
        new("Zombies", "Zombie population", SandboxFieldType.IntChoices, CatPopulation, null, "3", Choices: Population),
        new("Distribution", "Zombie distribution", SandboxFieldType.IntChoices, CatPopulation,
            "1=Urban Focused, 2=Uniform.", "1",
            Choices: Cs((1, "Urban Focused"), (2, "Uniform"))),

        // ---- Zombie Lore ----
        new("ZombieLore.Speed", "Speed", SandboxFieldType.IntChoices, CatZombieLore, null, "2",
            Choices: Cs((1, "Sprinters"), (2, "Fast Shamblers"), (3, "Shamblers"))),
        new("ZombieLore.Strength", "Strength", SandboxFieldType.IntChoices, CatZombieLore, null, "2",
            Choices: Cs((1, "Superhuman"), (2, "Normal"), (3, "Weak"))),
        new("ZombieLore.Toughness", "Toughness", SandboxFieldType.IntChoices, CatZombieLore, null, "2",
            Choices: Cs((1, "Tough"), (2, "Normal"), (3, "Fragile"))),
        new("ZombieLore.Transmission", "Infection transmission", SandboxFieldType.IntChoices, CatZombieLore, null, "1",
            Choices: Cs((1, "Blood + Saliva"), (2, "Saliva Only"), (3, "Everyone's Infected"), (4, "None"))),
        new("ZombieLore.Mortality", "Mortality (1=fast … 6=slow)", SandboxFieldType.Int, CatZombieLore, null, "5", IntMin: 1, IntMax: 6),
        new("ZombieLore.Reanimate", "Reanimate timing (1=fast … 6=slow)", SandboxFieldType.Int, CatZombieLore, null, "3", IntMin: 1, IntMax: 6),
        new("ZombieLore.Cognition", "Cognition", SandboxFieldType.IntChoices, CatZombieLore, null, "3",
            Choices: Cs((1, "Navigate + Use Doors"), (2, "Navigate"), (3, "Basic"))),
        new("ZombieLore.Memory", "Memory", SandboxFieldType.IntChoices, CatZombieLore, null, "2",
            Choices: Cs((1, "Long"), (2, "Normal"), (3, "Short"), (4, "None"))),
        new("ZombieLore.Sight", "Sight", SandboxFieldType.IntChoices, CatZombieLore, null, "2",
            Choices: Cs((1, "Eagle"), (2, "Normal"), (3, "Poor"))),
        new("ZombieLore.Hearing", "Hearing", SandboxFieldType.IntChoices, CatZombieLore, null, "2",
            Choices: Cs((1, "Pinpoint"), (2, "Normal"), (3, "Poor"))),
        new("ZombieLore.CrawlUnderVehicle", "Crawl under vehicle (1–7)", SandboxFieldType.Int, CatZombieLore, null, "5", IntMin: 1, IntMax: 7),
        new("ZombieLore.ThumpNoChasing", "Thump while not chasing (1–7)", SandboxFieldType.Int, CatZombieLore, null, "1", IntMin: 1, IntMax: 7),
        new("ZombieLore.ThumpOnConstruction", "Thump on player constructions", SandboxFieldType.Bool, CatZombieLore, null, "true"),
        new("ZombieLore.ActiveOnly", "Active period", SandboxFieldType.IntChoices, CatZombieLore, null, "1",
            Choices: Cs((1, "Both"), (2, "Day"), (3, "Night"))),
        new("ZombieLore.TriggerHouseAlarm", "Trigger house alarms", SandboxFieldType.Bool, CatZombieLore, null, "false"),
        new("ZombieLore.ZombiesDragDown", "Zombies drag players down", SandboxFieldType.Bool, CatZombieLore, null, "true"),
        new("ZombieLore.ZombiesFenceLunge", "Fence lunge", SandboxFieldType.Bool, CatZombieLore, null, "true"),
        new("ZombieLore.EnableSmellMod", "Enable smell mod", SandboxFieldType.Bool, CatZombieLore, null, "false"),
        new("ZombieLore.DefaultSmellMod", "Default smell modifier", SandboxFieldType.Float, CatZombieLore, null, "1.0"),

        // ---- Zombie Config ----
        new("ZombieConfig.PopulationMultiplier", "Population multiplier", SandboxFieldType.Float, CatZombieConfig, null, "1.0"),
        new("ZombieConfig.PopulationStartMultiplier", "Start multiplier", SandboxFieldType.Float, CatZombieConfig, null, "1.0"),
        new("ZombieConfig.PopulationPeakMultiplier", "Peak multiplier", SandboxFieldType.Float, CatZombieConfig, null, "1.5"),
        new("ZombieConfig.PopulationPeakDay", "Days to peak", SandboxFieldType.Int, CatZombieConfig, null, "28", IntMin: 1, IntMax: 365),
        new("ZombieConfig.RespawnHours", "Respawn delay (hrs, 0 disables)", SandboxFieldType.Float, CatZombieConfig, null, "72.0"),
        new("ZombieConfig.RespawnUnseenHours", "Cell unseen hours", SandboxFieldType.Float, CatZombieConfig, null, "16.0"),
        new("ZombieConfig.RespawnMultiplier", "Respawn fraction", SandboxFieldType.Float, CatZombieConfig, null, "0.1"),
        new("ZombieConfig.RedistributeHours", "Redistribute hours", SandboxFieldType.Float, CatZombieConfig, null, "12.0"),
        new("ZombieConfig.FollowSoundDistance", "Sound follow distance", SandboxFieldType.Int, CatZombieConfig, null, "100", IntMin: 1, IntMax: 1000),
        new("ZombieConfig.RallyGroupSize", "Rally group size", SandboxFieldType.Int, CatZombieConfig, null, "20"),
        new("ZombieConfig.RallyTravelDistance", "Rally travel distance", SandboxFieldType.Int, CatZombieConfig, null, "20"),
        new("ZombieConfig.RallyGroupSeparation", "Rally group separation", SandboxFieldType.Int, CatZombieConfig, null, "15"),
        new("ZombieConfig.RallyGroupRadius", "Rally group radius", SandboxFieldType.Int, CatZombieConfig, null, "3"),

        // ---- Loot ----
        new("FoodLoot", "Food loot", SandboxFieldType.IntChoices, CatLoot, null, "2", Choices: Loot),
        new("WeaponLoot", "Weapon loot", SandboxFieldType.IntChoices, CatLoot, null, "2", Choices: Loot),
        new("OtherLoot", "Other loot", SandboxFieldType.IntChoices, CatLoot, null, "2", Choices: Loot),
        new("LootRespawn", "Loot respawn", SandboxFieldType.IntChoices, CatLoot, null, "1",
            Choices: Cs((1, "None"), (2, "Every Day"), (3, "Every Week"), (4, "Every Month"))),
        new("SeenHoursPreventLootRespawn", "Hours visited blocks respawn", SandboxFieldType.Int, CatLoot, null, "0", IntMin: 0, IntMax: 1024),

        // ---- Survival ----
        new("StatsDecrease", "Stat decrease (1=fast … 7=slow)", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 7),
        new("Nutrition", "Nutrition system", SandboxFieldType.Bool, CatSurvival, null, "true"),
        new("FoodRotSpeed", "Food rot speed", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 7),
        new("FridgeFactor", "Fridge factor", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 7),
        new("NatureAbundance", "Nature abundance", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 5),
        new("Farming", "Farming speed", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 7),
        new("CompostTime", "Compost time", SandboxFieldType.Int, CatSurvival, null, "2", IntMin: 1, IntMax: 7),
        new("PlantResilience", "Plant resilience", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 5),
        new("PlantAbundance", "Plant abundance", SandboxFieldType.Int, CatSurvival, null, "3", IntMin: 1, IntMax: 5),
        new("EndRegen", "Endurance regen", SandboxFieldType.Int, CatSurvival, null, "4", IntMin: 1, IntMax: 7),
        new("DaysForRottenFoodRemoval", "Days before rotten food removal (-1 = never)", SandboxFieldType.Int, CatSurvival, null, "-1", IntMin: -1, IntMax: 999),

        // ---- Time ----
        new("DayLength", "Day length", SandboxFieldType.IntChoices, CatTime, null, "3",
            Choices: Cs((1, "15 min"), (2, "30 min"), (3, "1 hr"), (4, "2 hr"), (5, "3 hr"), (6, "4 hr"), (7, "5 hr"), (8, "12 hr"), (9, "24 hr"))),
        new("StartYear", "Start year", SandboxFieldType.Int, CatTime, null, "1", IntMin: 1, IntMax: 9999),
        new("StartMonth", "Start month", SandboxFieldType.IntChoices, CatTime, null, "7",
            Choices: Cs((1, "Jan"), (2, "Feb"), (3, "Mar"), (4, "Apr"), (5, "May"), (6, "Jun"), (7, "Jul"), (8, "Aug"), (9, "Sep"), (10, "Oct"), (11, "Nov"), (12, "Dec"))),
        new("StartDay", "Start day", SandboxFieldType.Int, CatTime, null, "9", IntMin: 1, IntMax: 31),
        new("StartTime", "Start time", SandboxFieldType.IntChoices, CatTime, null, "2",
            Choices: Cs((1, "7am"), (2, "9am"), (3, "12pm"), (4, "5pm"), (5, "9pm"), (6, "12am"))),
        new("NightDarkness", "Night darkness", SandboxFieldType.Int, CatTime, null, "2", IntMin: 1, IntMax: 4),
        new("NightLength", "Night length", SandboxFieldType.Int, CatTime, null, "2", IntMin: 1, IntMax: 6),
        new("TimeSinceApo", "Time since apocalypse", SandboxFieldType.Int, CatTime, null, "1", IntMin: 1, IntMax: 12),

        // ---- Weather ----
        new("Temperature", "Temperature", SandboxFieldType.Int, CatWeather, null, "3", IntMin: 1, IntMax: 5),
        new("Rain", "Rain frequency", SandboxFieldType.Int, CatWeather, null, "3", IntMin: 1, IntMax: 5),
        new("ErosionSpeed", "Erosion speed", SandboxFieldType.Int, CatWeather, null, "3", IntMin: 1, IntMax: 7),
        new("ErosionDays", "Erosion days (0 = default)", SandboxFieldType.Int, CatWeather, null, "0", IntMin: 0, IntMax: 36500),
        new("EnableSnowOnGround", "Snow accumulates", SandboxFieldType.Bool, CatWeather, null, "true"),
        new("MaxFogIntensity", "Max fog intensity", SandboxFieldType.Int, CatWeather, null, "1", IntMin: 0, IntMax: 1),
        new("MaxRainFxIntensity", "Max rain FX intensity", SandboxFieldType.Int, CatWeather, null, "1", IntMin: 0, IntMax: 1),

        // ---- Vehicles ----
        new("EnableVehicles", "Vehicles enabled", SandboxFieldType.Bool, CatVehicles, null, "true"),
        new("EnableSpecificVehicles", "Spawn vehicle stories", SandboxFieldType.Bool, CatVehicles, null, "true"),
        new("CarSpawnRate", "Car spawn rate", SandboxFieldType.Int, CatVehicles, null, "3", IntMin: 1, IntMax: 5),
        new("ChanceHasGasoline", "Chance has gas", SandboxFieldType.Int, CatVehicles, null, "3", IntMin: 1, IntMax: 6),
        new("InitialGasoline", "Initial gasoline", SandboxFieldType.Int, CatVehicles, null, "3", IntMin: 1, IntMax: 6),
        new("FuelStationGasoline", "Fuel-station gasoline", SandboxFieldType.Int, CatVehicles, null, "3", IntMin: 1, IntMax: 6),
        new("CarGasConsumption", "Gas consumption", SandboxFieldType.Float, CatVehicles, null, "1.0"),
        new("LockedCar", "Locked-car frequency", SandboxFieldType.Int, CatVehicles, null, "4", IntMin: 1, IntMax: 6),
        new("CarGeneralCondition", "General condition", SandboxFieldType.Int, CatVehicles, null, "3", IntMin: 1, IntMax: 5),
        new("CarDamageOnImpact", "Damage on impact", SandboxFieldType.Int, CatVehicles, null, "3", IntMin: 1, IntMax: 5),
        new("DamageToPlayerFromHitByACar", "Hit-by-car damage to player", SandboxFieldType.Int, CatVehicles, null, "1", IntMin: 1, IntMax: 4),
        new("TrafficJam", "Traffic jams", SandboxFieldType.Bool, CatVehicles, null, "true"),
        new("CarAlarm", "Car alarm rarity", SandboxFieldType.Int, CatVehicles, null, "4", IntMin: 1, IntMax: 6),
        new("PlayerDamageFromCrash", "Player damage from crash", SandboxFieldType.Bool, CatVehicles, null, "true"),
        new("SirenShutoffHours", "Siren shutoff (hrs)", SandboxFieldType.Float, CatVehicles, null, "0.0"),

        // ---- Character ----
        new("XpMultiplier", "XP multiplier", SandboxFieldType.Float, CatCharacter, null, "1.0"),
        new("CharacterFreePoints", "Free points at creation", SandboxFieldType.Int, CatCharacter, null, "5", IntMin: 0, IntMax: 100),
        new("ConstructionBonusPoints", "Construction bonus", SandboxFieldType.Int, CatCharacter, null, "3", IntMin: 1, IntMax: 5),
        new("BoredomDecreaseRate", "Boredom decrease rate", SandboxFieldType.Float, CatCharacter, null, "1.0"),
        new("RecoveredEnergyOnRest", "Energy recovered on rest", SandboxFieldType.Int, CatCharacter, null, "2", IntMin: 1, IntMax: 5),
        new("InjurySeverity", "Injury severity", SandboxFieldType.Int, CatCharacter, null, "2", IntMin: 1, IntMax: 3),
        new("BoneFracture", "Bone fractures", SandboxFieldType.Bool, CatCharacter, null, "true"),
        new("BloodLevel", "Blood / wound visuals", SandboxFieldType.Int, CatCharacter, null, "3", IntMin: 1, IntMax: 5),
        new("ClothingDegradation", "Clothing degradation", SandboxFieldType.Int, CatCharacter, null, "3", IntMin: 1, IntMax: 5),
        new("RearVulnerability", "Rear-attack vulnerability", SandboxFieldType.Int, CatCharacter, null, "3", IntMin: 1, IntMax: 5),
        new("AttackBlockMovements", "Attacks block movement", SandboxFieldType.Bool, CatCharacter, null, "true"),
        new("AllClothesUnlocked", "All clothes unlocked", SandboxFieldType.Bool, CatCharacter, null, "false"),
        new("StarterKit", "Spawn with starter kit", SandboxFieldType.Bool, CatCharacter, null, "false"),
        new("MultiHitZombies", "Multi-hit zombies", SandboxFieldType.Bool, CatCharacter, null, "false"),
        new("LineOfSightModifier", "Line-of-sight modifier", SandboxFieldType.Float, CatCharacter, null, "1.0"),

        // ---- World events ----
        new("Helicopter", "Helicopter event", SandboxFieldType.IntChoices, CatWorld, null, "2",
            Choices: Cs((1, "Never"), (2, "Sometimes"), (3, "Often"))),
        new("MetaEvent", "Meta event", SandboxFieldType.Int, CatWorld, null, "2", IntMin: 1, IntMax: 4),
        new("SleepingEvent", "Sleeping event", SandboxFieldType.Int, CatWorld, null, "1", IntMin: 1, IntMax: 4),
        new("GeneratorSpawning", "Generator spawning", SandboxFieldType.Int, CatWorld, null, "3", IntMin: 1, IntMax: 5),
        new("GeneratorFuelConsumption", "Generator fuel consumption", SandboxFieldType.Float, CatWorld, null, "1.0"),
        new("SurvivorHouseChance", "Survivor house chance", SandboxFieldType.Int, CatWorld, null, "3", IntMin: 1, IntMax: 4),
        new("VehicleStoryChance", "Vehicle story chance", SandboxFieldType.Int, CatWorld, null, "3", IntMin: 1, IntMax: 5),
        new("ZoneStoryChance", "Zone story chance", SandboxFieldType.Int, CatWorld, null, "3", IntMin: 1, IntMax: 5),
        new("AnnotatedMapChance", "Annotated map chance", SandboxFieldType.Int, CatWorld, null, "4", IntMin: 1, IntMax: 5),
        new("Alarm", "House alarm frequency", SandboxFieldType.Int, CatWorld, null, "4", IntMin: 1, IntMax: 6),
        new("LockedHouses", "Locked-house frequency", SandboxFieldType.Int, CatWorld, null, "6", IntMin: 1, IntMax: 6),
        new("AllowExteriorGenerator", "Allow exterior generators", SandboxFieldType.Bool, CatWorld, null, "true"),
        new("FireSpread", "Fire spread", SandboxFieldType.Bool, CatWorld, null, "true"),

        // ---- Misc ----
        new("WaterShut", "Water shutoff", SandboxFieldType.Int, CatMisc, null, "2", IntMin: 1, IntMax: 8),
        new("ElecShut", "Electricity shutoff", SandboxFieldType.Int, CatMisc, null, "2", IntMin: 1, IntMax: 8),
        new("WaterShutModifier", "Water shutoff modifier (days)", SandboxFieldType.Int, CatMisc, null, "14", IntMin: -1, IntMax: 36500),
        new("ElecShutModifier", "Electricity shutoff modifier (days)", SandboxFieldType.Int, CatMisc, null, "14", IntMin: -1, IntMax: 36500),
        new("HoursForCorpseRemoval", "Hours for corpse removal", SandboxFieldType.Int, CatMisc, null, "216", IntMin: -1, IntMax: 36500),
        new("DecayingCorpseHealthImpact", "Decaying corpse health impact", SandboxFieldType.Int, CatMisc, null, "3", IntMin: 1, IntMax: 5),
        new("ZombieAttractionMultiplier", "Zombie attraction multiplier", SandboxFieldType.Float, CatMisc, null, "1.0"),
        new("LightSource", "Light source rarity", SandboxFieldType.Int, CatMisc, null, "3", IntMin: 1, IntMax: 5),
        new("Battery", "Battery rarity", SandboxFieldType.Int, CatMisc, null, "3", IntMin: 1, IntMax: 5),
        new("SnowOnBlockedRoad", "Snow on blocked road", SandboxFieldType.Bool, CatMisc, null, "false"),
        new("KeepLogList", "Keep log list", SandboxFieldType.Bool, CatMisc, null, "false"),
    };

    public static readonly IReadOnlyDictionary<string, SandboxFieldDef> ByPath =
        AllFields.ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<SandboxFieldDef> InCategory(string category)
        => AllFields.Where(f => f.Category == category);
}
