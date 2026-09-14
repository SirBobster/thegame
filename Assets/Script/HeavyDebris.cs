
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeavyDebris : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Настройки")]
    [SerializeField] private float debrisDragSpeed = 0.3f;

    [Header("Предметы под балкой")]
    [SerializeField] private DraggableItem[] hiddenItems;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 dragOffset;

    private bool isDragging;
    private bool wasDroppedSuccessfully;

    private void Start()
    {
        // Пока балка находится на месте, предметы под ней
        // не должны принимать клики мыши.
        if (hiddenItems == null)
            return;

        foreach (DraggableItem item in hiddenItems)
        {
            if (item == null)
                continue;

            CanvasGroup itemCanvasGroup =
                item.GetComponent<CanvasGroup>();

            if (itemCanvasGroup != null)
            {
                itemCanvasGroup.blocksRaycasts = false;
            }
        }
    }

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
        isDragging = true;
        wasDroppedSuccessfully = false;

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
        if (!isDragging)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        Vector2 targetPosition = localPoint + dragOffset;

        // Балка медленно следует за курсором.
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            debrisDragSpeed);

        // Лёгкая случайная тряска для ощущения тяжести.
        Vector2 shake = Random.insideUnitCircle * 2f;
        rectTransform.localPosition += (Vector3)shake;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // Если балка не была успешно помещена в TrashZone,
        // она возвращается на исходное место.
        // Предметы при этом остаются заблокированными.
        // if (!wasDroppedSuccessfully)
        // {
        //     StartCoroutine(ReturnToStartPosition());
        // }
    }

    /// <summary>
    /// Обрабатывает попытку положить балку в DropZone.
    /// </summary>
    public void TryDrop(DropZone zone)
    {
        if (!isDragging || zone == null)
            return;

        // Балку можно убрать только через TrashZone.
        if (zone.Zone != DropZone.ZoneType.Trash)
        {
            wasDroppedSuccessfully = false;
            return;
        }

        wasDroppedSuccessfully = true;

        // Сначала открываем предметы, которые находились под балкой.
        UnlockHiddenItems();

        // После успешного перемещения балка больше не нужна.
        Destroy(gameObject);
    }

    /// <summary>
    /// Разблокирует предметы, находившиеся под балкой.
    /// </summary>
    private void UnlockHiddenItems()
    {
        if (hiddenItems == null)
            return;

        foreach (DraggableItem item in hiddenItems)
        {
            if (item == null)
                continue;

            item.UnlockFromDebris();
        }
    }

    /// <summary>
    /// Плавно возвращает балку на исходную позицию.
    /// Предметы под ней остаются заблокированными.
    /// </summary>
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
}

