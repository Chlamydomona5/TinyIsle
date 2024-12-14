using UnityEngine;

public class GameSaveManager : Singleton<GameSaveManager>
{
    public void SaveCurrent(string saveName)
    {
        // Save global data in managers
        GridManager.Instance.Save();
        PropertyManager.Instance.Save();
        UnlockWebManager.Instance.Save();
        AchievementManager.Instance.Save();
    }
    
    public void Load(string saveName)
    {
        // Load global data in managers
        GridManager.Instance.Load();
        PropertyManager.Instance.Load();
        UnlockWebManager.Instance.Load();
        AchievementManager.Instance.Load();
    }
}