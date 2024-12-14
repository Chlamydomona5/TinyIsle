using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class EntityBase : SerializedMonoBehaviour
{
    public Vector2Int coordinate;
    public List<Vector2Int> coveredCoordsRaw;
    public List<Vector2Int> RealCoveredCoords => GridManager.GetCoveredCoords(coordinate, coveredCoordsRaw);
    
    [ShowInInspector, ReadOnly] private SignMode _signMode = SignMode.Sign;
    [SerializeField] private MeshRenderer meshSign;
    private WorldTextUI _worldTextUI;
    
    [OdinSerialize, ReadOnly] private Dictionary<string, bool> _signStatus = new();
    protected UnityEvent OnSignDetect = new UnityEvent();
    
    [ReadOnly] public float modelHeight;

    public virtual void Init(Vector2Int coord, List<Vector2Int> covered, float y = 0)
    {
        coordinate = coord;
        coveredCoordsRaw = covered;
        transform.position = GridManager.Instance.Coord2Pos(coord) + new Vector3(0, y, 0);
        
        _signStatus = Constant.SignMaterials.ToDictionary(pair => pair.Key, pair => false);

        _worldTextUI = UIManager.Instance.GenerateWorldText();
        _worldTextUI.transform.position = transform.position + Vector3.up * modelHeight;
    }

    public virtual void MoveTo(Vector2Int coord, float yOffset = 0, bool triggerFeature = true)
    {
        GridManager.Instance.crystalController.SqueezeCrystalAt(GridManager.GetCoveredCoords(coord, coveredCoordsRaw));
        GridManager.Instance.boxController.SqueezeBoxAt(GridManager.GetCoveredCoords(coord, coveredCoordsRaw)); 
        
        coordinate = coord;
        transform.position = GridManager.Instance.Coord2Pos(coord) + new Vector3(0, yOffset, 0);
    }

    public virtual void OnTap()
    {
        
    }
    
    public void SetSign(string signId, bool status)
    {
        if (!Constant.SignMaterials.ContainsKey(signId))
        {
            Debug.Log("Sign not found: " + signId);
            return;
        }

        _signStatus[signId] = status;
        
        if (_signMode == SignMode.Sign) UpdateSignAsSign();
        else UpdateSignAsText();
    }

    public void SignSwitchToText(bool on)
    {
        _signMode = on ? SignMode.Text : SignMode.Sign;
        
        if (_signMode == SignMode.Sign) UpdateSignAsSign();
        else UpdateSignAsText();
    }

    public void UpdateSignAsSign()
    {
        if (_worldTextUI.gameObject.activeSelf) _worldTextUI.Close();
        
        var active = _signStatus.Values.Any(s => s);
        meshSign.transform.position = transform.position + Vector3.up * modelHeight;
        
        if (meshSign.gameObject.activeSelf && !active)
        {
            meshSign.transform.localScale = Vector3.one;
            meshSign.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => meshSign.gameObject.SetActive(false));
        }
        else if (!meshSign.gameObject.activeSelf && active)
        {
            meshSign.gameObject.SetActive(true);
            meshSign.transform.localScale = Vector3.zero;
            meshSign.transform.DOScale(Vector3.one, 0.2f);
        }
        
        //Set material, Order by the list order
        if (active)
        {
            var propsBlock = new MaterialPropertyBlock();
            meshSign.GetPropertyBlock(propsBlock);
            propsBlock.SetInt("_SerialNumber", Constant.SignMaterials[_signStatus.First(x => x.Value).Key]);
            meshSign.SetPropertyBlock(propsBlock);
        }
    }

    public void UpdateSignAsText()
    {
        if (meshSign.gameObject.activeSelf) meshSign.gameObject.SetActive(false);
        
        var active = _signStatus.Values.Any(s => s);

        if (active)
        {
            _worldTextUI.Open(Methods.GetLocalText(_signStatus.First(x => x.Value).Key), transform.position + Vector3.up *
                (modelHeight + 0.5f));
        }
        else
        {
            _worldTextUI.Close();
        }
        
    }
    
    public IEnumerator SignDetector()
    {
        yield return null;
        while (true)
        {
            OnSignDetect.Invoke();
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    public void GetHeight(GameObject model)
    {
        //modelHeight = the heighest size.y of all children
        modelHeight = 0;
        foreach (var child in model.GetComponentsInChildren<MeshRenderer>())
        {
            if (child.bounds.size.y > modelHeight)
            {
                modelHeight = child.bounds.size.y;
            }
        }
        
        foreach (var child in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (child.bounds.size.y > modelHeight)
            {
                modelHeight = child.bounds.size.y;
            }
        }
    }
}

public enum SignMode
{
    Sign,
    Text    
}