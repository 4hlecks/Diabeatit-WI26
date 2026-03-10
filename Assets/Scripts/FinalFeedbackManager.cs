using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to Finish scenes.
/// Shows one feedback entry at a time in the "script" textbox.
/// Adds a randomized summary message based on how full the lunchbox is:
/// - 0 items: "make sure to add food" style messages
/// - 1 to 3 items: "try adding more items" style messages
/// - high score or full lunchbox: randomized neutral-positive messages
/// - otherwise: a softer neutral fallback
/// </summary>
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

    [Header("Message Thresholds")]
    public int highScoreThreshold = 70;
    public int fullLunchboxCount = 7; // 6 food slots + 1 drink

    private int index = 0;
    private List<sceneData.FeedbackEntry> entries = new List<sceneData.FeedbackEntry>();

    private static readonly string[] StrongMessages =
    {
        "Nice work packing your lunchbox!",
        "That lunchbox is looking pretty balanced!",
        "Good job putting together a strong meal!",
        "You made some solid lunch choices!",
        "Your lunchbox came together really well!"
    };

    private static readonly string[] MidMessages =
    {
        "You’re off to a good start with your lunchbox.",
        "Nice start—there’s still room to make it even more balanced.",
        "You made a few good choices here.",
        "Your lunchbox has a start—keep building on it next time."
    };

    private static readonly string[] FewItemsMessages =
    {
        "Try adding a few more items next time.",
        "You could still add some more to your lunchbox.",
        "Your lunchbox could use a few more foods.",
        "There’s still room to add more items for a fuller meal.",
        "Try packing a little more next time to round it out."
    };

    private static readonly string[] NoItemsMessages =
    {
        "Your body needs food to stay strong and energized!",
        "Make sure to add food to your lunchbox next time!",
        "Don’t forget to pack something to eat next time!",
        "A lunchbox works best when you fill it with food your body can use!",
        "Try adding food next time so your lunchbox can fuel you up!"
    };

    private void Start()
    {
        LunchboxScoring.RecalculateAndStore();

        // Start with any scoring-generated entries already stored in sceneData
        entries = (sceneData.feedbackEntries != null && sceneData.feedbackEntries.Count > 0)
            ? new List<sceneData.FeedbackEntry>(sceneData.feedbackEntries)
            : new List<sceneData.FeedbackEntry>();

        // Remove old generic fallback entries so they don't conflict with the new randomized summary.
        RemoveGenericFallbackEntries();

        // Always ensure there is at least one general summary page.
        entries.Add(BuildRandomSummaryEntry());

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

    private void RemoveGenericFallbackEntries()
    {
        if (entries == null || entries.Count == 0)
            return;

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var e = entries[i];
            if (e == null) continue;

            string title = string.IsNullOrWhiteSpace(e.title) ? "" : e.title.Trim();
            string body = string.IsNullOrWhiteSpace(e.body) ? "" : e.body.Trim();

            bool isOldGenericFallback =
                (title == "Nice choices!" && body == "Try to include a few different food groups next time.") ||
                (title == "Nice choices!" && body == "Try to include a few different food groups to build a more balanced meal.");

            if (isOldGenericFallback)
            {
                entries.RemoveAt(i);
            }
        }
    }

    private sceneData.FeedbackEntry BuildRandomSummaryEntry()
    {
        int itemCount = GetPackedItemCount();
        bool isFullLunchbox = itemCount >= fullLunchboxCount;
        bool isHighScore = sceneData.FinalPoints >= highScoreThreshold;

        string title;
        string body;

        if (itemCount == 0)
        {
            title = PickRandom(NoItemsMessages);
            body = "Try filling your lunchbox with foods from different groups next time.";
        }
        else if (itemCount <= 3)
        {
            title = PickRandom(FewItemsMessages);
            body = "Adding more foods can help make your meal feel more complete and balanced.";
        }
        else if (isHighScore || isFullLunchbox)
        {
            title = PickRandom(StrongMessages);
            body = "You packed a lunchbox with a strong variety of foods.";
        }
        else
        {
            title = PickRandom(MidMessages);
            body = "See if you can add even more variety next time.";
        }

        return new sceneData.FeedbackEntry
        {
            title = title,
            body = body,
            bonus = 0
        };
    }

    private int GetPackedItemCount()
    {
        int foodCount = (sceneData.foodInSlots != null) ? sceneData.foodInSlots.Count : 0;
        int drinkCount = (sceneData.drinkInSlot != null) ? sceneData.drinkInSlot.Count : 0;
        return foodCount + drinkCount;
    }

    private string PickRandom(string[] options)
    {
        if (options == null || options.Length == 0)
            return "Nice work!";

        int randomIndex = Random.Range(0, options.Length);
        return options[randomIndex];
    }

    private void Render()
    {
        if (entries == null || entries.Count == 0)
            entries = new List<sceneData.FeedbackEntry> { BuildRandomSummaryEntry() };

        int safeIndex = Mathf.Clamp(index, 0, entries.Count - 1);
        var e = entries[safeIndex];

        string bonusPart = (e.bonus > 0) ? $"(+{e.bonus} Bonus Points) " : "";
        string text = $"{e.title} {bonusPart}You earned {sceneData.FinalPoints} points in total! {e.body}";

        text = text.Replace("\r", " ").Replace("\n", " ");
        while (text.Contains("  "))
            text = text.Replace("  ", " ");
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