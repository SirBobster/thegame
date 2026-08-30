using UnityEngine;
using System.Collections;

public class TargetController : MonoBehaviour
{
    [Header("Эффект исчезновения")]
    public float disappearTime = 0.35f;
    public float shrinkSpeed = 5f;

    private bool isDestroyed = false;

    public void Hit()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        StartCoroutine(DisappearEffect());
    }

    private IEnumerator DisappearEffect()
    {
        // Запоминаем начальный размер
        Vector3 startScale = transform.localScale;

        float timer = 0f;

        while (timer < disappearTime)
        {
            timer += Time.deltaTime;

            float progress = timer / disappearTime;

            // Уменьшаем объект
            transform.localScale = Vector3.Lerp(
                startScale,
                Vector3.zero,
                progress
            );

            // Дополнительно немного поворачиваем
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);

            yield return null;
        }

        // В конце полностью удаляем мишень
        Destroy(gameObject);
    }
}
