using System;
using System.Collections.Generic;
using DigitalRubyShared;
using Sirenix.OdinInspector;
using UnityEngine;

public class TouchHandler : MonoBehaviour
{
    [SerializeField] BuildHandler buildHandler;
    [SerializeField] private FishController fishController;
    [SerializeField] private LeavesRevealer leavesRevealer;

    private TapGestureRecognizer _tapGesture;
    private TapGestureRecognizer _doubleTapGesture;
    private PanGestureRecognizer _panGesture;
    private ScaleGestureRecognizer _scaleGesture;
    private RotateGestureRecognizer _rotateGesture;
    private LongPressGestureRecognizer _longPressGesture;
    
    private float _pressCoreTimer;

    private bool _isLeftDown;
    private bool _isRightDown;
    
    [ReadOnly] public TouchState touchState = TouchState.Camera;

    public bool pressOnCore;
    public UnitEntity currentMovingUnit;
    public UnitEntity tapSelectUnit;
    
    public CrystalStack CurrentMovingCrystalStack;
    
    [SerializeField] private Texture2D cursorUpTexture;
    [SerializeField] private Texture2D cursorDownTexture;
    
    [SerializeField] private StatButton statButton;

    
    #region Unity Callback

    private void Start()
    {
        FingersScript.Instance.ComponentTypesToDenyPassThrough.Add(typeof(UnityEngine.UI.Image));
        if(TutorialManager.Instance.withTutorial)
            CreateTutorialTapGesture();
        else Init();
    }

    public void Init()
    {
        FingersScript.Instance.RemoveGesture(_tapGesture);
        
        CreateTapGesture();
        CreateDoubleTapGesture();

        CreateScaleGesture();
        CreatePanGesture();
        
        CreateLongPressGesture();
        
        GridManager.Instance.onUnitBreakDestroy.AddListener(DestroyInterrupt);
        GridManager.Instance.onUnitSoldDestroy.AddListener(DestroyInterrupt);
        
    }
    
    #endregion

    #region CreateGesture

    private void CreateScaleGesture()
    { 
        _scaleGesture = new ScaleGestureRecognizer();
        _scaleGesture.StateUpdated += ScaleGestureCallback;
        FingersScript.Instance.AddGesture(_scaleGesture);
    }
    
    private void CreatePanGesture()
    {
        _panGesture = new PanGestureRecognizer();
        _panGesture.MinimumNumberOfTouchesToTrack = 1;
        _panGesture.StateUpdated += PanGestureCallback;
        FingersScript.Instance.AddGesture(_panGesture);
    }
    
    private void CreateTapGesture()
    {
        _tapGesture = new TapGestureRecognizer();
        _tapGesture.ThresholdSeconds = 0.3f;
        _tapGesture.StateUpdated += TapGestureCallback;
        FingersScript.Instance.AddGesture(_tapGesture);
    }

    private void CreateTutorialTapGesture()
    {
        _tapGesture = new TapGestureRecognizer();
        _tapGesture.ThresholdSeconds = 0.3f;
        _tapGesture.StateUpdated += TutorialTapCallback;
        FingersScript.Instance.AddGesture(_tapGesture);
    }



    private void CreateDoubleTapGesture()
    {
        _doubleTapGesture = new TapGestureRecognizer();
        _doubleTapGesture.NumberOfTapsRequired = 2;
        _doubleTapGesture.SequentialTapThresholdSeconds = .15f;
        _doubleTapGesture.StateUpdated += DoubleTapGestureCallback;
        FingersScript.Instance.AddGesture(_doubleTapGesture);
    }

    private void CreateLongPressGesture()
    {
        _longPressGesture = new LongPressGestureRecognizer();
        //Allow A Pan after a LongPress
        //_longPressGesture.AllowSimultaneousExecution(_panGesture);
        _longPressGesture.MaximumNumberOfTouchesToTrack = 1;
        _longPressGesture.MinimumDurationSeconds = 0.15f;
        _longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(_longPressGesture);
    }

    #endregion

    #region GestureCallback
    
    private void TutorialTapCallback(GestureRecognizer gesture)
    {
        if(gesture.State == GestureRecognizerState.Ended)
            leavesRevealer.Reveal();
    }

    private void LongPressGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Began)
        {
            var unit = GetTouchUnit(gesture);

            if (unit && unit.GetComponent<CoreUnit>())
            {
                _pressCoreTimer = 0f;
                pressOnCore = true;
            }
            else
            {
                if (GetTouchCoordinate(gesture) is var coord && (GridManager.Instance.crystalController.HasCrystal(coord, CrystalType.Expand | CrystalType.Unlock)))
                {
                    CurrentMovingCrystalStack = GridManager.Instance.crystalController.PopCrystalStack(coord);
                    buildHandler.StartCrystalMove(CurrentMovingCrystalStack, coord);
                }
                else if(unit)
                {
                    currentMovingUnit = unit;
                    if (currentMovingUnit)
                    {
                        if (!buildHandler.StartUnitMove(currentMovingUnit)) currentMovingUnit = null;
                        //Close select
                        UIManager.Instance.CloseUI();
                    }
                }

            }
        }

        if (pressOnCore && Mathf.Abs(gesture.DeltaX) < 10f && Mathf.Abs(gesture.DeltaY) < 10f)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                //Accumulate Time
                _pressCoreTimer += Time.deltaTime;
            
                if(GetTouchUnit(gesture) is var unit && unit && unit.GetComponent<CoreUnit>() is var core && core)
                { 
                    if(core.Press(_pressCoreTimer)) _longPressGesture.Reset();
                }
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                if (GetTouchUnit(gesture) is var unit && unit && unit.GetComponent<CoreUnit>() is var core && core)
                {
                    //Debug.Log("Interrupt");
                    core.Interrupt();
                }
                pressOnCore = false;
            }
        }
        else
        {
            pressOnCore = false;
        }

        if (currentMovingUnit)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                buildHandler.HoldUnitMove(currentMovingUnit,GetTouchCoordinate(gesture));
            }
            if (gesture.State == GestureRecognizerState.Ended)
            {
                buildHandler.ReleaseUnitMove(currentMovingUnit, GetTouchCoordinate(gesture));
                currentMovingUnit = null;
            }   
        }
        
        if (CurrentMovingCrystalStack != null)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                buildHandler.HoldCrystalMove(GetTouchCoordinate(gesture));
            }
            if (gesture.State == GestureRecognizerState.Ended)
            {
                buildHandler.ReleaseMoveCrystal(GetTouchCoordinate(gesture));
                CurrentMovingCrystalStack = null;
            }   
        }
    }
    
    private void TapGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            GridManager.Instance.onTap.Invoke();
            
            //Tap gesture
            switch (touchState)
            {
                case TouchState.Camera:
                    UIManager.Instance.CloseUI();
                    
                    var unitOnTap = GetTouchUnit(gesture);
                    // Harvest has higher priority
                    if (unitOnTap && unitOnTap is ProduceUnitEntity produceUnitEntity)
                    {
                        if(produceUnitEntity.TryHarvest()) return;
                    }
                    //Try to select unit
                    tapSelectUnit = unitOnTap;

                    if (_isRightDown)
                    {
                        UIManager.Instance.SelectUnit(tapSelectUnit);

                    }
                    else if(_isLeftDown)
                    {
                        //If there is a unit, show info
                        if(tapSelectUnit)
                        {
                            tapSelectUnit.OnTap();
                        }
                    }
                    
                    //If tap on ground, consume crystal and invoke ontap event
                    if (TapOnLayer(gesture,"Ground", out var _))
                    {
                        GridManager.Instance.crystalController.TapCrystalAt(GetTouchCoordinate(gesture));
                        GridManager.Instance.boxController.TapBoxAt(GetTouchCoordinate(gesture));
                    }
                    else if (TapOnLayer(gesture, "Fish", out var hit))
                    {
                        hit.collider.GetComponentInParent<FishSpotEntity>().OnClick();
                    }
                    
                    break;
                case TouchState.Build:
                    buildHandler.TryBuildOn(GetTouchCoordinate(gesture));
                    touchState = TouchState.Camera;
                    break;
                case TouchState.Fish:
                    fishController.OnTap();
                    break;
            }
        }
    }
    
    
    private void DoubleTapGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
        }
    }

    private void ScaleGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            CameraManager.Instance.Zoom((2 - _scaleGesture.ScaleMultiplier));
        }
    }
    
    private void PanGestureCallback(GestureRecognizer gesture)
    { 
        if(!_isRightDown) return;
        
        switch (touchState)
        {
            case TouchState.Camera:
                PanCamera(gesture);
                break;
            case TouchState.Build:
                PanBuild(gesture);
                break;
        }
    }

    #endregion

    #region Interface

    public Ray GetTouchPositionWorldRay(GestureRecognizer gesture)
    {
        Vector3 mousePos = new Vector3(gesture.FocusX, gesture.FocusY, 0f);
        mousePos.z = CameraManager.Instance.cam.nearClipPlane;
            
        Ray ray = CameraManager.Instance.cam.ScreenPointToRay(mousePos);
        //Draw ray
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green);
        return ray;
    }
    
    /*public UnitEntity GetTouchUnit(GestureRecognizer gesture)
    {
        var ray = GetTouchPositionWorldRay(gesture);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Unit")))
        {
            var unit = hit.collider.GetComponentInParent<UnitEntity>();
            if (unit != null)
            {
                return unit;
            }
        }
        return null;
    }*/
    
    public UnitEntity GetTouchUnit(GestureRecognizer gesture)
    {
        Vector2Int coord = GetTouchCoordinate(gesture);
        return GridManager.Instance.FindUnitAt(coord);
    }
    
    public Vector2Int GetTouchCoordinate(GestureRecognizer gesture)
    {
        var ray = GetTouchPositionWorldRay(gesture);
        //Find the cross point of ray and plane at y = 0
        
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
        {
            var crossPoint = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);
            return GridManager.Pos2Coord(crossPoint);
        }
        //Find the coordinate of cross point
        return GridManager.Pos2Coord(hit.transform.position);
    }

    public void TouchForbidden(bool portrait)
    {
        var enable = portrait;
        _tapGesture.Enabled = enable;
        _doubleTapGesture.Enabled = enable;
        _panGesture.Enabled = enable;
        _scaleGesture.Enabled = enable;
        _longPressGesture.Enabled = enable;
    }
    
    #endregion

    private bool TapOnLayer(GestureRecognizer gesture, string layer, out RaycastHit hit)
    {
        var ray = GetTouchPositionWorldRay(gesture);
        return Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask(layer));
    }
    
    private void PanBuild(GestureRecognizer gesture)
    {
        var coord = GetTouchCoordinate(gesture);
        if (gesture.State == GestureRecognizerState.Began)
        {
            buildHandler.StartBuild(coord);
        }
        else if (gesture.State == GestureRecognizerState.Executing)
        {
            buildHandler.BuildSelect(coord);
        }
        else if (gesture.State == GestureRecognizerState.Ended)
        {
            buildHandler.TryBuildOn(coord);
            touchState = TouchState.Camera;
        }
    }

    private void PanCamera(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            var pos = gesture.FocusX * Vector2.right + gesture.FocusY * Vector2.up;
            var prevPos = pos - gesture.DeltaX * Vector2.right - gesture.DeltaY * Vector2.up;
            var distance = CameraManager.Instance.cam.ScreenToWorldPoint(pos) - CameraManager.Instance.cam.ScreenToWorldPoint(prevPos);
            
            CameraManager.Instance.DragMove(-distance);
        }
    }

    private void DestroyInterrupt(UnitEntity entity)
    {
        if(currentMovingUnit == entity)
        {
            buildHandler.ResetMove();
        }
        if(tapSelectUnit == entity)
        {
            UIManager.Instance.CloseUI();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) CameraManager.Instance.Rotate(false);
        if (Input.GetKeyDown(KeyCode.E)) CameraManager.Instance.Rotate(true);
        
        if(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f)
            CameraManager.Instance.Zoom(1 - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 50f);
        
        if(Input.GetMouseButtonDown(0)) Cursor.SetCursor(cursorDownTexture, new Vector2(cursorDownTexture.width * .4f, cursorDownTexture.height * .4f), CursorMode.Auto);
        if(Input.GetMouseButtonUp(0)) Cursor.SetCursor(cursorUpTexture, new Vector2(cursorUpTexture.width * .3f, cursorUpTexture.height * .3f), CursorMode.Auto);
        
        if(Input.GetKeyDown(KeyCode.V)) statButton.OnClick();
        
        // Logic to distinguish left and right click
        if (Input.GetMouseButtonDown(0))
        {
            _isLeftDown = true;
            _isRightDown = false;
        }
        if (Input.GetMouseButtonDown(1))
        {
            _isRightDown = true;
            _isLeftDown = false;
        }
    }
}

public enum TouchState
{
    Camera,
    Build,
    Move,
    Fish
}