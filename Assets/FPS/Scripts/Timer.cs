using TMPro;
using UnityEngine;
using Unity.FPS.Game;
public class Timer : MonoBehaviour
{
    // Singleton para fácil acesso global
    public static Timer Instance { get; private set; }

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    private float currentTime = 0f;
    private bool isRunning = false;

    private void Awake()
    {
        // Configuração do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Inicia automaticamente quando a cena/jogo carregar
        StartTimer();

        EventManager.AddListener<AllObjectivesCompletedEvent>(OnObjectivesCompleted);
        EventManager.AddListener<GameOverEvent>(OnGameOver);
    }

    private void OnDestroy()
    {
        // Remove a inscrição para evitar erros
        EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnObjectivesCompleted);
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
    }

    private void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateUI();

        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateUI();
    }

    private void OnObjectivesCompleted(AllObjectivesCompletedEvent evt)
    {
        StopTimer(true);
    }

    private void OnGameOver(GameOverEvent evt)
    {
        StopTimer(evt.Win);
    }

    public void StopTimer(bool isVictory = false)
    {
        isRunning = false;

        // Só salva o tempo se o jogador tiver vencido a fase
        if (isVictory)
        {
            // Salva o tempo atual na memória da Unity
            PlayerPrefs.SetFloat("LastSpeedrunTime", currentTime);

            // BÔNUS: Lógica para salvar o "Recorde" (Melhor Tempo)
            float bestTime = PlayerPrefs.GetFloat("BestSpeedrunTime", float.MaxValue);
            if (currentTime < bestTime)
            {
                PlayerPrefs.SetFloat("BestSpeedrunTime", currentTime);
            }

            PlayerPrefs.Save();
        }
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        // Calcula minutos, segundos
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        // Formata para 00:00.000
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
