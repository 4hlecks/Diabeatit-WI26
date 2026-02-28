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

    // Receipt order (used by scorer if present)
    public static List<Item> receiptItems = new List<Item>();

    // Receipt display (names only, legacy)
    public static List<string> receiptFood = new List<string>();

    // Legacy: Finish-scene feedback pages (string based)
    public static List<string> feedbackMessages = new List<string>();

    // NEW: Structured feedback pages (preferred)
    [System.Serializable]
    public class FeedbackEntry
    {
        public string title;   // ex: "You created a Healthy Combo!"
        public string body;    // ex: "Vitamin C from bell peppers helps absorb iron from steak."
        public int bonus;      // ex: 3
    }

    public static List<FeedbackEntry> feedbackEntries = new List<FeedbackEntry>();

    // Optional: scored receipt lines (Food (Type) - adjusted points)
    public static List<string> receiptLines = new List<string>();
}