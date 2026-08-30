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

    [Header("Новая палка")]
    public GameObject palkaPrefab;


    [Header("Полёт палки")]
    public float shootSpeed = 10f;
    public float returnSpeed = 8f;

    [Header("Дальность полёта")]
    public float maxShootDistance = 8f;

    private int direction = 1;
    private Rigidbody2D rb;

    private Vector3 startPosition;
    private bool hitTarget = false;

    private bool isShooting = false;
    private bool isReturning = false;

    // Позиция ручки в момент выстрела
    private Vector3 rushkaShootPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        if (rushka != null)
        {
            rushkaShootPos = rushka.position;
        }
    }

    void Update()
    {
        if (!isShooting && !isReturning)
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
        // Обычное движение влево-вправо
        if (!isShooting && !isReturning)
        {
            MoveLeftRight();
        }

        // Полёт вверх
        if (isShooting)
        {
            Shoot();
        }

        // Возвращение вниз
        if (isReturning)
        {
            ReturnToStart();
        }
    }

    // =========================================================
    // ДВИЖЕНИЕ ВЛЕВО-ВПРАВО
    // =========================================================

    void MoveLeftRight()
    {
        Vector2 movement =
            Vector2.right * direction * moveSpeed * Time.fixedDeltaTime;

        // Двигаем палку
        rb.MovePosition(rb.position + movement);

        // Двигаем ручку вместе с палкой
        if (rushka != null)
        {
            rushka.position += new Vector3(movement.x, 0f, 0f);
        }

        // Границы движения
        if (rb.position.x >= rightLimit)
        {
            direction = -1;
        }

        if (rb.position.x <= leftLimit)
        {
            direction = 1;
        }
    }

    // =========================================================
    // НАЖАЛИ SPACE
    // =========================================================
    void StartShooting()
    {
        isShooting = true;
        hitTarget = false;

        if (rushka != null)
        {
            rushkaShootPos = rushka.position;
        }

        Debug.Log("СТРЕЛА ВЫПУЩЕНА!");
    }


    // =========================================================
    // ПОЛЁТ ВВЕРХ
    // =========================================================

    void Shoot()
    {
        Vector2 movement =
            Vector2.up * shootSpeed * Time.fixedDeltaTime;

        // Палка летит вверх
        rb.MovePosition(rb.position + movement);

        // Ручка остаётся там, где была при выстреле
        if (rushka != null)
        {
            rushka.position = rushkaShootPos;
        }

        float distance =
            rb.position.y - startPosition.y;

        // Долетели до максимальной дистанции
        if (distance >= maxShootDistance)
        {
            Debug.Log("ПРОМАХ!");
            StartReturning();
        }
    }

    // =========================================================
    // НАЧАЛО ВОЗВРАЩЕНИЯ
    // =========================================================

    void StartReturning()
    {
        isShooting = false;
        isReturning = true;
    }

    // =========================================================
    // ВОЗВРАЩЕНИЕ ПАЛКИ
    // =========================================================

    void ReturnToStart()
    {
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            startPosition,
            returnSpeed * Time.fixedDeltaTime
        );

        // Палка возвращается вниз
        rb.MovePosition(newPosition);

        // Ручка всё ещё стоит на месте
        if (rushka != null)
        {
            rushka.position = rushkaShootPos;
        }

        // Палка вернулась
        if (Vector2.Distance(rb.position, startPosition) < 0.05f)
        {
            rb.position = startPosition;

            isReturning = false;
            isShooting = false;

            // =================================================
            // ВАЖНО:
            // После возвращения ставим ручку рядом с палкой
            // =================================================

            if (rushka != null)
            {
                rushka.position = new Vector3(
                    startPosition.x,
                    rushkaShootPos.y,
                    rushkaShootPos.z
                );
            }

            Debug.Log("СТРЕЛА ВЕРНУЛАСЬ! РУЧКА СНОВА ДВИГАЕТСЯ ВМЕСТЕ.");
        }
    }

    // =========================================================
    // ПОПАДАНИЕ В ЦЕЛЬ
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isShooting)
            return;

        if (hitTarget)
            return;

        if (!other.CompareTag("Target"))
            return;

        // Блокируем повторное срабатывание
        hitTarget = true;
        isShooting = false;

        Debug.Log("ПОПАДАНИЕ!");

        // Уничтожаем Target
        Destroy(other.gameObject);

        // Проверяем, есть ли Prefab и Rushka
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

        // Создаём новую палку
        GameObject newPalka = Instantiate(
            palkaPrefab,
            rushka.position,
            transform.rotation
        );

        Debug.Log("НОВАЯ ПАЛКА СОЗДАНА: " + newPalka.name);

        // Получаем контроллер новой палки
        ArrowController newController =
            newPalka.GetComponent<ArrowController>();

        if (newController == null)
        {
            Debug.LogError("У НОВОЙ PALKA НЕТ ARROW CONTROLLER!");
            return;
        }

        // Передаём ручку новой палке
        newController.SetRushka(rushka);

        // Удаляем старую палку
        Destroy(gameObject);
    }



    public void SetRushka(Transform newRushka)
    {
        rushka = newRushka;

        startPosition = transform.position;

        isShooting = false;
        isReturning = false;
        hitTarget = false;

        if (rushka != null)
        {
            rushkaShootPos = rushka.position;
        }
    }



}