using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : Singleton<TutorialManager>
{
    public bool withTutorial;
    
    [SerializeField] private BuildHandler buildHandler;
    
    [OdinSerialize] private LinkedList<TutorialSection> _tutorialSections = new();
    [SerializeField, ReadOnly] private LinkedListNode<TutorialSection> _currentSection;

    [SerializeField] private List<GameObject> disableOnTutorial = new();
    
    [HideInInspector] public UnityEvent<string> endCurrentEvent = new();
    
    public GuidePointer pointerUI;

    public GameObject unlockButton;
    public GameObject unlockMask;
    public GameObject shopButton;
    public GameObject toolButton;
    public GameObject spiritButton;
    public GameObject achievementButton;
    
    public GameObject resourcePanel;
    public NewItemPanel newItemPanel;
    public TouchHandler touchHandler;

    public override void Awake()
    {
        base.Awake(); 
        if(!withTutorial) return;
        
        PrepareForTutorial();
        _currentSection = _tutorialSections.First;
        StartCurrent();
        endCurrentEvent.AddListener(TryEndCurrent);
    }

    private void StartCurrent()
    {
        StartCoroutine(_currentSection.Value.Start());
    }

    private void EndCurrent()
    {
        _currentSection.Value.End();
        _currentSection = _currentSection.Next;
    }

    private void TryEndCurrent(string id)
    {
        if (_currentSection == null) return;

        if (_currentSection.Value.ID == id)
        {
            Debug.Log($"End {_currentSection.Value.ID}");
            EndCurrent();
            if (_currentSection != null) StartCurrent();
        }
    }

    private void PrepareForTutorial()
    {
        foreach (var go in disableOnTutorial)
        {
            go.SetActive(false);
        }
    }
    
    [Button]
    private void GenerateNode()
    {
        _tutorialSections.AddLast(new Section1RevealLeaves());
        _tutorialSections.AddLast(new Section2Expand());
        _tutorialSections.AddLast(new Section3CollectCrystal());
        _tutorialSections.AddLast(new Section4LongpressAltar());
        _tutorialSections.AddLast(new Section6UnlockWebUnlock());
    }
    
    public void EnableAchievementButton()
    {
        achievementButton.SetActive(true);
    }
    
    public void TutorialCrystalMovable(bool movable)
    {
        buildHandler.TutorialCrystalMovable(movable);
    }
}