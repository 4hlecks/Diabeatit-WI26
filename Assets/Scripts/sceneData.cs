using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class sceneData
{
    // Scoring
    public static int BasePoints = 0;   // points after category-duplicate penalties, capped to 100
    public static int BonusPoints = 0;  // capped (see LunchboxScoring)
    public static int FinalPoints = 0;  // BasePoints + BonusPoints

    // Backwards-compat (some scenes still reference TotalPoints)
    public static int TotalPoints = 0;
    public static List<int> slotPositions = new List<int>();
    public static List<Item> foodInSlots = new List<Item>(); //to keep track of chosen foods to transfer to finishScene
    // Receipt data (keep the order items were picked)
    public static List<string> receiptFood = new List<string>();
    public static List<Item> receiptItems = new List<Item>();

    // Finish-scene scripts (page through these with a Next button)
    public static List<string> feedbackMessages = new List<string>();
    public static List<Item> drinkInSlot = new List<Item>();


}