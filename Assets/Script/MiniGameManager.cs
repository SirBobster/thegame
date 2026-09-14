using System;
using TMPro;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    [Header("Настройки мини-игры")]
    [SerializeField] private float timeLimit = 25f;
    [SerializeField] private int targetValuableCount = 4;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Зоны")]
    [SerializeField] private DropZone saveZone;
    [SerializeField] private DropZone trashZone;

    public event Action<bool> OnMiniGameComplete;

    private float currentTime;
    private int savedValuables;
    private bool isGameFinished;

    public int SavedValuables => savedValuables;
    public bool IsGameFinished => isGameFinished;

    private void Start()
    {
        currentTime = timeLimit;
        savedValuables = 0;

        UpdateTimerText();
        UpdateProgressText();
    }

    private void Update()
    {
        if (isGameFinished)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerText();

            CompleteGame(false);
            return;
        }

        UpdateTimerText();
    }

    /// <summary>
    /// Добавляет одну спасённую ценность.
    /// </summary>
    public void AddSavedValuable()
    {
        if (isGameFinished)
            return;

        savedValuables++;

        UpdateProgressText();

        if (savedValuables >= targetValuableCount)
        {
            CompleteGame(true);
        }
    }

    private void CompleteGame(bool success)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;

        // Таймер больше не обновляется, но катсцена не блокируется.
        OnMiniGameComplete?.Invoke(success);
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = currentTime.ToString("0.0");
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
            progressText.text = $"Спасено: {savedValuables} / {targetValuableCount}";
    }
}