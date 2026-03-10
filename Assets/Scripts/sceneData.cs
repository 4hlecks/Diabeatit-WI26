using System.Collections.Generic;

public static class sceneData
{
    public static int SelectedLunchboxIndex = 0;

    // Scoring
    public static int BasePoints = 0;
    public static int BonusPoints = 0;
    public static int FinalPoints = 0;

    // Backwards compat
    public static int TotalPoints = 0;

    // Slot placement
    public static List<int> slotPositions = new List<int>();

    // Items chosen
    public static List<Item> foodInSlots = new List<Item>();
    public static List<Item> drinkInSlot = new List<Item>();

    // Receipt order (used by scorer)
    public static List<Item> receiptItems = new List<Item>();

    // Legacy receipt display (names only)
    public static List<string> receiptFood = new List<string>();

    // Legacy finish-scene feedback strings
    public static List<string> feedbackMessages = new List<string>();

    [System.Serializable]
    public class FeedbackEntry
    {
        public string title;
        public string body;
        public int bonus;
    }

    public static List<FeedbackEntry> feedbackEntries = new List<FeedbackEntry>();

    [System.Serializable]
    public class ReceiptEntry
    {
        public string foodName;
        public string category;
        public int points;
        public bool penalized;
    }

    [System.Serializable]
    public class BonusBreakdownEntry
    {
        public string title;   // ex: "Steak + Bell Pepper"
        public string body;    // ex: combo description
        public int bonus;      // ex: 3
        public bool isCombo;   // true for food combos, false for color/balance bonuses
    }

    // Legacy formatted receipt lines
    public static List<string> receiptLines = new List<string>();

    // Structured data for report scene
    public static List<ReceiptEntry> receiptEntries = new List<ReceiptEntry>();
    public static List<BonusBreakdownEntry> bonusBreakdowns = new List<BonusBreakdownEntry>();
    public static string bonusSummaryText = "";
}