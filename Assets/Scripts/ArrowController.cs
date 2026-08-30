using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowController : MonoBehaviour
{
    [Header("Движение стрелы")]
    public float moveSpeed = 6f;
    public float leftLimit = -5f;
    public float rightLimit = 5f;

    [Header("Полёт стрелы")]
    public float shootSpeed = 10f;
    public float returnSpeed = 8f;

    [Header("Дальность полёта")]
    public float maxShootDistance = 8f;

    private int direction = 1;

    private Rigidbody2D rb;

    private Vector3 startPosition;

    private bool isShooting = false;
    private bool isReturning = false;

    // Позиция, с которой начался текущий выстрел
    private float shootStartY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Запоминаем начальную позицию стрелы
        startPosition = transform.position;
    }

    void Update()
    {
        // Если стрела стоит на месте и готова к выстрелу
        if (!isShooting && !isReturning)
        {
            // Нажали Space
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

        // Полёт стрелы
        if (isShooting)
        {
            Shoot();
        }

        // Возвращение стрелы
        if (isReturning)
        {
            ReturnToStart();
        }
    }

    // =========================================
    // НАЧАЛО ВЫСТРЕЛА
    // =========================================

    void StartShooting()
    {
        isShooting = true;

        // Запоминаем высоту, с которой стрела начала лететь
        shootStartY = rb.position.y;

        Debug.Log("СТРЕЛА ВЫПУЩЕНА!");
    }

    // =========================================
    // ДВИЖЕНИЕ ВЛЕВО / ВПРАВО
    // =========================================

    void MoveLeftRight()
    {
        Vector2 movement =
            Vector2.right *
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        if (rb.position.x >= rightLimit)
        {
            direction = -1;
        }

        if (rb.position.x <= leftLimit)
        {
            direction = 1;
        }
    }

    // =========================================
    // ПОЛЁТ СТРЕЛЫ
    // =========================================

    void Shoot()
    {
        Vector2 movement =
            Vector2.up *
            shootSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        // Проверяем, не пролетела ли стрела
        // максимальную разрешённую дистанцию
        float distance = rb.position.y - shootStartY;

        if (distance >= maxShootDistance)
        {
            Debug.Log("ПРОМАХ!");

            StartReturning();
        }
    }

    // =========================================
    // НАЧАЛО ВОЗВРАЩЕНИЯ
    // =========================================

    void StartReturning()
    {
        isShooting = false;
        isReturning = true;
    }

    // =========================================
    // ВОЗВРАЩЕНИЕ СТРЕЛЫ
    // =========================================

    void ReturnToStart()
    {
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            startPosition,
            returnSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        // Проверяем, вернулась ли стрела
        if (Vector2.Distance(rb.position, startPosition) < 0.05f)
        {
            rb.position = startPosition;

            isReturning = false;
            isShooting = false;

            Debug.Log("СТРЕЛА ГОТОВА К НОВОМУ ВЫСТРЕЛУ!");
        }
    }

    // =========================================
    // ПОПАДАНИЕ В МИШЕНЬ
    // =========================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isShooting)
            return;

        if (other.CompareTag("Target"))
        {
            Debug.Log("ПОПАДАНИЕ!");

            // Ищем скрипт мишени
            TargetController target =
                other.GetComponent<TargetController>();

            if (target != null)
            {
                // Запускаем эффект исчезновения
                target.Hit();
            }
            else
            {
                // Если скрипта нет — просто удаляем объект
                Destroy(other.gameObject);
            }

            // После попадания стрела начинает возвращаться
            StartReturning();
        }
    }
}
