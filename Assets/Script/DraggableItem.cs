using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public enum ItemType
    {
        Valuable,
        Trash
    }

    public enum Weight
    {
        Light,
        Heavy
    }

    [Header("Тип предмета")]
    [SerializeField] private ItemType itemType = ItemType.Valuable;
    [SerializeField] private Weight weight = Weight.Light;

    [Header("Перетаскивание")]
    [SerializeField] private float heavyDragSpeed = 0.35f;

    [Header("Состояние")]
    [SerializeField] private bool isUnderDebris;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 dragOffset;

    private bool isDragging;
    private bool wasDroppedSuccessfully;

    public ItemType Type => itemType;
    public Weight ItemWeight => weight;
    public bool IsUnderDebris => isUnderDebris;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isUnderDebris)
            return;

        isDragging = true;
        wasDroppedSuccessfully = false;

        // Запоминаем положение относительно курсора.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        dragOffset = -localPoint;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isUnderDebris)
            return;

        Vector2 targetPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        targetPosition = localPoint + dragOffset;

        if (weight == Weight.Heavy)
        {
            // Тяжёлый предмет медленно догоняет курсор.
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                targetPosition,
                heavyDragSpeed);
        }
        else
        {
            rectTransform.anchoredPosition = targetPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // Если DropZone не принял предмет,
        // возвращаем его на исходное место.
        if (!wasDroppedSuccessfully)
        {
            StartCoroutine(ReturnToStartPosition());
        }
    }

    /// <summary>
    /// Вызывается DropZone при успешном попадании.
    /// </summary>
    public void TryDrop(DropZone zone)
    {
        if (!isDragging || zone == null)
            return;

        if (!zone.Accepts(itemType))
        {
            wasDroppedSuccessfully = false;
            return;
        }

        wasDroppedSuccessfully = true;

        MiniGameManager manager =
            FindAnyObjectByType<MiniGameManager>();

        if (itemType == ItemType.Valuable && manager != null)
        {
            manager.AddSavedValuable();
        }

        Destroy(gameObject);
    }

    private IEnumerator ReturnToStartPosition()
    {
        Vector2 from = rectTransform.anchoredPosition;
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(from, startPosition, t);

            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;
    }

    /// <summary>
    /// Открывает предмет после перемещения балки.
    /// </summary>
    public void UnlockFromDebris()
    {
        isUnderDebris = false;
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}