using Unit;
using UnityEngine;
using RectInt = Unit.RectInt;

public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private Transform rangePivot;

    public void AdjustWithRange(RectInt range)
    {
        if (range == null || range.IsZero())
        {
            rangePivot.gameObject.SetActive(false);
            return;
        }

        rangePivot.gameObject.SetActive(true);
        var square = range.GenerateSquare();
        rangePivot.localScale = new Vector3(square.width, 1, square.length);
        rangePivot.localPosition = new Vector3(square.pivot.x, 0, square.pivot.y);
    }
}