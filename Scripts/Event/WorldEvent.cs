
using System.Collections.Generic;

public class WorldEvent
{
    public WorldEvent(string title, string description, List<WorldEventChoice> choices)
    {
        Title = title;
        Description = description;
        Choices = choices;
    }
    
    public string Title;
    public string Description;
    public List<WorldEventChoice> Choices;
}