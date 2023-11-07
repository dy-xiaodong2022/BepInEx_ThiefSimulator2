namespace BepInEx_ThiefSimulator2;

public interface IGameItem
{
    int ID { get; }
    int ItemValue { get; }
    string Name { get; }
}