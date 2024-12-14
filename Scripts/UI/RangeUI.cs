using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class RangeUI : SerializedMonoBehaviour
{
    public int size = 5;
    [OdinSerialize] private Image[,] _rangeImages;

    [SerializeField] private Sprite entitySprite;
    [SerializeField] private Sprite rangeSprite;
    
    [Button]
    public void GetChildren()
    {
        _rangeImages = new Image[size, size];
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var x = i % size;
            var y = i / size;
            _rangeImages[x, y] = child.GetComponent<Image>();
        }
    }

    public void Load(List<Vector2Int> entities, List<Vector2Int> range)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Sprite sprite = null;

                _rangeImages[i, j].color = Color.white;
                
                if (entities.Contains(new Vector2Int(i, j))) sprite = entitySprite;
                else if (range.Contains(new Vector2Int(i, j))) sprite = rangeSprite;
                else _rangeImages[i, j].color = Color.clear;

                _rangeImages[i, j].sprite = sprite;
            }
        }
    }
}