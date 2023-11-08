using System;
using BepInEx.Configuration;

namespace BepInEx_ThiefSimulator2;

public class Config : IConfig
{
    public ConfigEntry<bool> NoPolice { get; set; }
    public ConfigEntry<float> PlayerSpeed { get; set; }
    public ConfigEntry<bool> PlayerInfJump { get; set; }
    public ConfigEntry<bool> PlayerGhost { get; set; }
    public ConfigEntry<bool> ShowItems { get; set; }
    public ConfigEntry<bool> ShowNormalItems { get; set; }
    public ConfigEntry<bool> ShowGoodItems { get; set; }
    public ConfigEntry<bool> ShowValuableItems { get; set; }
    public ConfigEntry<bool> ShowVeryValuableItems { get; set; }
    public ConfigEntry<bool> TakeItemOverride { get; set; }
    public ConfigEntry<bool> KillPerson { get; set; }
    public ConfigEntry<bool> LockpickAlwaysSuccess { get; set; }
    
    public Config()
    {
        ConfigFile configFile = new ConfigFile("ThiefSimulator2.ini", true);
        this.NoPolice = configFile.Bind("Game", "NoPolice", true, "Disable police");
        // this.PlayerSpeed = configFile.Bind("Game", "PlayerSpeed", 2f, "Player speed");
        this.PlayerSpeed = configFile.Bind("Game", "PlayerSpeed", 2f, 
            new ConfigDescription("Player Speed", new AcceptableValueRange<float>(1, 10)));
        this.PlayerInfJump = configFile.Bind("Game", "PlayerInfJump", false, "Player infinite jump");
        this.PlayerGhost = configFile.Bind("Game", "PlayerGhost", false, "Player ghost");
        this.ShowItems = configFile.Bind("Game", "ShowItems", true, "Show items");
        this.ShowNormalItems = configFile.Bind("Game", "ShowNormalItems", false, "Show normal items");
        this.ShowGoodItems = configFile.Bind("Game", "ShowGoodItems", false, "Show good items");
        this.ShowValuableItems = configFile.Bind("Game", "ShowValuableItems", true, "Show valuable items");
        this.ShowVeryValuableItems = configFile.Bind("Game", "ShowVeryValuableItems", true, "Show very valuable items");
        this.TakeItemOverride = configFile.Bind("Game", "TakeItemOverride", true, "Take item override");
        this.KillPerson = configFile.Bind("Game", "KillPerson", true, "Kill person");
        this.LockpickAlwaysSuccess = configFile.Bind("Game", "LockpickAlwaysSuccess", true, "Lockpick always success");
    }
    
    public T GetConfigField<T>(string key)
    {
        var propertyInfo = typeof(Config).GetProperty(key);
        if (propertyInfo == null) throw new Exception($"Config field '{key}' not found.");
        if (propertyInfo.GetValue(this) is ConfigEntry<T> configEntry) return configEntry.Value;
        throw new Exception($"Config field '{key}' not found.");
    }

    public void SetConfigField<T>(string key, T value)
    {
        var propertyInfo = typeof(Config).GetProperty(key);
        if (propertyInfo == null) throw new Exception($"Config field '{key}' not found.");
        if (propertyInfo.GetValue(this) is ConfigEntry<T> configEntry) configEntry.Value = value;
        return;
    }
}