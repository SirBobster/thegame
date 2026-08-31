using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowController : MonoBehaviour
{
    [Header("Движение палки")]
    public float moveSpeed = 6f;
    public float leftLimit = -5f;
    public float rightLimit = 5f;

    [Header("Ручка")]
    public Transform rushka;

    [Header("Prefab новой палки")]
    public GameObject palkaPrefab;

    [Header("Полёт палки")]
    public float shootSpeed = 10f;

    [Header("Дальность полёта")]
    public float maxShootDistance = 8f;

    private int direction = 1;

    private Rigidbody2D rb;

    private Vector3 startPosition;

    private bool isShooting = false;
    private bool hitTarget = false;

    // Положение ручки в момент выстрела
    private Vector3 rushkaShootPos;

    // Предыдущая позиция палки.
    // Нужна для проверки очень быстрых попаданий.
    private Vector2 previousPosition;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        startPosition = transform.position;
        previousPosition = rb.position;

        if (rushka != null)
        {
            rushkaShootPos = rushka.position;
        }

        if (palkaPrefab == null)
        {
            Debug.LogError(
                "У ПАЛКИ " + gameObject.name +
                " НЕ НАЗНАЧЕН PALKA PREFAB!"
            );
        }
    }


    void Update()
    {
        if (!isShooting)
        {
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                StartShooting();
            }
        }
    }


    void FixedUpdate()
    {
        if (!isShooting)
        {
            MoveLeftRight();
        }
        else
        {
            Shoot();
        }
    }


    // =========================================================
    // ДВИЖЕНИЕ ВЛЕВО / ВПРАВО
    // =========================================================

    void MoveLeftRight()
    {
        float moveX =
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        Vector2 newPosition = rb.position;

        newPosition.x += moveX;

        // ==========================================
        // ЖЁСТКИЕ ГРАНИЦЫ
        // ==========================================

        if (newPosition.x >= rightLimit)
        {
            newPosition.x = rightLimit;
            direction = -1;
        }
        else if (newPosition.x <= leftLimit)
        {
            newPosition.x = leftLimit;
            direction = 1;
        }

        // ==========================================
        // ДВИГАЕМ ПАЛКУ
        // ==========================================

        rb.MovePosition(newPosition);

        // ==========================================
        // ДВИГАЕМ РУЧКУ РОВНО В ТУ ЖЕ X-КООРДИНАТУ
        // ==========================================

        if (rushka != null)
        {
            Vector3 rushkaPosition = rushka.position;

            rushkaPosition.x = newPosition.x;

            rushka.position = rushkaPosition;
        }
    }



    // =========================================================
    // SPACE
    // =========================================================

    void StartShooting()
    {
        isShooting = true;
        hitTarget = false;

        if (rushka != null)
        {
            rushkaShootPos = rushka.position;
        }

        previousPosition = rb.position;

        Debug.Log("СТРЕЛА ВЫПУЩЕНА!");
    }


    // =========================================================
    // ПОЛЁТ ВВЕРХ
    // =========================================================

    void Shoot()
    {
        Vector2 oldPosition = rb.position;

        float moveY =
            shootSpeed *
            Time.fixedDeltaTime;

        Vector2 newPosition =
            oldPosition +
            Vector2.up * moveY;

        // Двигаем палку
        rb.MovePosition(newPosition);

        // Ручка остаётся на месте
        if (rushka != null)
        {
            rushka.position = rushkaShootPos;
        }

        // =====================================================
        // ПРОВЕРКА ПОПАДАНИЯ ПРИ ОЧЕНЬ БОЛЬШОЙ СКОРОСТИ
        // =====================================================

        CheckFastTargetHit(oldPosition, newPosition);

        if (hitTarget)
            return;

        // =====================================================
        // ПРОВЕРКА ДАЛЬНОСТИ
        // =====================================================

        float distance =
            newPosition.y - startPosition.y;

        if (distance >= maxShootDistance)
        {
            Debug.Log("ПРОМАХ!");

            SpawnNewPalka();
        }

        previousPosition = newPosition;
    }


    // =========================================================
    // ПРОВЕРКА TARGET ДАЖЕ ПРИ ОЧЕНЬ БОЛЬШОЙ СКОРОСТИ
    // =========================================================

    void CheckFastTargetHit(Vector2 oldPosition, Vector2 newPosition)
    {
        Vector2 direction = newPosition - oldPosition;
        float distance = direction.magnitude;

        if (distance <= 0f)
            return;

        // Проверяем все коллайдеры на пути палки
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            oldPosition,
            direction.normalized,
            distance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (!hit.collider.CompareTag("Target"))
                continue;

            HitTarget(hit.collider);
            return;
        }
    }



    // =========================================================
    // ПОПАДАНИЕ
    // =========================================================

    void HitTarget(Collider2D target)
    {
        if (hitTarget)
            return;

        if (!isShooting)
            return;

        hitTarget = true;
        isShooting = false;

        Debug.Log("ПОПАДАНИЕ!");

        Destroy(target.gameObject);

        SpawnNewPalka();
    }


    // =========================================================
    // СОЗДАНИЕ НОВОЙ ПАЛКИ
    // =========================================================

    void SpawnNewPalka()
    {
        if (palkaPrefab == null)
        {
            Debug.LogError("PALKA PREFAB НЕ НАЗНАЧЕН!");
            return;
        }

        if (rushka == null)
        {
            Debug.LogError("RUSHKA НЕ НАЗНАЧЕНА!");
            return;
        }

        // Новая палка появляется на ручке
        GameObject newPalka = Instantiate(
            palkaPrefab,
            rushka.position,
            transform.rotation
        );

        Debug.Log(
            "НОВАЯ ПАЛКА СОЗДАНА: " +
            newPalka.name
        );

        ArrowController newController =
            newPalka.GetComponent<ArrowController>();

        if (newController == null)
        {
            Debug.LogError(
                "У НОВОЙ PALKA НЕТ ARROW CONTROLLER!"
            );

            Destroy(newPalka);
            return;
        }

        // Передаём ручку
        newController.SetRushka(rushka);

        // Передаём prefab дальше
        newController.palkaPrefab = palkaPrefab;

        // Удаляем старую палку
        Destroy(gameObject);

        Debug.Log(
            "СТАРАЯ ПАЛКА УДАЛЕНА, " +
            "НОВАЯ ПАЛКА ГОТОВА!"
        );
    }


    // =========================================================
    // ПЕРЕДАЧА РУЧКИ НОВОЙ ПАЛКЕ
    // =========================================================

    public void SetRushka(Transform newRushka)
    {
        rushka = newRushka;

        startPosition = transform.position;

        isShooting = false;
        hitTarget = false;

        if (rushka != null)
        {
            rushkaShootPos = rushka.position;

            // Сразу синхронизируем X
            Vector3 newPosition = transform.position;

            newPosition.x = rushka.position.x;

            transform.position = newPosition;
        }

        previousPosition = transform.position;
    }
}
