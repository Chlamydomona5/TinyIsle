using UnityEngine.Events;

public class WorldEventChoice
{
    public WorldEventChoice(string name, string description, UnityAction effect)
    {
        Name = name;
        Description = description;
        Event = effect;
    }

    public string Name;
    public string Description;

    public UnityAction Event;
}