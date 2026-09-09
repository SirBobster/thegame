using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DiceMiniGame : MonoBehaviour
{
[Header("Кубики игрока")]
public Image[] playerDiceImages = new Image[3];


[Header("Кубики оппонента")]
public Image[] opponentDiceImages = new Image[3];

[Header("Спрайты кубиков от 1 до 6")]
public Sprite[] diceSprites = new Sprite[6];

[Header("UI")]
public Text playerScoreText;
public Text opponentScoreText;
public Text roundResultText;
public Text finalResultText;
public Text hintText;

[Header("Доска")]
public RectTransform diceBoard;

[Header("Зоны кубиков игрока")]
public RectTransform[] playerDiceZones = new RectTransform[3];

[Header("Зоны кубиков оппонента")]
public RectTransform[] opponentDiceZones = new RectTransform[3];

[Header("Подбор кубиков")]
public float followSpeed = 15f;
public float holdSpread = 40f;
public Color highlightColor = Color.yellow;

[Header("Настройки броска")]
[SerializeField] private float rollDuration = 1.5f;
[SerializeField] private float opponentStartDelay = 0.15f;
[SerializeField] private float delayBetweenDice = 0.10f;
[SerializeField] private float spriteChangeInterval = 0.06f;

[Header("Разделение кубиков")]
[SerializeField] private float diceSeparationDistance = 70f;
[SerializeField] private float separationForce = 5f;

[Header("Движение кубиков")]
[SerializeField] private float movementArcHeight = 100f;
[SerializeField] private float randomMovementRadius = 120f;
[SerializeField] private float rotationSpeed = 720f;

[Header("Отскок при приземлении")]
[SerializeField] private float landingBounceHeight = 35f;
[SerializeField] private float landingBounceDuration = 0.25f;
[SerializeField] private float landingScaleAmount = 0.85f;

[Header("Покачивание кубиков в руке")]
[SerializeField] private float handSwayAmount = 5f;
[SerializeField] private float handSwaySpeed = 4f;
[SerializeField] private float handRotationAmount = 8f;

// Текущий раунд.
private int currentRound = 1;

// Количество выигранных раундов.
private int playerRoundWins = 0;
private int opponentRoundWins = 0;

// Общие очки за все раунды.
private int playerTotalScore = 0;
private int opponentTotalScore = 0;

// Идёт ли сейчас бросок.
private bool isRolling = false;

// Подобран ли конкретный кубик игрока.
private bool[] playerDiceHeld = new bool[3];

// Был ли уже сыгран первый раунд.
private bool gameStarted = false;

// Исходные цвета кубиков.
private Color[] playerOriginalColors;

// Позиции кубиков во время нахождения в руке.
private Vector3[] handTargetPositions = new Vector3[3];

// Случайные параметры покачивания каждого кубика.
private float[] handSwayOffsets = new float[3];

private void Start()
{
    InitializeDice();

    RestartGame();
}

private void Update()
{
    // Проверяем наведение и клик мыши.
    if (!isRolling && currentRound <= 3)
    {
        HandleMouseInput();
    }

    // Кубики, находящиеся в руке, следуют за курсором.
    if (!isRolling)
    {
        UpdateHeldDice();
    }

    // Бросок выполняется только после того,
    // как игрок поднял все три кубика.
    if (Keyboard.current != null &&
        Keyboard.current.spaceKey.wasPressedThisFrame &&
        !isRolling &&
        currentRound <= 3)
    {
        if (AllPlayerDiceHeld())
        {
            StartCoroutine(PlayRound());
        }
        else
        {
            SetHint("Сначала подберите все 3 кубика (ЛКМ)");
        }
    }
}

/// <summary>
/// Инициализация данных кубиков.
/// </summary>
private void InitializeDice()
{
    playerOriginalColors = new Color[3];

    for (int i = 0; i < 3; i++)
    {
        if (playerDiceImages[i] != null)
        {
            playerOriginalColors[i] =
                playerDiceImages[i].color;

            playerDiceImages[i].raycastTarget = true;
        }

        playerDiceHeld[i] = false;
        handSwayOffsets[i] = Random.Range(0f, 10f);
    }

    // У кубиков оппонента отключаем возможность
    // использования их как объектов подбора.
    for (int i = 0; i < opponentDiceImages.Length && i < 3; i++)
    {
        if (opponentDiceImages[i] != null)
        {
            opponentDiceImages[i].raycastTarget = false;
        }
    }
}

/// <summary>
/// Полностью перезапускает игру.
/// </summary>
public void RestartGame()
{
    StopAllCoroutines();

    currentRound = 1;

    playerRoundWins = 0;
    opponentRoundWins = 0;

    playerTotalScore = 0;
    opponentTotalScore = 0;

    isRolling = false;
    gameStarted = false;

    // Все кубики игрока снова доступны.
    for (int i = 0; i < 3; i++)
    {
        playerDiceHeld[i] = false;
    }

    // Сбрасываем UI.
    if (playerScoreText != null)
        playerScoreText.text = "Игрок: 0";

    if (opponentScoreText != null)
        opponentScoreText.text = "Оппонент: 0";

    if (roundResultText != null)
        roundResultText.text =
            "Раунд 1";

    if (finalResultText != null)
        finalResultText.text = "";

    // Первый раунд начинается с зон.
    ResetDicePositions(
        playerDiceImages,
        playerDiceZones
    );

    ResetDicePositions(
        opponentDiceImages,
        opponentDiceZones
    );

    // Начальное значение кубиков.
    SetDiceToValue(playerDiceImages, 1);
    SetDiceToValue(opponentDiceImages, 1);

    RestorePlayerDiceColors();

    SetHint("Подберите кубики (ЛКМ)");
}

/// <summary>
/// Обработка мыши:
/// наведение + подбор кубиков.
/// </summary>
private void HandleMouseInput()
{
    if (Mouse.current == null)
        return;

    Vector2 mousePosition =
        Mouse.current.position.ReadValue();

    int hoveredDice = -1;

    // Ищем кубик игрока под курсором.
    for (int i = 0; i < 3; i++)
    {
        if (playerDiceImages[i] == null)
            continue;

        // Уже взятый кубик повторно подобрать нельзя.
        if (playerDiceHeld[i])
            continue;

        RectTransform diceRect =
            playerDiceImages[i].rectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(
                diceRect,
                mousePosition,
                null))
        {
            hoveredDice = i;
            break;
        }
    }

    // Сбрасываем подсветку всех кубиков.
    RestorePlayerDiceColors();

    // Подсвечиваем кубик под курсором.
    if (hoveredDice >= 0)
    {
        playerDiceImages[hoveredDice].color =
            highlightColor;

        SetHint("Подберите кубик (ЛКМ)");

        // Проверяем клик ЛКМ.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PickUpDice(hoveredDice);
        }
    }
    else
    {
        if (!AllPlayerDiceHeld())
        {
            SetHint("Подберите кубики (ЛКМ)");
        }
    }
}

/// <summary>
/// Подбирает конкретный кубик игрока.
/// </summary>
private void PickUpDice(int index)
{
    if (index < 0 || index >= 3)
        return;

    if (playerDiceHeld[index])
        return;

    playerDiceHeld[index] = true;

    // Делаем кубик визуально выделенным.
    playerDiceImages[index].color =
        highlightColor;

    // Кубик становится поверх остальных UI-объектов.
    playerDiceImages[index].transform.SetAsLastSibling();

    handSwayOffsets[index] =
        Random.Range(0f, 10f);

    // Если собраны все три кубика —
    // меняем подсказку.
    if (AllPlayerDiceHeld())
    {
        SetHint("Все кубики в руке!\nНажмите SPACE для броска");
    }
    else
    {
        SetHint("Подберите остальные кубики (ЛКМ)");
    }
}

/// <summary>
/// Проверяет, подняты ли все три кубика.
/// </summary>
private bool AllPlayerDiceHeld()
{
    for (int i = 0; i < 3; i++)
    {
        if (!playerDiceHeld[i])
            return false;
    }

    return true;
}

/// <summary>
/// Обновляет положение кубиков в руке.
/// Кубики располагаются веером вокруг курсора.
/// </summary>
private void UpdateHeldDice()
{
    if (Mouse.current == null)
        return;

    Vector2 mousePosition = Mouse.current.position.ReadValue();
    Vector3 mouseWorldPosition;

    if (diceBoard != null)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            diceBoard, mousePosition, null, out mouseWorldPosition
        );
    }
    else
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, 0f)
        );
        mouseWorldPosition.z = 0f;
    }

    // Шаг 1: Рассчитываем базовые позиции веером
    Vector3[] basePositions = new Vector3[3];
    
    for (int i = 0; i < 3; i++)
    {
        if (!playerDiceHeld[i])
            continue;

        float spreadOffset = (i - 1) * 20f; // Маленький базовый разброс
        float verticalOffset = Mathf.Abs(i - 1) * -5f;
        
        basePositions[i] = mouseWorldPosition + new Vector3(spreadOffset, verticalOffset, 0f);
    }

    // Шаг 2: Отталкивание кубиков друг от друга
    Vector3[] finalPositions = new Vector3[3];
    
    for (int i = 0; i < 3; i++)
    {
        if (!playerDiceHeld[i])
            continue;
            
        finalPositions[i] = basePositions[i];
        
        // Проверяем столкновения с другими кубиками
        for (int j = 0; j < 3; j++)
        {
            if (i == j || !playerDiceHeld[j])
                continue;
                
            Vector3 direction = basePositions[i] - basePositions[j];
            float distance = direction.magnitude;
            
            // Если кубики слишком близко - отталкиваем
            if (distance < diceSeparationDistance && distance > 0)
            {
                Vector3 pushDirection = direction.normalized;
                float pushForce = (diceSeparationDistance - distance) * separationForce;
                
                finalPositions[i] += pushDirection * pushForce;
            }
        }
    }

    // Шаг 3: Применяем позиции с плавным следованием
    for (int i = 0; i < 3; i++)
    {
        if (!playerDiceHeld[i])
            continue;

        RectTransform diceRect = playerDiceImages[i].rectTransform;
        
        // Плавное движение к позиции
        diceRect.position = Vector3.Lerp(
            diceRect.position,
            finalPositions[i],
            followSpeed * Time.deltaTime
        );

        // Лёгкое покачивание
        float sway = Mathf.Sin(Time.time * handSwaySpeed + handSwayOffsets[i]);
        float rotation = sway * handRotationAmount;
        
        diceRect.rotation = Quaternion.Euler(0f, 0f, rotation);
        diceRect.position += Vector3.up * (sway * handSwayAmount * 0.5f);
    }
}

/// <summary>
/// Основная корутина раунда.
/// </summary>
private IEnumerator PlayRound()
{
    if (!AllPlayerDiceHeld())
        yield break;

    isRolling = true;

    SetHint("Бросок...");

    if (roundResultText != null)
    {
        roundResultText.text =
            "Раунд " + currentRound +
            "\nБросок...";
    }

    if (finalResultText != null)
        finalResultText.text = "";

    // Получаем результаты заранее.
    int[] playerResults =
        GenerateDiceResults();

    int[] opponentResults =
        GenerateDiceResults();

    // Бросаем кубики игрока.
    yield return StartCoroutine(
        RollHeldPlayerDice(
            playerResults
        )
    );

    // Небольшая задержка перед броском оппонента.
    yield return new WaitForSeconds(
        opponentStartDelay
    );

    // Бросаем кубики оппонента.
    yield return StartCoroutine(
        RollOpponentDice(
            opponentResults
        )
    );

    // Считаем суммы.
    int playerRoundScore =
        SumDice(playerResults);

    int opponentRoundScore =
        SumDice(opponentResults);

    playerTotalScore +=
        playerRoundScore;

    opponentTotalScore +=
        opponentRoundScore;

    // Определяем победителя раунда.
    if (playerRoundScore > opponentRoundScore)
    {
        playerRoundWins++;

        if (roundResultText != null)
        {
            roundResultText.text =
                "Раунд " +
                currentRound +
                ": победа игрока!\n" +
                playerRoundScore +
                " : " +
                opponentRoundScore;
        }
    }
    else if (opponentRoundScore > playerRoundScore)
    {
        opponentRoundWins++;

        if (roundResultText != null)
        {
            roundResultText.text =
                "Раунд " +
                currentRound +
                ": победа оппонента!\n" +
                playerRoundScore +
                " : " +
                opponentRoundScore;
        }
    }
    else
    {
        if (roundResultText != null)
        {
            roundResultText.text =
                "Раунд " +
                currentRound +
                ": ничья!\n" +
                playerRoundScore +
                " : " +
                opponentRoundScore;
        }
    }

    UpdateScoreUI();

    isRolling = false;

    // После третьего раунда показываем финальный результат.
    if (currentRound >= 3)
    {
        ShowFinalResult();
    }
    else
    {
        currentRound++;

        // В следующем раунде кубики игрока снова доступны.
        PrepareNextRound();

        if (roundResultText != null)
        {
            roundResultText.text +=
                "\n\nСледующий раунд: " +
                currentRound;
        }

        SetHint("Подберите кубики (ЛКМ)");
    }
}

/// <summary>
/// Подготавливает следующий раунд.
/// Важно: кубики НЕ телепортируются обратно в стартовые зоны.
/// Они остаются там, где упали в предыдущем раунде.
/// </summary>
private void PrepareNextRound()
{
    for (int i = 0; i < 3; i++)
    {
        playerDiceHeld[i] = false;

        if (playerDiceImages[i] != null)
        {
            playerDiceImages[i].color =
                playerOriginalColors[i];

            playerDiceImages[i].transform
                .SetAsLastSibling();
        }
    }

    // Кубики оппонента также остаются на доске.
    RestorePlayerDiceColors();
}

/// <summary>
/// Бросок кубиков игрока из руки.
/// </summary>
private IEnumerator RollHeldPlayerDice(
    int[] finalResults)
{
    Coroutine[] animations =
        new Coroutine[3];

    for (int i = 0; i < 3; i++)
    {
        animations[i] =
            StartCoroutine(
                AnimateThrownDice(
                    playerDiceImages[i],
                    finalResults[i],
                    i * delayBetweenDice
                )
            );
    }

    float waitTime =
        rollDuration +
        delayBetweenDice * 2f +
        landingBounceDuration;

    yield return new WaitForSeconds(
        waitTime
    );

    // После броска кубики больше не находятся в руке.
    for (int i = 0; i < 3; i++)
    {
        playerDiceHeld[i] = false;
    }
}

/// <summary>
/// Бросок кубиков оппонента.
/// Их невозможно подобрать мышью.
/// </summary>
private IEnumerator RollOpponentDice(
    int[] finalResults)
{
    Coroutine[] animations =
        new Coroutine[3];

    for (int i = 0; i < 3; i++)
    {
        if (opponentDiceImages[i] == null)
            continue;

        animations[i] =
            StartCoroutine(
                AnimateThrownDice(
                    opponentDiceImages[i],
                    finalResults[i],
                    i * delayBetweenDice
                )
            );
    }

    float waitTime =
        rollDuration +
        delayBetweenDice * 2f +
        landingBounceDuration;

    yield return new WaitForSeconds(
        waitTime
    );
}

/// <summary>
/// Анимация полёта одного кубика.
/// После завершения кубик остаётся на месте падения.
/// </summary>
private IEnumerator AnimateThrownDice(
    Image diceImage,
    int finalValue,
    float stopDelay)
{
    if (diceImage == null)
        yield break;

    RectTransform diceRect =
        diceImage.rectTransform;

    Vector3 startPosition =
        diceRect.position;

    // Выбираем случайное место падения на доске.
    Vector3 targetPosition =
        GetRandomBoardPoint();

    // Две промежуточные точки траектории.
    Vector3 randomPoint1 =
        GetRandomBoardPointNear(
            startPosition,
            targetPosition
        );

    Vector3 randomPoint2 =
        GetRandomBoardPointNear(
            startPosition,
            targetPosition
        );

    float elapsed = 0f;
    float spriteTimer = 0f;

    Vector3 originalScale =
        diceRect.localScale;

    // Случайное направление вращения.
    float rotationDirection =
        Random.value > 0.5f
            ? 1f
            : -1f;

    float totalRotation =
        Random.Range(
            720f,
            1440f
        ) * rotationDirection;

    while (elapsed < rollDuration)
    {
        elapsed += Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                elapsed / rollDuration
            );

        Vector3 position;

        // Первая половина пути.
        if (progress < 0.5f)
        {
            float t =
                progress * 2f;

            position =
                Vector3.Lerp(
                    startPosition,
                    randomPoint1,
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    )
                );
        }
        else
        {
            // Вторая половина пути.
            float t =
                (progress - 0.5f) * 2f;

            position =
                Vector3.Lerp(
                    randomPoint1,
                    targetPosition,
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    )
                );

            // Добавляем небольшое изменение траектории.
            position =
                Vector3.Lerp(
                    position,
                    randomPoint2,
                    Mathf.Sin(
                        t * Mathf.PI
                    ) * 0.25f
                );
        }

        // Высота полёта.
        float arc =
            Mathf.Sin(
                progress * Mathf.PI
            ) *
            movementArcHeight;

        position +=
            Vector3.up * arc;

        diceRect.position =
            position;

        // Вращение.
        float rotation =
            totalRotation *
            progress;

        diceRect.rotation =
            Quaternion.Euler(
                0f,
                0f,
                rotation
            );

        // Быстрая смена граней.
        spriteTimer +=
            Time.deltaTime;

        if (spriteTimer >=
            spriteChangeInterval)
        {
            spriteTimer = 0f;

            SetDiceImage(
                diceImage,
                Random.Range(1, 7)
            );
        }

        yield return null;
    }

    // Точно устанавливаем место падения.
    diceRect.position =
        targetPosition;

    // Показываем настоящий результат.
    SetDiceImage(
        diceImage,
        finalValue
    );

    // Кубики останавливаются с небольшой задержкой.
    if (stopDelay > 0f)
    {
        yield return new WaitForSeconds(
            stopDelay
        );
    }

    // Отскок при приземлении.
    yield return StartCoroutine(
        LandingBounce(
            diceRect,
            targetPosition,
            originalScale
        )
    );

    // ВАЖНО:
    // После завершения кубик остаётся здесь.
    diceRect.position =
        targetPosition;

    diceRect.localScale =
        originalScale;

    diceRect.rotation =
        Quaternion.Euler(
            0f,
            0f,
            Random.Range(
                -10f,
                10f
            )
        );
}

/// <summary>
/// Эффект отскока при приземлении.
/// </summary>
private IEnumerator LandingBounce(
    RectTransform diceRect,
    Vector3 targetPosition,
    Vector3 originalScale)
{
    if (diceRect == null)
        yield break;

    float elapsed = 0f;

    while (elapsed <
           landingBounceDuration)
    {
        elapsed +=
            Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                elapsed /
                landingBounceDuration
            );

        // Подпрыгивание вверх-вниз.
        float bounce =
            Mathf.Sin(
                progress * Mathf.PI
            ) *
            landingBounceHeight;

        diceRect.position =
            targetPosition +
            Vector3.up * bounce;

        // Небольшое сжатие при ударе.
        float scale =
            Mathf.Sin(
                progress * Mathf.PI
            );

        diceRect.localScale =
            originalScale *
            (
                1f -
                scale *
                (1f - landingScaleAmount) *
                0.25f
            );

        yield return null;
    }

    diceRect.position =
        targetPosition;

    diceRect.localScale =
        originalScale;
}

/// <summary>
/// Получает случайную точку внутри игровой доски.
/// </summary>
private Vector3 GetRandomBoardPoint()
{
    if (diceBoard == null)
    {
        return transform.position;
    }

    Rect boardRect =
        diceBoard.rect;

    // Небольшой отступ от краёв.
    float padding = 60f;

    float minX =
        boardRect.xMin + padding;

    float maxX =
        boardRect.xMax - padding;

    float minY =
        boardRect.yMin + padding;

    float maxY =
        boardRect.yMax - padding;

    // Защита от слишком маленькой панели.
    if (minX > maxX)
    {
        minX = boardRect.xMin;
        maxX = boardRect.xMax;
    }

    if (minY > maxY)
    {
        minY = boardRect.yMin;
        maxY = boardRect.yMax;
    }

    Vector3 localPoint =
        new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );

    return diceBoard.TransformPoint(
        localPoint
    );
}

/// <summary>
/// Создаёт промежуточную случайную точку
/// между началом и конечной точкой.
/// </summary>
private Vector3 GetRandomBoardPointNear(
    Vector3 start,
    Vector3 target)
{
    Vector3 middle =
        Vector3.Lerp(
            start,
            target,
            Random.Range(
                0.25f,
                0.75f
            )
        );

    Vector2 randomOffset =
        Random.insideUnitCircle *
        randomMovementRadius;

    return middle +
           new Vector3(
               randomOffset.x,
               randomOffset.y,
               0f
           );
}

/// <summary>
/// Возвращает кубики в заданные зоны.
/// Используется только при полном RestartGame().
/// </summary>
private void ResetDicePositions(
    Image[] diceImages,
    RectTransform[] diceZones)
{
    if (diceImages == null ||
        diceZones == null)
        return;

    int count =
        Mathf.Min(
            3,
            Mathf.Min(
                diceImages.Length,
                diceZones.Length
            )
        );

    for (int i = 0; i < count; i++)
    {
        if (diceImages[i] == null ||
            diceZones[i] == null)
            continue;

        RectTransform diceRect =
            diceImages[i].rectTransform;

        diceRect.position =
            diceZones[i].position;

        diceRect.rotation =
            Quaternion.identity;

        diceRect.localScale =
            Vector3.one;
    }
}

/// <summary>
/// Генерирует три случайных значения кубиков.
/// </summary>
private int[] GenerateDiceResults()
{
    int[] results =
        new int[3];

    for (int i = 0; i < 3; i++)
    {
        results[i] =
            Random.Range(1, 7);
    }

    return results;
}

/// <summary>
/// Считает сумму трёх кубиков.
/// </summary>
private int SumDice(
    int[] diceValues)
{
    int sum = 0;

    for (int i = 0;
         i < diceValues.Length;
         i++)
    {
        sum +=
            diceValues[i];
    }

    return sum;
}

/// <summary>
/// Устанавливает спрайт нужного значения.
/// </summary>
private void SetDiceImage(
    Image diceImage,
    int value)
{
    if (diceImage == null)
        return;

    if (diceSprites == null ||
        diceSprites.Length < 6)
    {
        Debug.LogWarning(
            "Массив diceSprites должен содержать 6 спрайтов."
        );

        return;
    }

    value =
        Mathf.Clamp(
            value,
            1,
            6
        );

    diceImage.sprite =
        diceSprites[value - 1];
}

/// <summary>
/// Устанавливает начальное значение
/// для всех кубиков группы.
/// </summary>
private void SetDiceToValue(
    Image[] diceImages,
    int value)
{
    if (diceImages == null)
        return;

    for (int i = 0;
         i < diceImages.Length &&
         i < 3;
         i++)
    {
        SetDiceImage(
            diceImages[i],
            value
        );
    }
}

/// <summary>
/// Возвращает обычный цвет кубиков игрока.
/// </summary>
private void RestorePlayerDiceColors()
{
    if (playerOriginalColors == null)
        return;

    for (int i = 0;
         i < 3;
         i++)
    {
        if (playerDiceImages[i] == null)
            continue;

        // Кубик в руке остаётся подсвеченным.
        if (playerDiceHeld[i])
        {
            playerDiceImages[i].color =
                highlightColor;
        }
        else
        {
            playerDiceImages[i].color =
                playerOriginalColors[i];
        }
    }
}

/// <summary>
/// Обновляет UI счёта.
/// </summary>
private void UpdateScoreUI()
{
    if (playerScoreText != null)
    {
        playerScoreText.text =
            "Игрок: " +
            playerTotalScore +
            " (" +
            playerRoundWins +
            " побед)";
    }

    if (opponentScoreText != null)
    {
        opponentScoreText.text =
            "Оппонент: " +
            opponentTotalScore +
            " (" +
            opponentRoundWins +
            " побед)";
    }
}

/// <summary>
/// Показывает подсказку игроку.
/// </summary>
private void SetHint(string message)
{
    if (hintText != null)
        hintText.text = message;
}

/// <summary>
/// Показывает итоговый результат после трёх раундов.
/// </summary>
private void ShowFinalResult()
{
    string result;

    // Сначала сравниваем победы по раундам.
    if (playerRoundWins >
        opponentRoundWins)
    {
        result =
            "ПОБЕДИЛ ИГРОК!\n\n" +
            "Победы: " +
            playerRoundWins +
            " : " +
            opponentRoundWins;
    }
    else if (opponentRoundWins >
             playerRoundWins)
    {
        result =
            "ПОБЕДИЛ ОППОНЕНТ!\n\n" +
            "Победы: " +
            playerRoundWins +
            " : " +
            opponentRoundWins;
    }
    else
    {
        // Если количество побед одинаковое,
        // сравниваем общий счёт.
        if (playerTotalScore >
            opponentTotalScore)
        {
            result =
                "ПОБЕДИЛ ИГРОК!\n\n" +
                "Побед по раундам: " +
                playerRoundWins +
                " : " +
                opponentRoundWins +
                "\nОбщий счёт: " +
                playerTotalScore +
                " : " +
                opponentTotalScore;
        }
        else if (opponentTotalScore >
                 playerTotalScore)
        {
            result =
                "ПОБЕДИЛ ОППОНЕНТ!\n\n" +
                "Побед по раундам: " +
                playerRoundWins +
                " : " +
                opponentRoundWins +
                "\nОбщий счёт: " +
                playerTotalScore +
                " : " +
                opponentTotalScore;
        }
        else
        {
            result =
                "НИЧЬЯ!\n\n" +
                "Побед по раундам: " +
                playerRoundWins +
                " : " +
                opponentRoundWins +
                "\nОбщий счёт: " +
                playerTotalScore +
                " : " +
                opponentTotalScore;
        }
    }

    if (finalResultText != null)
        finalResultText.text =
            result;

    if (roundResultText != null)
        roundResultText.text =
            "Игра завершена!";

    SetHint(
        "Игра завершена"
    );
}


}
