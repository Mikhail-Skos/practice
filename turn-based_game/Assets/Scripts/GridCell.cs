using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridCell : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int gridPosition;
    public Hero occupiedHero;
    
    private Image cellImage;
    private Color defaultColor;

    private void Awake()
    {
        cellImage = GetComponent<Image>();
        if (cellImage != null)
            defaultColor = cellImage.color;
    }

    public void Highlight(Color color)
    {
        if (cellImage != null)
            cellImage.color = color;
    }

    public void ResetHighlight()
    {
        if (cellImage != null)
            cellImage.color = defaultColor;
    }

    public bool IsHighlighted(Color color)
    {
        return cellImage != null && cellImage.color == color;
    }

    public Vector3 GetCenterPosition()
    {
        return transform.position;
    }

    // Обработчик клика по клетке
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ActionSystem.Instance != null)
        {
            ActionSystem.Instance.HandleCellClick(gridPosition);
        }
    }
}