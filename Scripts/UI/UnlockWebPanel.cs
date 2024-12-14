using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnlockWebPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Vector2 initialCenter;
    [SerializeField] private float buttonSize = 100;

    [SerializeField] private TextMeshProUGUI currentResourceText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI buyCostText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private RectTransform webParent;
    [SerializeField] private Image selectedImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private UnlockItemButton unlockItemButtonPrefab;
    [SerializeField] private Image dependencyPrefab;

    [SerializeField] private Sprite buyButtonCanBuy;
    [SerializeField] private Sprite buyButtonCannotBuy;
    [SerializeField] private Sprite confirmButtonCanConfirm;
    [SerializeField] private Sprite confirmButtonCannotConfirm;

    private Dictionary<UnlockItem, UnlockItemButton> _item2button = new();

    public int currentResource;
    public UnlockItemButton currentItem;

    private void FixedUpdate()
    {
        //Update current resource
        currentResource = GridManager.Instance.crystalController.FindAllCrystal(CrystalType.Unlock)
            .Sum(crystal => crystal.count) - UnlockWebManager.Instance.GetCurrentReleaseCost();
        currentResourceText.text = "x" + currentResource.ToString();

        if (currentItem && currentItem.item)
        {
            selectedImage.transform.position = _item2button[currentItem.item].transform.position;
        }


        if (currentItem && currentItem.item && currentItem.item.unlockType == UnlockType.CostExpandCrystal)
        {
            if (!currentItem.item.CanUnlock(currentResource) || !CheckDependency(currentItem.item) || UnlockWebManager.Instance.IsMaxLevel(currentItem.item))
                buyButton.image.sprite = buyButtonCannotBuy;
            else buyButton.image.sprite = buyButtonCanBuy;
        }
        else if (currentItem && currentItem.item && (currentItem.item.unlockType == UnlockType.MiniumAchievementPoint ||
                                                     currentItem.item.unlockType == UnlockType.NeedCertainAchievement))
        {
            if (!currentItem.item.CanUnlock(currentResource) || !CheckDependency(currentItem.item) || UnlockWebManager.Instance.IsMaxLevel(currentItem.item))
                confirmButton.image.sprite = confirmButtonCannotConfirm;
            else confirmButton.image.sprite = confirmButtonCanConfirm;
        }
    }

    private void Awake()
    {
        foreach (var item in UnlockWebManager.Instance.allUnlockItems)
        {
            var button = Instantiate(unlockItemButtonPrefab, webParent);
            //Set position
            button.transform.localPosition = new Vector3(item.coord.x * buttonSize + initialCenter.x, item.coord.y * buttonSize + initialCenter.y, 0);

            button.Init(this, item);
            _item2button.Add(item, button);
        }

        RefreshButton();
    }

    private IEnumerator Start()
    {
        //Execute after layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(webParent);
        yield return null;
        //Draw dependencies
        foreach (var pair in _item2button)
        {
            foreach (var preItem in pair.Key.preItems)
            {
                var preButton = _item2button[preItem];
                if (!preButton) Debug.LogError("PreButton not found: " + preItem.coord);
                var dependency = Instantiate(dependencyPrefab, webParent);
                
                dependency.transform.SetAsFirstSibling();
                var position1 = pair.Value.transform.position;
                var position2 = preButton.transform.position;
                dependency.transform.position = (position1 + position2) / 2;
                dependency.rectTransform.sizeDelta =
                    new Vector2(Vector2.Distance(position1, position2) - pair.Value.GetComponent<RectTransform>().sizeDelta.x, 20);
                dependency.rectTransform.localRotation = Quaternion.Euler(0, 0,
                    Vector2.SignedAngle(Vector2.right, position1 - position2));
            }
        }
    }

    private void OnEnable()
    {
        Unselect();
        RefreshButton();
    }
    
    private void Unselect()
    {
        selectedImage.gameObject.SetActive(false);
        titleText.text = "";
        descriptionText.text = "";
        buyCostText.text = "";
        buyButton.onClick.RemoveAllListeners();
        videoPlayer.clip = null;
        videoPlayer.Stop();
        videoPlayer.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        UnlockWebManager.Instance.ReleaseUnlock();
    }

    public void Select(UnlockItem item)
    {
        Unselect();

        selectedImage.gameObject.SetActive(true);
        selectedImage.transform.position = _item2button[item].transform.position;

        if (_item2button[item].state != UnlockItemButton.State.Unknown)
        {
            titleText.text = item.LocalizeName;
            descriptionText.text = item.Description;
            if(item.Effect is UnlockShopUnit && UnlockWebManager.Instance.IsUnlocked(item) && !UnlockWebManager.Instance.IsMaxLevel(item))
            {
                descriptionText.text = Methods.GetLocalText("Unlock_UpgradePrefix") + "\n" + item.Effect.ExtraDescription(UnlockWebManager.Instance.GetUnlockLevel(item));
            }

            if (item.unlockType == UnlockType.CostExpandCrystal)
            {
                buyButton.gameObject.SetActive(true);
                confirmButton.gameObject.SetActive(false);
                
                buyCostText.text = "x" + item.CostExpandCrystal.ToString();
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => { _item2button[item].OnBuy(); });
            }
            else
            {
                confirmButton.gameObject.SetActive(true);
                buyButton.gameObject.SetActive(false);
                
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => { _item2button[item].OnBuy(); });
            }
            
            var video = Resources.Load<VideoClip>("UnlockVideos/" + item.ID);
            if (video)
            {
                videoPlayer.gameObject.SetActive(true);
                videoPlayer.clip = video;
                videoPlayer.Play();
            }

        }

    }

    public void RefreshButton()
    {
        if (!UnlockWebManager.Instance) return;
        var unlockItems = UnlockWebManager.Instance.allUnlockItems;
        foreach (var item in unlockItems)
        {
            var button = _item2button[item];
            if (!button) continue;

            button.state = UnlockItemButton.State.Locked;
            
            if (UnlockWebManager.Instance.UnlockVisible(item) == false) button.state = UnlockItemButton.State.Unknown;
            else if (UnlockWebManager.Instance.IsMaxLevel(item)) button.state = UnlockItemButton.State.UnlockedMax;
            else if (UnlockWebManager.Instance.IsUnlocked(item)) button.state = UnlockItemButton.State.UnlockedBase;
            //else if (CheckDependency(item)) button.state = UnlockItemButton.State.Locked;
        }
    }

    public bool CheckDependency(UnlockItem item)
    {
        foreach (var preItem in item.preItems)
        {
            if (!UnlockWebManager.Instance.IsUnlocked(preItem)) return false;
        }

        return true;
    }

    public void Open()
    {
        UIManager.Instance.CloseUI();
        gameObject.SetActive(true);
    }
}