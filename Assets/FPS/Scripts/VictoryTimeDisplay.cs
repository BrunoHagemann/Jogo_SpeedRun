using UnityEngine;
using TMPro;

public class VictoryTimeDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI lastTimeText;
    public TextMeshProUGUI bestTimeText; // Opcional: para mostrar o recorde

    // OnEnable roda toda vez que a tela/objeto é ativada (perfeito para menus que abrem e fecham)
    private void OnEnable()
    {
        // Puxa os tempos salvos
        float lastTime = PlayerPrefs.GetFloat("LastSpeedrunTime", 0f);
        float bestTime = PlayerPrefs.GetFloat("BestSpeedrunTime", 0f);

        // Aplica nos textos
        if (lastTimeText != null) lastTimeText.text = "Tempo Final: " + FormatTime(lastTime);
        if (bestTimeText != null) bestTimeText.text = "Recorde: " + FormatTime(bestTime);
    }

    // Função para formatar o número flutuante de volta para 00:00.000
    private string FormatTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60f);
        int seconds = Mathf.FloorToInt(timeToFormat % 60f);
        int milliseconds = Mathf.FloorToInt((timeToFormat * 1000f) % 1000f);

        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
}
