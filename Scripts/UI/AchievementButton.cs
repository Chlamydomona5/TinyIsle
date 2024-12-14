using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Button button;

    [SerializeField] private Image lockedImage;
    [SerializeField] private Image waitToUnlockImage;
    [SerializeField] private Image unlockedImage;

    public void SetAchievement(Achievement_SO achievementSO, AchievementPanel panel, bool hasBeenChecked)
    {
        title.text = achievementSO.LocalizeName;
        button.onClick.AddListener(() => panel.ShowAchievement(achievementSO));
        
        lockedImage.gameObject.SetActive(false);
        waitToUnlockImage.gameObject.SetActive(false);
        unlockedImage.gameObject.SetActive(false);
        
        if (AchievementManager.Instance.IsAchievementFinished(achievementSO))
        {
            if (hasBeenChecked) unlockedImage.gameObject.SetActive(true);
            else waitToUnlockImage.gameObject.SetActive(true);
        }
        else lockedImage.gameObject.SetActive(true);

    }
}