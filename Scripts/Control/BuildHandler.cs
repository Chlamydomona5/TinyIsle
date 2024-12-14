using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class BuildHandler : MonoBehaviour
{
   [HideInInspector] public UnityEvent<bool> onCurrentBuildEnd = new();

   [SerializeField] private float liftHeight = 2;
   [SerializeField] private float timerTime = .75f;

   [SerializeField] TouchHandler touchHandler;
   [SerializeField] private GridSelector buildSelector;

   [SerializeField] private Canvas canvas;
   [SerializeField] private Camera mainCamera;
   [SerializeField] private PopMenuPanel popMenuPanel;
   [SerializeField] private ProgressBarUI progressBarInstance;
   [SerializeField] private GameObject expandArrow;
   [SerializeField] private List<GameObject> expandPreviewList;
   [SerializeField] private ProducePreview producePreview;
   
   [SerializeField, ReadOnly] private Unit_SO currentProduceUnitSoToBuild;
   [SerializeField, ReadOnly] private UnitEntity currentProduceUnitEntityToMove;
   
   [SerializeField] private Vector2Int longPressOffset;

   private Vector3 _moveUnitOriginalPos;
   private Coroutine _currentTimer;
   private bool _timerFinished;
   private bool _tutorialCrystalMovable = true;
   
   private CrystalStack movingCrystalStack;
   
   private Vector2Int _moveCrystalStackOriginalCoord;


   private void Start()
   {
      var sTransform = new GameObject("movingCrystalStack");
      sTransform.transform.SetParent(transform);
      sTransform.transform.position = Vector3.zero;
      movingCrystalStack = new CrystalStack(Vector2Int.zero, sTransform.transform);
   }

   //TODO: Build Mode can be removed since it's not used
   public void OnClickBuildTap(Unit_SO unitSo)
   {
      currentProduceUnitSoToBuild = unitSo;
      touchHandler.touchState = TouchState.Build;
   }

   public void StartBuild(Vector2Int coord)
   {
      buildSelector.gameObject.SetActive(true);
      buildSelector.AdjustWithUnit(currentProduceUnitSoToBuild);
      AdjustSelector_Build(coord);
   }

   public void BuildSelect(Vector2Int coord)
   {
      AdjustSelector_Build(coord);
   }

   /// <summary>
   /// 尝试在指定位置建造建筑
   /// </summary>
   public bool TryBuildOn(Vector2Int coord)
   {
      //建造
      bool ret = GridManager.Instance.BuildUnit(currentProduceUnitSoToBuild, coord);
      buildSelector.gameObject.SetActive(false);

      onCurrentBuildEnd.Invoke(ret);
      onCurrentBuildEnd.RemoveAllListeners();

      return ret;
   }

   private void AdjustSelector_Build(Vector2Int coord)
   {
      buildSelector.transform.position = GridManager.Instance.Coord2Pos(coord);
      bool canBuild = GridManager.Instance.CanBuildEntity(currentProduceUnitSoToBuild.coveredCoords, coord);
      buildSelector.ChangeMode(canBuild);
   }

   private void AdjustSelector_Move(Vector2Int coord, bool canMove)
   {
      buildSelector.transform.position = GridManager.Instance.Coord2Pos(coord);
      buildSelector.ChangeMode(canMove);
   }

   public void StartCrystalMove(CrystalStack crystalStack, Vector2Int coord)
   {
      if (!_tutorialCrystalMovable)
      {
         UIManager.Instance.CenteredInform("CannotMoveExpandCrystal");
         return;
      }
      
      GridManager.Instance.LockCoord(coord);
      
      touchHandler.touchState = TouchState.Move;
      _moveCrystalStackOriginalCoord = coord;
      
      crystalStack.MoveAllTo(movingCrystalStack);
      movingCrystalStack.Bottom.DOMoveY(movingCrystalStack.Bottom.position.y + liftHeight, .1f);
      
      buildSelector.gameObject.SetActive(true);
      buildSelector.AdjustToNormal();
      AdjustSelector_Move(coord + longPressOffset,GridManager.Instance.crystalController.IsCrystalable(coord + longPressOffset,movingCrystalStack.CurrentType));
   }
   
   public void HoldCrystalMove(Vector2Int coord)
   {
      var offsetCoord = coord + longPressOffset;
      
      var xz = GridManager.Instance.Coord2Pos(offsetCoord);

      movingCrystalStack.Bottom.position = new Vector3(xz.x, movingCrystalStack.Bottom.position.y, xz.z);

      //If expand crystal, show arrow
      if (movingCrystalStack.CurrentType == CrystalType.Expand)
      {
         if (!GridManager.Instance.IsBlockExist(offsetCoord))
         {
            expandArrow.SetActive(true);
            var direction = new Vector3(xz.x, 0, xz.z);

            expandArrow.transform.position = xz + Vector3.up * 3f;
            expandArrow.transform.rotation = Quaternion.LookRotation(direction);  
            
            //Preview
            var list = GridManager.Instance.ExpandToward(coord, movingCrystalStack.Crystals.Peek().Value, false);
            expandPreviewList.ForEach(go => { go.SetActive(false); });
            for (int i = 0; i < list.Count; i++)
            {
               expandPreviewList[i].SetActive(true);
               expandPreviewList[i].transform.position = Methods.YtoZero(GridManager.Instance.Coord2Pos(list[i]));
            }
            
         }
         else expandArrow.SetActive(false);
      }
      
      AdjustSelector_Move(offsetCoord,GridManager.Instance.crystalController.IsCrystalable(offsetCoord,movingCrystalStack.CurrentType));
   }

   public void ReleaseMoveCrystal(Vector2Int coord)
   {
      var offsetCoord = coord + longPressOffset;
      
      touchHandler.touchState = TouchState.Camera;
      
      GridManager.Instance.UnlockCoord();
      
      foreach (var crystal in movingCrystalStack.Crystals)
      {
         //If Is Crystalable, then move
         if (GridManager.Instance.crystalController.IsCrystalable(offsetCoord, crystal.type))
         {
            GridManager.Instance.crystalController.PushCrystalTo(offsetCoord, crystal);
         }
         //Else, move back
         else
            GridManager.Instance.crystalController.PushCrystalTo(_moveCrystalStackOriginalCoord, crystal);
      }
      
      movingCrystalStack.ClearAll();
      
      expandArrow.SetActive(false);
      expandPreviewList.ForEach(go => { go.SetActive(false); });
      ResetMove();
   }

   public bool StartUnitMove(UnitEntity unitEntity)
   {
      unitEntity.transform.DOComplete();

      if(unitEntity.unitSo.groundFixed)
      {
         Debug.Log("Cannot move ground fixed unit: " + unitEntity.name);
         return false;
      }
      touchHandler.touchState = TouchState.Move;
      
      unitEntity.enabled = false;
      
      _moveUnitOriginalPos = unitEntity.transform.position;
      unitEntity.transform.DOMoveY(unitEntity.transform.position.y + liftHeight, .1f);

      buildSelector.gameObject.SetActive(true);
      buildSelector.AdjustWithUnit(unitEntity.unitSo);

      currentProduceUnitEntityToMove = unitEntity;
      AdjustSelector_Move(unitEntity.coordinate,GridManager.Instance.CanMoveTo(currentProduceUnitEntityToMove, unitEntity.coordinate + longPressOffset));
      
      AudioManager.instance.Play("SelectUnit");
      unitEntity.transform.localScale = new Vector3(1, 2f, 1);
      unitEntity.transform.DOScaleY(1f, .2f);
      
      GridManager.Instance.crystalController.SqueezeCrystalAt(unitEntity.RealCoveredCoords);
      
      return true;
   }

   /// <summary>
   /// 玩家拖动建筑时的处理
   /// </summary>
   public void HoldUnitMove(UnitEntity entity, Vector2Int coord)
   {
      var offsetCoord = coord + longPressOffset;
      
      var xz = GridManager.Instance.Coord2Pos(offsetCoord);
      AdjustSelector_Move(offsetCoord,GridManager.Instance.CanMoveTo(currentProduceUnitEntityToMove, offsetCoord));
      var pos = new Vector3(xz.x, entity.transform.position.y, xz.z);
      entity.transform.position = pos;
      
      var unit = GridManager.Instance.FindUnitAt(offsetCoord);
      // If not in range, then sell timer start
      if (unit && unit.GetComponent<CoreUnit>() || !GridManager.Instance.IsBlockExist(offsetCoord))
      {
         TryStartSellTimer(entity);
      }
      else
      {
         StopTimer();
      }
      
      progressBarInstance.GetComponent<RectTransform>().anchoredPosition =
         Methods.World2Canvas(entity.transform.position + entity.modelHeight * Vector3.up / 2f, canvas, mainCamera);

      if (entity is ProduceUnitEntity produceUnitEntity &&
          produceUnitEntity.produceUnitSo.produceType == ProduceType.Interval)
      {
         producePreview.AdjustTo(produceUnitEntity, coord);
      }
   }

   /// <summary>
   /// 玩家放开拖动手指时的处理
   /// </summary>
   public void ReleaseUnitMove(UnitEntity unitEntity, Vector2Int coord)
   {
      unitEntity.transform.DOComplete();

      var offsetCoord = coord + longPressOffset;
      //Sell
      if (_timerFinished)
      {
         unitEntity.Sold();
      }
      //Move
      else
      {
         if (!GridManager.Instance.MoveUnit(unitEntity, offsetCoord))
         {
            unitEntity.transform.DOKill();
            unitEntity.transform.DOMove(_moveUnitOriginalPos, .2f).SetEase(Ease.OutQuad);
         }
         unitEntity.enabled = true;
         
         AudioManager.instance.Play("SelectUnit");
      }
      ResetMove();
      unitEntity.transform.DOScaleY(.25f, .15f).SetLoops(2, LoopType.Yoyo).onComplete += () =>
      {
         unitEntity.transform.localScale = Vector3.one;
      };
   }

   public void ResetMove()
   {
      buildSelector.gameObject.SetActive(false);
      producePreview.gameObject.SetActive(false);
      touchHandler.touchState = TouchState.Camera;
      ResetTimerState();
   }

   private void TryStartSellTimer(UnitEntity follow)
   {
      //If only, then cannot be sold
      if(follow.unitSo.only) return;
      
      if (_currentTimer == null)
      {
         StopTimer();
         _currentTimer = StartCoroutine(Timer(timerTime));
      }
   }

   private void StopTimer()
   {
      ResetTimerState();
      if (_currentTimer != null)
      {
         StopCoroutine(_currentTimer);
         _currentTimer = null;
      }
   }

   private IEnumerator Timer(float time)
   {
      var timer = 0f;
      ResetTimerState();
      progressBarInstance.gameObject.SetActive(true);
      progressBarInstance.GetComponent<RectTransform>();
      while (timer < time)
      {
         timer += Time.fixedDeltaTime;
         //UI
         progressBarInstance.SetTo(timer / time);

         yield return new WaitForFixedUpdate();
      }

      _timerFinished = true;
      progressBarInstance.transform.DOScale(new Vector3(.5f, .5f, .5f), .1f).SetLoops(2, LoopType.Yoyo);
   }

   private void ResetTimerState()
   {
      progressBarInstance.SetTo(0);
      progressBarInstance.gameObject.SetActive(false);

      _timerFinished = false;
   }

   public void TutorialCrystalMovable(bool movable)
   {
      _tutorialCrystalMovable = movable;
   }
}

