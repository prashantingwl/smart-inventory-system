namespace SmartInventory.Core.Entities;

/// <summary>
/// Temperature zones for cold-chain inventory.
/// Exact ranges come from product specs, but these are the conventional bands.
/// </summary>
public enum TemperatureZone
{
    Frozen,   // ~ -25°C to -10°C
    Chilled,  // ~ 0°C to 8°C
    Ambient   // ~ 13°C to 25°C
}