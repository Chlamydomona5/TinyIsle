using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventChoiceButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;
    
    public void LoadChoice(WorldEventChoice choice, WorldEventPanel panel)
    {
        nameText.text = choice.Name;
        descriptionText.text = choice.Description;
        button.onClick.AddListener(choice.Event.Invoke);
        button.onClick.AddListener(panel.CloseEvent);
    }
}