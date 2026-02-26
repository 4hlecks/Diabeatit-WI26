using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to Finish scenes.
/// It shows one message at a time in the "script" textbox and replaces {POINTS} with the final score.
/// Also sanitizes messages to a single paragraph (no line breaks) and enables TMP auto-sizing.
/// </summary>
public class FinalFeedbackManager : MonoBehaviour
{
    [Header("Script Text (use ONE of these)")]
    public TMP_Text scriptTMP;     // TextMeshProUGUI (recommended)
    public Text scriptText;        // Legacy UI Text (fallback)

    [Header("Next Button")]
    public Button nextButton;

    [Header("Auto Size (TMP only)")]
    public bool enableAutoSize = true;
    public float fontSizeMax = 36f;
    public float fontSizeMin = 18f;

    private int index = 0;
    private List<string> msgs = new List<string>();

    private void Start()
    {
        LunchboxScoring.RecalculateAndStore();

        msgs = (sceneData.feedbackMessages != null && sceneData.feedbackMessages.Count > 0)
            ? new List<string>(sceneData.feedbackMessages)
            : new List<string> { "You earned {POINTS} points! Nice choices! Try to include a few different food groups next time!" };

        // TMP auto-size so text shrinks if it would overflow the box
        if (scriptTMP != null && enableAutoSize)
        {
            scriptTMP.enableAutoSizing = true;
            scriptTMP.fontSizeMax = fontSizeMax;
            scriptTMP.fontSizeMin = fontSizeMin;
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextMessage);
        }

        Render();
    }

    private void Render()
    {
        string body = msgs[Mathf.Clamp(index, 0, msgs.Count - 1)];

        // Replace placeholder with actual score
        body = body.Replace("{POINTS}", sceneData.FinalPoints.ToString());

        // Force single-paragraph formatting even if a string contains line breaks
        body = body.Replace("\r", " ").Replace("\n", " ");
        while (body.Contains("  ")) body = body.Replace("  ", " ");

        if (scriptTMP != null) scriptTMP.text = body;
        if (scriptText != null) scriptText.text = body;

        if (nextButton != null)
            nextButton.gameObject.SetActive(index < msgs.Count - 1);
    }

    public void NextMessage()
    {
        if (index < msgs.Count - 1)
        {
            index++;
            Render();
        }
    }
}