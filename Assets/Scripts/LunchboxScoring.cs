using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized scoring + feedback rules for the Lunchbox minigame.
///
/// Rules:
/// - Max "regular" points capped to 100
/// - Duplicate category penalty: 2+ items in same category => the 2nd+ items are worth half (rounded)
/// - Balanced meal bonus: 7/6/5 distinct categories => +5/+3/+2
/// - Color bonus: 4+ distinct food colors => +2
/// - Pairing Power (combo) bonuses: specific item pairs => +3 each
/// - Bonus cap: total bonus capped to 16
/// - Generates a list of feedback messages to page through on the Finish scene
/// - Generates receipt lines: "Apple (Fruit) - 16 points" (or 8 if penalized)
///
/// NOTE: Messages can include the placeholder "{POINTS}" anywhere.
/// FinalFeedbackManager replaces "{POINTS}" with the player's final score.
/// Messages are single-paragraph only (no emojis, no line breaks).
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
        public readonly string Message;

        public ComboRule(string a, string b, string message)
        {
            A = a; B = b; Message = message;
        }

        public bool IsSatisfied(HashSet<string> foods) => foods.Contains(A) && foods.Contains(B);
        public string Id => $"{A}|{B}";
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

    // Combo rules (Food name matching must match your Item.Food strings)
    private static readonly List<ComboRule> ComboRules = new List<ComboRule>
    {
        new ComboRule("Steak","Bell Pepper","You created a Healthy Combo! You earned {POINTS} points! Vitamin C from bell peppers helps absorb iron from steak. +3 Points"),
        new ComboRule("Tofu","Orange","You created a Healthy Combo! You earned {POINTS} points! Vitamin C from oranges helps boost iron absorption from tofu. +3 Points"),
        new ComboRule("Avocado","Carrots","You created a Healthy Combo! You earned {POINTS} points! Healthy fats in avocado help absorb vitamin A from carrots. +3 Points"),
        new ComboRule("Cheese","Whole Grain Bread","You created a Healthy Combo! You earned {POINTS} points! Whole grains help the body absorb calcium from cheese. +3 Points"),
        new ComboRule("Eggs","Quinoa","You created a Healthy Combo! You earned {POINTS} points! Eggs and quinoa provide a strong combo of protein and whole grains for steady energy! +3 Points"),
        new ComboRule("Yogurt","Banana","You created a Healthy Combo! You earned {POINTS} points! Probiotics in yogurt and fiber in bananas support gut health! +3 Points"),
        new ComboRule("Banana","Water","You created a Healthy Combo! You earned {POINTS} points! Bananas provide potassium to balance hydration from water! +3 Points"),
        new ComboRule("Fish","Grapes","You created a Healthy Combo! You earned {POINTS} points! Omega-3s in fish and antioxidants in grapes support heart health! +3 Points"),
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
        public List<string> Messages;
        public List<ReceiptLine> Receipt;
    }

    public static Result RecalculateAndStore()
    {
        Result r = Calculate();
        sceneData.BasePoints = r.BasePoints;
        sceneData.BonusPoints = r.BonusPoints;
        sceneData.FinalPoints = r.FinalPoints;
        sceneData.TotalPoints = r.FinalPoints; // backwards-compat
        sceneData.feedbackMessages = r.Messages;
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
        string balancedMessage = null;

        if (distinctCount >= 7)
        {
            balancedBonus = 5;
            balancedMessage = "SUPER JOB! You earned {POINTS} points! You have a completely balanced lunchbox. Click on each item to learn more.";
        }
        else if (distinctCount == 6)
        {
            balancedBonus = 3;
            ItemType missing = GetMissingCategory(distinctTypes);

            if (missing == ItemType.Snack)
            {
                balancedMessage = "Great job! You earned {POINTS} points! You’ve built a balanced meal! Don't forget—you can still choose a snack to enjoy. A little treat can be part of a healthy meal too!";
            }
            else
            {
                balancedMessage = "Great job! You earned {POINTS} points! Try picking something from the " + PrettyType(missing) + " next time :)";
            }
        }
        else if (distinctCount == 5)
        {
            balancedBonus = 2;
            List<ItemType> missing2 = GetTwoMissingCategories(distinctTypes);
            balancedMessage = "Great job! You earned {POINTS} points! Can you choose something from " + PrettyType(missing2[0]) + " and " + PrettyType(missing2[1]) + " too?";
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

        // ---------- Combo bonuses ----------
        HashSet<string> foods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Item it in picked)
        {
            if (it == null) continue;
            if (!string.IsNullOrWhiteSpace(it.Food))
                foods.Add(it.Food.Trim());
        }

        List<string> comboMessages = new List<string>();
        int comboBonus = 0;
        HashSet<string> awardedCombos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in ComboRules)
        {
            if (!rule.IsSatisfied(foods)) continue;
            if (awardedCombos.Contains(rule.Id)) continue;

            awardedCombos.Add(rule.Id);
            comboBonus += 3;
            comboMessages.Add(rule.Message);
        }

        // ---------- Total bonus (cap) ----------
        int uncappedBonus = balancedBonus + colorBonus + comboBonus;
        int bonusPoints = Mathf.Clamp(uncappedBonus, 0, MaxBonusPoints);

        int finalPoints = basePoints + bonusPoints;

        // ---------- Messages (paged in Finish scene) ----------
        List<string> messages = new List<string>();

        // 1) Primary feedback first
        if (!string.IsNullOrEmpty(balancedMessage))
            messages.Add(balancedMessage);
        else
            messages.Add("Nice choices! You earned {POINTS} points! Try to include a few different food groups to build a more balanced meal.");

        // 2) Duplicate-category message (only if it happened)
        if (hasDuplicates)
            messages.Add("Nice choice! You earned {POINTS} points! Try to balance your meal with different food groups next time!");

        // 3) Color bonus message (only if earned)
        if (colorBonus > 0)
            messages.Add("Awesome choice! You earned {POINTS} points! You’ve picked four colorful foods—your meal is not only bright but packed with nutrients! +2 Points");

        // 4) Combo messages (0..N)
        messages.AddRange(comboMessages);

        return new Result
        {
            BasePoints = basePoints,
            BonusPoints = bonusPoints,
            FinalPoints = finalPoints,
            Messages = messages,
            Receipt = receiptLines
        };
    }

    // ----------------- helpers -----------------

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