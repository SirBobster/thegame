using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowController : MonoBehaviour
{
    [Header("Движение ArrowRoot")]
    public float moveSpeed = 6f;
    public float leftLimit = -5f;
    public float rightLimit = 5f;

    [Header("Дочерние объекты")]
    public Transform palka;
    public Transform rushka;
    private Vector3 palkaLocalPosition;
    private Quaternion palkaLocalRotation;

    [Header("Prefab летающей палки")]
    public GameObject palkaPrefab;

    private Rigidbody2D rb;

    private int direction = 1;

    private bool isReady = true;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (palka != null)
        {
            palkaLocalPosition = palka.localPosition;
            palkaLocalRotation = palka.localRotation;
        }


        if (rb == null)
        {
            Debug.LogError(
                "На ArrowRoot нет Rigidbody2D!"
            );
        }

        if (palka == null)
        {
            Debug.LogError(
                "В ArrowController не назначена Palka!"
            );
        }

        if (rushka == null)
        {
            Debug.LogError(
                "В ArrowController не назначена Rushka!"
            );
        }

        if (palkaPrefab == null)
        {
            Debug.LogError(
                "В ArrowController не назначен Palka Prefab!"
            );
        }
    }


    void Update()
    {
        if (!isReady)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartShooting();
        }
    }


    void FixedUpdate()
    {
        if (!isReady)
            return;

        MoveLeftRight();
    }


    // =========================================================
    // ДВИЖЕНИЕ ARROW ROOT
    // =========================================================

    void MoveLeftRight()
    {
        Vector2 newPosition = rb.position;

        newPosition.x +=
            direction *
            moveSpeed *
            Time.fixedDeltaTime;


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


        rb.MovePosition(newPosition);
    }


    // =========================================================
    // SPACE
    // =========================================================

    void StartShooting()
    {
        isReady = false;

        Debug.Log("SPACE → ПАЛКА ВЫПУЩЕНА!");

        if (palka == null)
        {
            Debug.LogError("Palka отсутствует!");
            return;
        }

        if (palkaPrefab == null)
        {
            Debug.LogError("Palka Prefab не назначен!");
            return;
        }


        // Запоминаем позицию палки
        Vector3 spawnPosition = palka.position;

        Quaternion spawnRotation = palka.rotation;


        // Создаём самостоятельную палку
        GameObject flyingPalka = Instantiate(
            palkaPrefab,
            spawnPosition,
            spawnRotation
        );


        // Передаём ручку новой летающей палке
        FlyingPalka flyingScript =
            flyingPalka.GetComponent<FlyingPalka>();


        if (flyingScript != null)
        {
            flyingScript.SetRushka(
                rushka,
                palka,
                palkaLocalPosition,
                palkaLocalRotation
            );

        }

        else
        {
            Debug.LogError(
                "На Palka Prefab отсутствует FlyingPalka!"
            );
        }


        // Прячем палку внутри ArrowRoot
        palka.gameObject.SetActive(false);


        Debug.Log(
            "Старая Palka спрятана. " +
            "Новая Palka летит."
        );
    }
    public void ReadyForNextShot()
    {
        isReady = true;

        Debug.Log("НОВАЯ ПАЛКА ГОТОВА! ДВИЖЕНИЕ ВОЗОБНОВЛЕНО!");
    }

}
