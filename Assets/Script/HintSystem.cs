using UnityEngine;
using UnityEngine.UI;

public class HintSystem : MonoBehaviour
{
    [Header("Первый ценный предмет")]
    [SerializeField] private DraggableItem firstValuableItem;

    [Header("Стрелка")]
    [SerializeField] private GameObject hintArrow;

    [Header("Анимация")]
    [SerializeField] private float pulseSpeed = 2f;

    [SerializeField] private float pulseAmount = 0.08f;

    private Vector3 originalScale;
    private bool hintFinished;

    private void Start()
    {
        if (hintArrow != null)
        {
            hintArrow.SetActive(true);
        }

        if (firstValuableItem != null)
        {
            originalScale = firstValuableItem.transform.localScale;
        }
    }

    private void Update()
    {
        // Destroy() превращает ссылку в null,
        // поэтому таким способом можно определить,
        // что первый предмет уже забран.
        if (firstValuableItem == null)
        {
            HideHint();
            return;
        }

        if (hintFinished)
            return;

        // Пульсация предмета.
        float pulse =
            1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        firstValuableItem.transform.localScale =
            originalScale * pulse;
    }

    private void HideHint()
    {
        if (hintFinished)
            return;

        hintFinished = true;

        if (hintArrow != null)
            hintArrow.SetActive(false);
    }
}