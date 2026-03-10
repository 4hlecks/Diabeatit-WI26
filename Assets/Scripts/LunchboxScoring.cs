using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class LunchboxScoring
{
    public const int MaxBasePoints = 100;
    public const int MaxBonusPoints = 16;

    private enum FoodColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Purple
    }

    private sealed class ComboRule
    {
        public readonly string A;
        public readonly string B;
        public readonly string Title;
        public readonly string Body;
        public readonly int Bonus;

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
        {
            return normalizedFoods.Contains(KeyA) && normalizedFoods.Contains(KeyB);
        }

        public string Id => $"{KeyA}|{KeyB}";
    }

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

        public List<sceneData.FeedbackEntry> Entries;
        public List<string> Messages;
        public List<ReceiptLine> Receipt;

        public List<sceneData.ReceiptEntry> ReceiptEntries;
        public List<sceneData.BonusBreakdownEntry> BonusBreakdowns;
        public string BonusSummaryText;
    }

    private static readonly Dictionary<string, FoodColor> FoodToColor =
        new Dictionary<string, FoodColor>(StringComparer.OrdinalIgnoreCase)
        {
            { "Tomato", FoodColor.Red },
            { "Apple", FoodColor.Red },
            { "Strawberries", FoodColor.Red },
            { "Strawberry", FoodColor.Red },
            { "Bell Pepper", FoodColor.Red },

            { "Orange", FoodColor.Orange },
            { "Oranges", FoodColor.Orange },
            { "Carrot", FoodColor.Orange },
            { "Carrots", FoodColor.Orange },

            { "Banana", FoodColor.Yellow },
            { "Bananas", FoodColor.Yellow },
            { "Corn", FoodColor.Yellow },

            { "Lettuce", FoodColor.Green },
            { "Broccoli", FoodColor.Green },
            { "Cucumber", FoodColor.Green },
            { "Snap Pea", FoodColor.Green },
            { "Avocado", FoodColor.Green },

            { "Grapes", FoodColor.Purple },
            { "Blueberries", FoodColor.Purple }
        };

    private static readonly List<ComboRule> ComboRules = new List<ComboRule>
    {
        new ComboRule(
            "Steak",
            "Bell Pepper",
            "Healthy Combo!",
            "Vitamin C from bell peppers helps absorb iron from steak.",
            3),

        new ComboRule(
            "Tofu",
            "Orange",
            "Healthy Combo!",
            "Vitamin C from oranges helps boost iron absorption from tofu.",
            3),

        new ComboRule(
            "Avocado",
            "Carrots",
            "Healthy Combo!",
            "Healthy fats in avocado help absorb vitamin A from carrots.",
            3),

        new ComboRule(
            "Cheese",
            "Whole Grain Bread",
            "Healthy Combo!",
            "Whole grains help the body absorb calcium from cheese.",
            3),

        new ComboRule(
            "Eggs",
            "Quinoa",
            "Healthy Combo!",
            "Eggs and quinoa provide a strong combo of protein and whole grains for steady energy.",
            3),

        new ComboRule(
            "Yogurt",
            "Banana",
            "Healthy Combo!",
            "Probiotics in yogurt and fiber in bananas support gut health.",
            3),

        new ComboRule(
            "Banana",
            "Water",
            "Healthy Combo!",
            "Bananas provide potassium to balance hydration from water.",
            3),

        new ComboRule(
            "Fish",
            "Grapes",
            "Healthy Combo!",
            "Omega-3s in fish and antioxidants in grapes support heart health.",
            3),
    };

    public static Result RecalculateAndStore()
    {
        Result r = Calculate();

        sceneData.BasePoints = r.BasePoints;
        sceneData.BonusPoints = r.BonusPoints;
        sceneData.FinalPoints = r.FinalPoints;
        sceneData.TotalPoints = r.FinalPoints;

        sceneData.feedbackEntries = r.Entries ?? new List<sceneData.FeedbackEntry>();
        sceneData.feedbackMessages = r.Messages ?? new List<string>();

        sceneData.receiptLines = new List<string>();
        if (r.Receipt != null)
        {
            for (int i = 0; i < r.Receipt.Count; i++)
                sceneData.receiptLines.Add(r.Receipt[i].Text);
        }

        sceneData.receiptEntries = r.ReceiptEntries ?? new List<sceneData.ReceiptEntry>();
        sceneData.bonusBreakdowns = r.BonusBreakdowns ?? new List<sceneData.BonusBreakdownEntry>();
        sceneData.bonusSummaryText = r.BonusSummaryText ?? "";

        return r;
    }

    public static Result Calculate()
    {
        List<Item> picked = new List<Item>();
        picked.AddRange(sceneData.foodInSlots);
        picked.AddRange(sceneData.drinkInSlot);

        Dictionary<ItemType, int> seenTypeCount = new Dictionary<ItemType, int>();
        List<ReceiptLine> receiptLines = new List<ReceiptLine>();
        List<sceneData.ReceiptEntry> receiptEntries = new List<sceneData.ReceiptEntry>();

        int basePointsRaw = 0;
        bool hasDuplicates = false;

        List<Item> receiptOrder =
            (sceneData.receiptItems != null && sceneData.receiptItems.Count > 0)
            ? sceneData.receiptItems
            : picked;

        foreach (Item it in receiptOrder)
        {
            if (it == null) continue;

            if (!seenTypeCount.ContainsKey(it.type))
                seenTypeCount[it.type] = 0;

            seenTypeCount[it.type]++;

            bool penalized = seenTypeCount[it.type] >= 2;
            if (penalized) hasDuplicates = true;

            int adjusted = penalized ? Mathf.RoundToInt(it.points * 0.5f) : it.points;
            basePointsRaw += adjusted;

            receiptLines.Add(new ReceiptLine
            {
                Penalized = penalized,
                Text = $"{it.Food} - {adjusted} points"
            });

            receiptEntries.Add(new sceneData.ReceiptEntry
            {
                foodName = it.Food,
                category = it.type.ToString(),
                points = adjusted,
                penalized = penalized
            });
        }

        int basePoints = Mathf.Clamp(basePointsRaw, 0, MaxBasePoints);

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
                balancedBody = "You’ve built a balanced meal! Don’t forget—you can still choose a snack to enjoy. A little treat can be part of a healthy meal too!";
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

        HashSet<FoodColor> distinctColors = new HashSet<FoodColor>();
        foreach (Item it in picked)
        {
            if (it == null) continue;
            if (string.IsNullOrWhiteSpace(it.Food)) continue;

            if (FoodToColor.TryGetValue(it.Food.Trim(), out FoodColor color))
                distinctColors.Add(color);
        }

        int colorBonus = distinctColors.Count >= 4 ? 2 : 0;

        HashSet<string> normalizedFoods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> keyToDisplayName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Item it in picked)
        {
            if (it == null) continue;
            if (string.IsNullOrWhiteSpace(it.Food)) continue;

            string display = it.Food.Trim();
            string key = NormalizeFoodKey(display);

            if (string.IsNullOrEmpty(key)) continue;

            normalizedFoods.Add(key);
            if (!keyToDisplayName.ContainsKey(key))
                keyToDisplayName[key] = display;
        }

        int comboBonus = 0;
        HashSet<string> awardedCombos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<sceneData.BonusBreakdownEntry> bonusBreakdowns = new List<sceneData.BonusBreakdownEntry>();

        foreach (ComboRule rule in ComboRules)
        {
            if (!rule.IsSatisfied(normalizedFoods)) continue;
            if (awardedCombos.Contains(rule.Id)) continue;

            awardedCombos.Add(rule.Id);
            comboBonus += rule.Bonus;

            string aDisplay = keyToDisplayName.TryGetValue(rule.KeyA, out string aName) ? aName : rule.A;
            string bDisplay = keyToDisplayName.TryGetValue(rule.KeyB, out string bName) ? bName : rule.B;

            bonusBreakdowns.Add(new sceneData.BonusBreakdownEntry
            {
                title = $"{aDisplay} + {bDisplay}",
                body = rule.Body,
                bonus = rule.Bonus,
                isCombo = true
            });
        }

        if (balancedBonus > 0)
        {
            bonusBreakdowns.Add(new sceneData.BonusBreakdownEntry
            {
                title = balancedTitle,
                body = balancedBody,
                bonus = balancedBonus,
                isCombo = false
            });
        }

        if (colorBonus > 0)
        {
            bonusBreakdowns.Add(new sceneData.BonusBreakdownEntry
            {
                title = "Color Bonus!",
                body = "You picked four colorful foods—your meal is bright and packed with nutrients!",
                bonus = colorBonus,
                isCombo = false
            });
        }

        int uncappedBonus = balancedBonus + colorBonus + comboBonus;
        int bonusPoints = Mathf.Clamp(uncappedBonus, 0, MaxBonusPoints);
        int finalPoints = basePoints + bonusPoints;

        List<sceneData.FeedbackEntry> entries = new List<sceneData.FeedbackEntry>();

        if (comboBonus > 0)
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "You created a Healthy Combo!",
                body = "Great food pairings can help your body absorb nutrients better.",
                bonus = comboBonus
            });
        }
        else if (!string.IsNullOrEmpty(balancedTitle) && !string.IsNullOrEmpty(balancedBody))
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = balancedTitle,
                body = balancedBody,
                bonus = balancedBonus
            });
        }
        else
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "Nice choices!",
                body = "Try to include a few different food groups to build a more balanced meal.",
                bonus = 0
            });
        }

        if (hasDuplicates)
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "Nice choice!",
                body = "Try to balance your meal with different food groups next time.",
                bonus = 0
            });
        }

        if (colorBonus > 0)
        {
            entries.Add(new sceneData.FeedbackEntry
            {
                title = "Awesome choice!",
                body = "You picked four colorful foods—your meal is bright and packed with nutrients!",
                bonus = colorBonus
            });
        }

        List<string> legacyMessages = new List<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            sceneData.FeedbackEntry e = entries[i];
            string bonusPart = e.bonus > 0 ? $" (+{e.bonus} Bonus Points)" : "";
            legacyMessages.Add($"{e.title}{bonusPart} {e.body}");
        }

        string bonusSummaryText = BuildBonusSummaryText(bonusBreakdowns, finalPoints, bonusPoints);

        return new Result
        {
            BasePoints = basePoints,
            BonusPoints = bonusPoints,
            FinalPoints = finalPoints,
            Entries = entries,
            Messages = legacyMessages,
            Receipt = receiptLines,
            ReceiptEntries = receiptEntries,
            BonusBreakdowns = bonusBreakdowns,
            BonusSummaryText = bonusSummaryText
        };
    }

    private static string BuildBonusSummaryText(List<sceneData.BonusBreakdownEntry> breakdowns, int finalPoints, int bonusPoints)
    {
        if (breakdowns == null || breakdowns.Count == 0 || bonusPoints <= 0)
            return $"Great job! You earned {finalPoints} points!";

        List<sceneData.BonusBreakdownEntry> combos = new List<sceneData.BonusBreakdownEntry>();
        List<sceneData.BonusBreakdownEntry> otherBonuses = new List<sceneData.BonusBreakdownEntry>();

        for (int i = 0; i < breakdowns.Count; i++)
        {
            if (breakdowns[i].isCombo) combos.Add(breakdowns[i]);
            else otherBonuses.Add(breakdowns[i]);
        }

        StringBuilder sb = new StringBuilder();

        if (combos.Count > 0)
        {
            if (combos.Count == 1)
            {
                sb.Append("Healthy combo! ");
                sb.Append(combos[0].title);
                sb.Append(": ");
                sb.Append(combos[0].body);
            }
            else
            {
                sb.Append("Healthy combos! ");
                for (int i = 0; i < combos.Count; i++)
                {
                    sb.Append(combos[i].title);
                    sb.Append(": ");
                    sb.Append(combos[i].body);

                    if (i < combos.Count - 1)
                        sb.Append(" ");
                }
            }

            sb.Append(" You earned ");
            sb.Append(bonusPoints);
            sb.Append(" bonus points!");
            return sb.ToString().Trim();
        }

        if (otherBonuses.Count > 0)
        {
            for (int i = 0; i < otherBonuses.Count; i++)
            {
                sb.Append(otherBonuses[i].title);
                sb.Append(" ");
                sb.Append(otherBonuses[i].body);

                if (i < otherBonuses.Count - 1)
                    sb.Append(" ");
            }

            sb.Append(" You earned ");
            sb.Append(bonusPoints);
            sb.Append(" bonus points!");
            return sb.ToString().Trim();
        }

        return $"Great job! You earned {finalPoints} points!";
    }

    private static string NormalizeFoodKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";

        s = s.Trim().ToLowerInvariant();

        StringBuilder sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
        }

        string key = sb.ToString();

        if (key.Length > 3 && key.EndsWith("s"))
            key = key.Substring(0, key.Length - 1);

        return key;
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
        ItemType[] all =
        {
            ItemType.Protein,
            ItemType.Grains,
            ItemType.Fruit,
            ItemType.Snack,
            ItemType.Drink,
            ItemType.Vegetable,
            ItemType.Dairy
        };

        foreach (ItemType t in all)
        {
            if (!have.Contains(t))
                return t;
        }

        return ItemType.Snack;
    }

    private static List<ItemType> GetTwoMissingCategories(HashSet<ItemType> have)
    {
        List<ItemType> missing = new List<ItemType>();

        ItemType[] all =
        {
            ItemType.Protein,
            ItemType.Grains,
            ItemType.Fruit,
            ItemType.Snack,
            ItemType.Drink,
            ItemType.Vegetable,
            ItemType.Dairy
        };

        foreach (ItemType t in all)
        {
            if (!have.Contains(t))
            {
                missing.Add(t);
                if (missing.Count == 2)
                    break;
            }
        }

        while (missing.Count < 2)
            missing.Add(ItemType.Snack);

        return missing;
    }
}