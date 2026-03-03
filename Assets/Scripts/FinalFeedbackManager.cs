using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to Finish scenes.
/// Shows one feedback entry at a time in the "script" textbox.
/// Format:
///   "{Title} (+N Bonus Points) You earned ___ points in total! {Body}"
/// Forces single paragraph and enables TMP auto-sizing.
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
    private List<sceneData.FeedbackEntry> entries = new List<sceneData.FeedbackEntry>();

    private void Start()
    {
        LunchboxScoring.RecalculateAndStore();

        entries = (sceneData.feedbackEntries != null && sceneData.feedbackEntries.Count > 0)
            ? new List<sceneData.FeedbackEntry>(sceneData.feedbackEntries)
            : new List<sceneData.FeedbackEntry>
            {
                new sceneData.FeedbackEntry
                {
                    title = "Nice choices!",
                    body = "Try to include a few different food groups next time.",
                    bonus = 0
                }
            };

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
        int safeIndex = Mathf.Clamp(index, 0, entries.Count - 1);
        var e = entries[safeIndex];

        string bonusPart = (e.bonus > 0) ? $"(+{e.bonus} Bonus Points) " : "";
        string text = $"{e.title} {bonusPart}You earned {sceneData.FinalPoints} points in total! {e.body}";

        // Force single paragraph
        text = text.Replace("\r", " ").Replace("\n", " ");
        while (text.Contains("  ")) text = text.Replace("  ", " ");
        text = text.Trim();

        if (scriptTMP != null) scriptTMP.text = text;
        if (scriptText != null) scriptText.text = text;

        if (nextButton != null)
            nextButton.gameObject.SetActive(index < entries.Count - 1);
    }

    public void NextMessage()
    {
        if (index < entries.Count - 1)
        {
            index++;
            Render();
        }
    }
}