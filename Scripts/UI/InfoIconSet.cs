using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "InfoIconSet", menuName = "SO/InfoIconSet")]
public class InfoIconSet : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<string, Sprite> IconDict = new Dictionary<string, Sprite>();
    
    [Button]
    private void LoadIcons()
    {
        IconDict.Clear();
        var icons = Resources.LoadAll<Sprite>("InfoIcons");
        foreach (var icon in icons)
        {
            IconDict[icon.name] = icon;
        }
    }
    
    public Sprite GetIcon(string iconId)
    {
        var icon = IconDict.TryGetValue(iconId, out var result) ? result : null;
        if(!icon) Debug.LogError("Icon not found: " + iconId);
        return icon;
    }
}