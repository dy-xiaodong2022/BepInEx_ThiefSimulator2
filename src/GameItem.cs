using UnityEngine;

namespace BepInEx_ThiefSimulator2;

public class GameItem: IGameItem
{
    public int ID { get; }
    public int ItemValue { get; }
    public string Name { get; }
    
    // create
    public GameItem(ItemID item)
    {
        this.ID = item.item_ID;
        this.ItemValue = (int)item.item_value;
        this.Name = item.item_name;
    }
}