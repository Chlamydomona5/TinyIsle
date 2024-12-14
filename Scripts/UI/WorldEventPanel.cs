using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WorldEventPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform buttonParent;
    
    [SerializeField] private EventChoiceButton choiceButtonPrefab;


    public void LoadEvent(WorldEvent worldEvent)
    {
        gameObject.SetActive(true);
        
        titleText.text = worldEvent.Title;
        descriptionText.text = worldEvent.Description;

        foreach (WorldEventChoice choice in worldEvent.Choices)
        {
            EventChoiceButton button = Instantiate(choiceButtonPrefab, buttonParent);
            button.LoadChoice(choice, this);
        }
    }

    public void Clear()
    {
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void CloseEvent()
    {
        Clear();
        gameObject.SetActive(false);
    }
}