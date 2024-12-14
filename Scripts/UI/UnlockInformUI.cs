using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UnlockInformUI : MaskBar
{
    [SerializeField] private Image icon;

    private Queue<UnlockItem> waitingItems = new();
    private Sequence _sequence;

    public void TryToInform(UnlockItem newItem)
    {
        waitingItems.Enqueue(newItem);
        
        if (_sequence == null || _sequence.IsComplete())
        {
            StartInform();
        }
    }

    private void StartInform()
    {
        _sequence = HoldFor(3f);
        var item = waitingItems.Dequeue();
        _sequence.OnPlay((() => icon.sprite = item.Icon));
        _sequence.OnComplete(() =>
        {
            if (waitingItems.Count > 0)
            {
                StartInform();
            }
        });
    }

}