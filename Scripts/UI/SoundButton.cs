using CarterGames.Assets.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoundButton : Button
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        AudioManager.instance.Play("UIButton");
    }
}