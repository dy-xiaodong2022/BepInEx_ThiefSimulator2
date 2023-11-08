using System;
using BepInEx.Configuration;

namespace BepInEx_ThiefSimulator2;

public interface IConfig
{
    ConfigEntry<bool> NoPolice { get; set; }
    ConfigEntry<float> PlayerSpeed { get; set; }
    ConfigEntry<bool> PlayerInfJump { get; set; }
    ConfigEntry<bool> PlayerGhost { get; set; }
    ConfigEntry<bool> ShowItems { get; set; }
    ConfigEntry<bool> ShowNormalItems { get; set; }
    ConfigEntry<bool> ShowGoodItems { get; set; }
    ConfigEntry<bool> ShowValuableItems { get; set; }
    ConfigEntry<bool> ShowVeryValuableItems { get; set; }
    ConfigEntry<bool> TakeItemOverride { get; set; }
    ConfigEntry<bool> KillPerson { get; set; }
    ConfigEntry<bool> LockpickAlwaysSuccess { get; set; }
}