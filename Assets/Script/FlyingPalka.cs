
using UnityEngine;

public class FlyingPalka : MonoBehaviour
{
    [Header("Полёт")]
    public float shootSpeed = 10f;

    [Header("Дальность полёта")]
    public float maxShootDistance = 8f;

    private Rigidbody2D rb;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Transform rushka;
    private Transform originalPalka;

    private Vector3 startPosition;

    private bool finished = false;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        startPosition = transform.position;

        if (rb == null)
        {
            Debug.LogError(
                "На FlyingPalka нет Rigidbody2D!"
            );
        }

        // Запускаем палку вверх
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * shootSpeed;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (finished)
            return;

        // Расстояние от места старта
        float distance =
            transform.position.y - startPosition.y;

        // Если улетела слишком далеко
        if (distance >= maxShootDistance)
        {
            Miss();
        }
    }


    // =========================================================
    // ПЕРЕДАЁМ РУЧКУ
    // =========================================================

    public void SetRushka(
        Transform newRushka,
        Transform newOriginalPalka,
        Vector3 originalLocalPosition,
        Quaternion originalLocalRotation)
    {
        rushka = newRushka;
        originalPalka = newOriginalPalka;

        this.originalLocalPosition = originalLocalPosition;
        this.originalLocalRotation = originalLocalRotation;
    }


    // =========================================================
    // СТОЛКНОВЕНИЕ
    // =========================================================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (finished)
            return;


        // =====================================================
        // ПОПАЛИ НЕ В ТОГО
        // =====================================================

        if (collision.collider.CompareTag("NotShoot"))
        {
            WrongTarget();
            return;
        }


        // =====================================================
        // ПОПАЛИ В ПРАВИЛЬНУЮ ЦЕЛЬ
        // =====================================================

        if (collision.collider.CompareTag("Target"))
        {
            HitTarget(collision.collider);
        }
    }


    // =========================================================
    // ПОПАДАНИЕ В NOT SHOOT
    // =========================================================

    void WrongTarget()
    {
        if (finished)
            return;

        finished = true;

        Debug.Log("УБИЛ НЕ ТОГО!");

        // Возвращаем палку
        ReturnNewPalka();
    }


    // =========================================================
    // ПОПАДАНИЕ В TARGET
    // =========================================================

    void HitTarget(Collider2D target)
    {
        if (finished)
            return;

        finished = true;

        Debug.Log("ПОПАДАНИЕ!");

        // Уничтожаем Target
        Destroy(target.gameObject);

        // Создаём новую палку
        ReturnNewPalka();
    }


    // =========================================================
    // ПРОМАХ
    // =========================================================

    void Miss()
    {
        if (finished)
            return;

        finished = true;

        Debug.Log("ПРОМАХ!");

        // Возвращаем палку
        ReturnNewPalka();
    }


    // =========================================================
    // НОВАЯ ПАЛКА
    // =========================================================

    void ReturnNewPalka()
    {
        if (rushka == null)
        {
            Debug.LogError(
                "FlyingPalka не знает, где находится Rushka!"
            );

            Destroy(gameObject);
            return;
        }

        if (originalPalka == null)
        {
            Debug.LogError(
                "FlyingPalka не знает, где находится исходная Palka!"
            );

            Destroy(gameObject);
            return;
        }


        // Находим ArrowController у ArrowRoot
        ArrowController arrowController =
            rushka.GetComponentInParent<ArrowController>();


        // =====================================================
        // ОСТАНАВЛИВАЕМ ЛЕТЯЩУЮ ПАЛКУ
        // =====================================================

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }


        // =====================================================
        // ВОЗВРАЩАЕМ ИСХОДНУЮ ПАЛКУ
        // =====================================================

        originalPalka.gameObject.SetActive(true);

        originalPalka.localPosition =
            originalLocalPosition;

        originalPalka.localRotation =
            originalLocalRotation;


        // =====================================================
        // РАЗРЕШАЕМ СЛЕДУЮЩИЙ ВЫСТРЕЛ
        // =====================================================

        if (arrowController != null)
        {
            arrowController.ReadyForNextShot();
        }
        else
        {
            Debug.LogError(
                "Не найден ArrowController у ArrowRoot!"
            );
        }


        // =====================================================
        // УНИЧТОЖАЕМ ЛЕТЯЩУЮ ПАЛКУ
        // =====================================================

        Destroy(gameObject);


        Debug.Log(
            "Palka возвращена! ArrowRoot снова двигается!"
        );
    }
}

