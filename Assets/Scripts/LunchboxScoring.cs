using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized scoring + feedback rules for the Lunchbox minigame.
///
/// Rules:
/// - Max base points capped to 100
/// - Duplicate category penalty: 2+ items in same category => the 2nd+ items are worth half (rounded)
/// - Balanced meal bonus: 7/6/5 distinct categories => +5/+3/+2
/// - Color bonus: 4+ distinct food colors => +2
/// - Pairing Power (combo) bonuses: specific item pairs => +3 each
/// - Bonus cap: total bonus capped to 16
/// - Generates structured feedback entries (title/body/bonus) for Finish scene
/// - If 1+ combos are earned, a COMBO SUMMARY becomes the FIRST message (replacing the default "Nice/Great job" page)
/// - Combo summary stacks pairs like "(Banana + Yogurt, Steak + Bell Pepper) (6+ points)"
/// - Generates receipt lines: "Apple (Fruit) - 16 points" (or 8 if penalized)
/// </summary>
public static class LunchboxScoring
{
    public const int MaxBasePoints = 100;
    public const int MaxBonusPoints = 16;

    private enum FoodColor { Red, Orange, Yellow, Green, Purple }

    private sealed class ComboRule
    {
        public readonly string A;
        public readonly string B;
        public readonly string Title;
        public readonly string Body;
        public readonly int Bonus;

        // Normalized keys for matching
        public readonly string KeyA;
        public readonly string KeyB;

        public ComboRule(string a, string b, string title, string body, int bonus)
        {
            A = a;
            B = b;
            Title = title;
            Body = body;
            Bonus = bonus;

            KeyA = NormalizeFoodKey(a);
            KeyB = NormalizeFoodKey(b);
        }

        public bool IsSatisfied(HashSet<string> normalizedFoods)
            => normalizedFoods.Contains(KeyA) && normalizedFoods.Contains(KeyB);

        public string Id => $"{KeyA}|{KeyB}";
    }

    // Mapping for the "4 colors" bonus. Extend as you add foods.
    private static readonly Dictionary<string, FoodColor> FoodToColor =
        new Dictionary<string, FoodColor>(StringComparer.OrdinalIgnoreCase)
        {
            { "Tomato", FoodColor.Red },
            { "Apple", FoodColor.Red },
            { "Bell Pepper", FoodColor.Red },

            { "Orange", FoodColor.Orange },
            { "Carrot", FoodColor.Orange },
            { "Carrots", FoodColor.Orange },

            { "Banana", FoodColor.Yellow },
            { "Corn", FoodColor.Yellow },

            { "Lettuce", FoodColor.Green },
            { "Cucumber", FoodColor.Green },
            { "Snap Pea", FoodColor.Green },
            { "Avocado", FoodColor.Green },

            { "Grapes", FoodColor.Purple },
        };

    // Combo rules (matched using normalized food keys)
    private static readonly List<ComboRule> ComboRules = new List<ComboRule>
    {
        new ComboRule("Steak","Bell Pepper",
            "You created a Healthy Combo!",
            "Vitamin C from bell peppers helps absorb iron from steak.",
            3),

        new ComboRule("Tofu","Orange",
            "You created a Healthy Combo!",
            "Vitamin C from oranges helps boost iron absorption from tofu.",
            3),

        new ComboRule("Avocado","Carrots",
            "You created a Healthy Combo!",
            "Healthy fats in avocado help absorb vitamin A from carrots.",
            3),

        new ComboRule("Cheese","Whole Grain Bread",
            "You created a Healthy Combo!",
            "Whole grains help the body absorb calcium from cheese.",
            3),

        new ComboRule("Eggs","Quinoa",
            "You created a Healthy Combo!",
            "Eggs and quinoa provide a strong combo of protein and whole grains for steady energy.",
            3),

        new ComboRule("Yogurt","Banana",
            "You created a Healthy Combo!",
            "Probiotics in yogurt and fiber in bananas support gut health.",
            3),

        new ComboRule("Banana","Water",
            "You created a Healthy Combo!",
            "Bananas provide potassium to balance hydration from water.",
            3),

        new ComboRule("Fish","Grapes",
            "You created a Healthy Combo!",
            "Omega-3s in fish and antioxidants in grapes support heart health.",
            3),
    };

    public struct ReceiptLine
    {
        public string Text;
        public bool Penalized;
    }

    public struct Result
    {
        public int BasePoints;
        public int BonusPoints;
        public int FinalPoints;

        public List<sceneData.FeedbackEntry> Entries; // structured feedback (preferred)
        public List<string> Messages;                 // legacy (optional)
        public List<ReceiptLine> Receipt;
    }

    public static Result RecalculateAndStore()
    {
        Result r = Calculate();

        sceneData.BasePoints = r.BasePoints;
        sceneData.BonusPoints = r.BonusPoints;
        sceneData.FinalPoints = r.FinalPoints;

        // backwards-compat
        sceneData.TotalPoints = r.FinalPoints;

        // Store structured entries (preferred)
        sceneData.feedbackEntries = (r.Entries != null) ? r.Entries : new List<sceneData.FeedbackEntry>();

        // Optional: keep legacy messages populated too
        sceneData.feedbackMessages = (r.Messages != null) ? r.Messages : new List<string>();

        // Store formatted receipt lines (adjusted per-item points)
        sceneData.receiptLines = new List<string>();
        for (int i = 0; i < r.Receipt.Count; i++)
            sceneData.receiptLines.Add(r.Receipt[i].Text);

        return r;
    }

    public static Result Calculate()
    {
        // All picked items (for bonuses)
        List<Item> picked = new List<Item>();
        picked.AddRange(sceneData.foodInSlots);
        picked.AddRange(sceneData.drinkInSlot);

        // ---------- Duplicate-category penalty (halve 2nd+ items of same category) ----------
        Dictionary<ItemType, int> seenTypeCount = new Dictionary<ItemType, int>();
        List<ReceiptLine> receiptLines = new List<ReceiptLine>();
        int basePointsRaw = 0;
        bool hasDuplicates = false;

        // Receipt order should match pick order (receiptItems); fallback to picked
        List<Item> receiptOrder = (sceneData.receiptItems != null && sceneData.receiptItems.Count > 0)
            ? sceneData.receiptItems
            : picked;

        foreach (Item it in receiptOrder)
        {
            if (it == null) continue;

            if (!seenTypeCount.ContainsKey(it.type))
                seenTypeCount[it.type] = 0;

            seenTypeCount[it.type] += 1;

            bool penalized = (seenTypeCount[it.type] >= 2);
            if (penalized) hasDuplicates = true;

            int adjusted = penalized ? Mathf.RoundToInt(it.points * 0.5f) : it.points;
            basePointsRaw += adjusted;

            receiptLines.Add(new ReceiptLine
            {
                Penalized = penalized,
                Text = $"{it.Food} ({it.type}) - {adjusted} points"
            });
        }

        int basePoints = Mathf.Clamp(basePointsRaw, 0, MaxBasePoints);

        // ---------- Balanced meal bonus (distinct categories) ----------
        HashSet<ItemType> distinctTypes = new HashSet<ItemType>();
        foreach (Item it in picked)
        {
            if (it == null) continue;
            distinctTypes.Add(it.type);
        }

        int distinctCount = distinctTypes.Count;
        int balancedBonus = 0;

        string balancedTitle = null;
        string balancedBody = null;

        if (distinctCount >= 7)
        {
            balancedBonus = 5;
            balancedTitle = "SUPER JOB!";
            balancedBody = "You have a completely balanced lunchbox.";
        }
        else if (distinctCount == 6)
        {
            balancedBonus = 3;
            ItemType missing = GetMissingCategory(distinctTypes);

            balancedTitle = "Great job!";
            if (missing == ItemType.Snack)
            {
                balancedBody = "You’ve built a balanced meal. Don’t forget—you can still choose a snack to enjoy. A little treat can be part of a healthy meal too!";
            }
            else
            {
                balancedBody = "Try picking something from the " + PrettyType(missing) + " next time.";
            }
        }
        else if (distinctCount == 5)
        {
            balancedBonus = 2;
            List<ItemType> missing2 = GetTwoMissingCategories(distinctTypes);
            balancedTitle = "Great job!";
            balancedBody = "Can you choose something from " + PrettyType(missing2[0]) + " and " + PrettyType(missing2[1]) + " too?";
        }

        // ---------- Color bonus (4+ distinct colors) ----------
        HashSet<FoodColor> distinctColors = new HashSet<FoodColor>();
        foreach (Item it in picked)
        {
            if (it == null) continue;
            if (string.IsNullOrWhiteSpace(it.Food)) continue;

            if (FoodToColor.TryGetValue(it.Food.Trim(), out FoodColor c))
                distinctColors.Add(c);
        }
        int colorBonus = (distinctColors.Count >= 4) ? 2 : 0;

        // ---------- Combo bonuses (normalized matching) ----------
        HashSet<string> normalizedFoods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> keyToDisplayName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Item it in picked)
        {
            if (it == null) continue;
            if (string.IsNullOrWhiteSpace(it.Food)) continue;

            string display = it.Food.Trim();
            string key = NormalizeFoodKey(display);

            if (!string.IsNullOrEmpty(key))
            {
                normalizedFoods.Add(key);
                if (!keyToDisplayName.ContainsKey(key))
                    keyToDisplayName[key] = display; // keep first seen display name
            }
        }

        int comboBonus = 0;
        HashSet<string> awardedCombos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // We store earned combos as "A + B" strings so we can stack them in one parentheses list.
        List<string> earnedPairs = new List<string>();

        foreach (var rule in ComboRules)
        {
            if (!rule.IsSatisfied(normalizedFoods)) continue;
            if (awardedCombos.Contains(rule.Id)) continue;

            awardedCombos.Add(rule.Id);
            comboBonus += rule.Bonus;

            string aDisplay = keyToDisplayName.TryGetValue(rule.KeyA, out var aName) ? aName : rule.A;
            string bDisplay = keyToDisplayName.TryGetValue(rule.KeyB, out var bName) ? bName : rule.B;

            earnedPairs.Add($"{aDisplay} + {bDisplay}");
        }

        // ---------- Total bonus (cap) ----------
        int uncappedBonus = balancedBonus + colorBonus + comboBonus;
        int bonusPoints = Mathf.Clamp(uncappedBonus, 0, MaxBonusPoints);

        int finalPoints = basePoints + bonusPoints;

        // ---------- Structured feedback entries ----------
        List<sceneData.FeedbackEntry> entries = new List<sceneData.FeedbackEntry>();

        // 1) If combos exist: create ONE summary message FIRST and replace the default "Nice/Great job" first page.
        if (earnedPairs.Count > 0)
        {
            // Optional: randomize order before displaying
            ShuffleInPlace(earnedPairs);

            string pairList = string.Join(", ", earnedPairs);
            string pointsText = $"{comboBonus}+ points";

            entries.Add(new sceneData.FeedbackEntry
            {
                // Put both pair stacking + stacked points inside the title parentheses
                title = $"You created Healthy Combos! ({pairList}) ({pointsText})",
                body = "These pairings work well together.",
                // IMPORTANT: set bonus to 0 so FinalFeedbackManager doesn't add a second bonus label.
                bonus = 0
            });
        }

        // 2) Balanced/default page AFTER the combo summary (or first if no combos)
        if (!string.IsNullOrEmpty(balancedTitle) && !string.IsNullOrEmpty(balancedBody))
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = balancedTitle,
                body = balancedBody,
                bonus = balancedBonus
            });
        }
        else if (entries.Count == 0)
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "Nice choices!",
                body = "Try to include a few different food groups to build a more balanced meal.",
                bonus = 0
            });
        }

        // 3) Duplicate-category message
        if (hasDuplicates)
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "Nice choice!",
                body = "Try to balance your meal with different food groups next time.",
                bonus = 0
            });
        }

        // 4) Color bonus message
        if (colorBonus > 0)
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "Awesome choice!",
                body = "You’ve picked four colorful foods—your meal is not only bright but packed with nutrients!",
                bonus = colorBonus
            });
        }

        // Legacy messages (optional)
        List<string> legacyMessages = new List<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            string bonusPart = (e.bonus > 0) ? $" +{e.bonus} Points" : "";
            legacyMessages.Add($"{e.title} You earned {finalPoints} points! {e.body}{bonusPart}");
        }

        return new Result
        {
            BasePoints = basePoints,
            BonusPoints = bonusPoints,
            FinalPoints = finalPoints,
            Entries = entries,
            Messages = legacyMessages,
            Receipt = receiptLines
        };
    }

    // ----------------- helpers -----------------

    // Normalizes strings so "Water Bottle" can match "Water", "Whole-Grain Bread" matches "Whole Grain Bread",
    // and "Carrots" can match "Carrot" by removing a trailing 's' (basic plural handling).
    private static string NormalizeFoodKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";

        s = s.Trim().ToLowerInvariant();

        // Keep letters/numbers only (removes spaces, punctuation, hyphens, etc.)
        System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
        }

        string key = sb.ToString();

        // Basic plural handling: remove trailing 's' if word is longer than 3
        if (key.Length > 3 && key.EndsWith("s"))
            key = key.Substring(0, key.Length - 1);

        return key;
    }

    private static void ShuffleInPlace(List<string> list)
    {
        // Fisher–Yates shuffle
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    private static string PrettyType(ItemType t)
    {
        switch (t)
        {
            case ItemType.Protein: return "Proteins";
            case ItemType.Grains: return "Grains";
            case ItemType.Fruit: return "Fruits";
            case ItemType.Snack: return "Snacks";
            case ItemType.Drink: return "Drinks";
            case ItemType.Vegetable: return "Vegetables";
            case ItemType.Dairy: return "Dairy";
            default: return t.ToString();
        }
    }

    private static ItemType GetMissingCategory(HashSet<ItemType> have)
    {
        ItemType[] all = new[]
        {
            ItemType.Protein,
            ItemType.Grains,
            ItemType.Fruit,
            ItemType.Snack,
            ItemType.Drink,
            ItemType.Vegetable,
            ItemType.Dairy,
        };

        foreach (var t in all)
            if (!have.Contains(t)) return t;

        return ItemType.Snack;
    }

    private static List<ItemType> GetTwoMissingCategories(HashSet<ItemType> have)
    {
        List<ItemType> missing = new List<ItemType>();
        ItemType[] all = new[]
        {
            ItemType.Protein,
            ItemType.Grains,
            ItemType.Fruit,
            ItemType.Snack,
            ItemType.Drink,
            ItemType.Vegetable,
            ItemType.Dairy,
        };

        foreach (var t in all)
        {
            if (!have.Contains(t))
            {
                missing.Add(t);
                if (missing.Count == 2) break;
            }
        }

        while (missing.Count < 2) missing.Add(ItemType.Snack);
        return missing;
    }
}