using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class LeavesRevealer : SerializedMonoBehaviour
{
    [OdinSerialize] private List<List<GameObject>> _leavesLayers;
    [SerializeField] private Transform leafParent;
    private int _index;

    private void Start()
    {
        if (TutorialManager.Instance.withTutorial) leafParent.gameObject.SetActive(true);
    }

    public void Reveal()
    {
        if (_index >= _leavesLayers.Count) return;

        Tween tween = null;
        if (_index < _leavesLayers.Count - 1)
        {
            foreach (var leaf in _leavesLayers[_index])
            {
                tween = leaf.transform.DOMove(leaf.transform.position - leaf.transform.forward * 1f, 1f);
                leaf.transform.DOShakeRotation(0.5f, 1f, 10, 90f);
                AudioManager.instance.Play("ClickLeaves",0.05f);
            }
        }
        else
        {
            foreach (var leaf in _leavesLayers[0].Concat(_leavesLayers[1]).Concat(_leavesLayers[2]))
            {
                tween = leaf.transform.DOMove(leaf.transform.position - leaf.transform.forward * 20f, 1f);
                AudioManager.instance.Play("LeavesSeparate",0.1f);
            }
        }

        _index++;

        if (_index >= _leavesLayers.Count)
        {
            tween.onComplete += () =>
            {
                TutorialManager.Instance?.endCurrentEvent.Invoke("RevealLeaves");
                Destroy(gameObject);
                Destroy(leafParent.gameObject);
            };
        }
    }
}