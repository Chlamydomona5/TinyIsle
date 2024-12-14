using CarterGames.Assets.AudioManager;
using EPOOutline;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Canvas screenCanvas;
    [SerializeField] private Canvas worldCanvas;
    
    [SerializeField] private ShopPanel shopPanel;
    [SerializeField] private PopMenuPanel popMenuPanel;
    [SerializeField] private WorldEventPanel worldEventPanel;
    [SerializeField] private TipPanel tipPanel;
    [SerializeField] private UnitInfoPanel unitInfoPanel;
    [SerializeField] private SpiritInfoPanel spiritInfoPanel;
    
    [SerializeField] private UnlockWebPanel unlockWebPanel;
    [SerializeField] private CenteredInform centeredInform;
    
    [SerializeField] private ProducePreview producePreview;

    [SerializeField] private Material outlineMaterial;
    private UnitEntity _selectedEntity;
    private Outlinable _selectedOutlinable;

    [SerializeField] private WorldTextUI worldTextUI;
    
    public void WorldEvent(WorldEvent worldEvent)
    {
        worldEventPanel.LoadEvent(worldEvent);
    }

    public void TipViaID(string tip)
    {
        tipPanel.TipViaID(tip);
    }
    
    public void TipStraight(string text)
    {
        tipPanel.TipStraight(text);
    }

    public void SelectUnit(UnitEntity unit)
    {
        if(unit && unit == _selectedEntity) return;
        
        if (_selectedEntity)
        {
            _selectedEntity.ShowLevel(false);
            _selectedEntity.SignSwitchToText(false);
        }
        
        _selectedEntity = unit;
        
        if(!unit) return;

        unit.SignSwitchToText(true);
        SelectOutline(unit);
        DisplayUnitInfo(unit);

        //Show the produce preview
        if (unit is ProduceUnitEntity produceUnitEntity &&
            produceUnitEntity.produceUnitSo.produceType == ProduceType.Interval)
        {
            producePreview.AdjustTo(produceUnitEntity, produceUnitEntity.coordinate);
        }
        else producePreview.gameObject.SetActive(false);

        AudioManager.instance.Play("SelectUnit");

    }

    public void DisplayUnitInfo(UnitEntity entity)
    {
        unitInfoPanel.gameObject.SetActive(true);
        unitInfoPanel.ChangeMode(false);
        unitInfoPanel.LoadInfoEntity(entity);
    }
    
    public void DisplaySpiritInfo(Spirit spirit)
    {
        CloseUI();
        spiritInfoPanel.gameObject.SetActive(true);
        spiritInfoPanel.LoadInfo(spirit);
    }

    public void CloseUI()
    {
        popMenuPanel.Pop(false);
        unitInfoPanel.gameObject.SetActive(false);
        producePreview.gameObject.SetActive(false);
        unlockWebPanel.gameObject.SetActive(false);
        spiritInfoPanel.gameObject.SetActive(false);

        if (_selectedEntity)
        {
            _selectedEntity.ShowLevel(false);
            _selectedEntity.SignSwitchToText(false);
            SelectOutline(null);
            _selectedEntity = null;
        }

    }

    private void SelectOutline(UnitEntity unit)
    {
        if (_selectedOutlinable) Destroy(_selectedOutlinable);
        
        if(!unit) return;
        _selectedOutlinable = unit.gameObject.AddComponent<Outlinable>();
        _selectedOutlinable.AddChildrenMeshRendererWithoutMark();
        _selectedOutlinable.OutlineParameters.Color = Color.white;
    }
    

    public void CenteredInform(string messageID)
    {
        centeredInform.Open(messageID);
    }

    public WorldTextUI GenerateWorldText()
    {
        return Instantiate(worldTextUI, worldCanvas.transform);
    }
}