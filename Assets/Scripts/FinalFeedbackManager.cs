using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalFeedbackManager : MonoBehaviour
{
    [Header("Script Text (use ONE of these)")]
    public TMP_Text scriptTMP;
    public Text scriptText;

    [Header("Next Button")]
    public Button nextButton;

    [Header("Auto Size (TMP only)")]
    public bool enableAutoSize = true;
    public float fontSizeMax = 36f;
    public float fontSizeMin = 18f;

    private void Start()
    {
        LunchboxScoring.RecalculateAndStore();

        if (scriptTMP != null && enableAutoSize)
        {
            scriptTMP.enableAutoSizing = true;
            scriptTMP.fontSizeMax = fontSizeMax;
            scriptTMP.fontSizeMin = fontSizeMin;
        }

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        Render();
    }

    private void Render()
    {
        string text = $"Congratulations! You earned {sceneData.FinalPoints} points. Amazing job! Would you like to play again?";

        text = text.Replace("\r", " ").Replace("\n", " ");
        while (text.Contains("  "))
            text = text.Replace("  ", " ");
        text = text.Trim();

        if (scriptTMP != null) scriptTMP.text = text;
        if (scriptText != null) scriptText.text = text;
    }
}