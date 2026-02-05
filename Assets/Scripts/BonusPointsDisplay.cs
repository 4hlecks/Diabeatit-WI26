using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BonusPointsDisplay : MonoBehaviour
{
    public FoodSlot[] foodSlots;           // Array of food slots in the scene
    public FoodSlot coasterSlot;           // Slot for drink
    public GameObject inventoryItemPrefab; // Prefab for inventory items
    public Text finalPointsText;           // Text showing final points
    public Text bonusDescriptionText;      // Text for showing bonus combinations
    public string targetScene;             // Next scene to load
    
    private List<string> bonusCombos = new List<string>();
    private int currentIndex = 0;
    
    void Start()
    {
        // Display total points
        if (finalPointsText != null)
        {
            finalPointsText.text = sceneData.TotalPoints.ToString();
        }
        
        // Display food items
        DisplayFoodItems();
        
        // Find all bonus combinations the player collected
        FindBonusCombinations();
        if (bonusCombos.Count > 0)
        {
            DisplayCurrentCombo();
        }
        else
        {
            bonusDescriptionText.text = "No bonus pairings found. Try combining different foods next time!";
        }
    }
    
    void DisplayFoodItems()
    {
        // Display food items in slots
        int slotIndex = 0;
        foreach (Item item in sceneData.foodInSlots)
        {
            if (slotIndex < foodSlots.Length)
            {
                SpawnNewItem(item, foodSlots[slotIndex]);
                slotIndex++;
            }
        }
        
        // Display drink if there is one
        if (sceneData.drinkInSlot.Count > 0 && coasterSlot != null)
        {
            SpawnNewItem(sceneData.drinkInSlot[0], coasterSlot);
        }
    }

    void SpawnNewItem(Item item, FoodSlot slot)
    {
        // Added null check
        if (slot == null || inventoryItemPrefab == null)
        {
            Debug.LogError("Cannot spawn item: " + (slot == null ? "slot is null" : "prefab is null"));
            return;
        }
        
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        if (inventoryItem != null)
            inventoryItem.InitializeItem(item);
        else
            Debug.LogError("InventoryItem component not found on prefab");
    }
    
    void FindBonusCombinations()
    {
        List<string> selected = sceneData.receiptFood;
        
        for (int i = 0; i < selected.Count; i++)
        {
            for (int j = i + 1; j < selected.Count; j++)
            {
                string a = selected[i];
                string b = selected[j];

                if ((a.StartsWith("Steak") && b.StartsWith("Bell Pepper")) || (a.StartsWith("Bell Pepper") && b.StartsWith("Steak")))
                    bonusCombos.Add("Steak + Bell Pepper: Vitamin C in bell peppers helps absorb iron from steak. +3 points");
                
                else if ((a.StartsWith("Tofu") && b.StartsWith("Orange")) || (a.StartsWith("Orange") && b.StartsWith("Tofu")))
                    bonusCombos.Add("Tofu + Orange: Vitamin C in oranges boosts iron absorption from tofu. +3 points");
                
                else if ((a.StartsWith("Avocado") && b.StartsWith("Carrots")) || (a.StartsWith("Carrots") && b.StartsWith("Avocado")))
                    bonusCombos.Add("Avocado + Carrots: Healthy fats in avocado help absorb vitamin A from carrots. +3 points");
                
                else if ((a.StartsWith("Cheese") && b.StartsWith("Whole Grain Bread")) || (a.StartsWith("Whole Grain Bread") && b.StartsWith("Cheese")))
                    bonusCombos.Add("Cheese + Whole Grain Bread: Whole grains help the body absorb calcium from cheese. +3 points");
                
                else if ((a.StartsWith("Eggs") && b.StartsWith("Quinoa")) || (a.StartsWith("Quinoa") && b.StartsWith("Eggs")))
                    bonusCombos.Add("Eggs + Quinoa: A strong combo of protein and whole grains for steady energy. +3 points");
                
                else if ((a.StartsWith("Yogurt") && b.StartsWith("Banana")) || (a.StartsWith("Banana") && b.StartsWith("Yogurt")))
                    bonusCombos.Add("Yogurt + Banana: Probiotics in yogurt + fiber in banana support gut health. +3 points");
                
                else if ((a.StartsWith("Banana") && b.StartsWith("Water")) || (a.StartsWith("Water") && b.StartsWith("Banana")))
                    bonusCombos.Add("Banana + Water: Bananas provide potassium to balance hydration from water. +3 points");
                
                else if ((a.StartsWith("Fish") && b.StartsWith("Grapes")) || (a.StartsWith("Grapes") && b.StartsWith("Fish")))
                    bonusCombos.Add("Fish + Grapes: Omega-3s in fish + antioxidants in grapes support heart health. +3 points");
            }
        }
    }
    
    void DisplayCurrentCombo()
    {
        if (currentIndex < bonusCombos.Count)
        {
            bonusDescriptionText.text = bonusCombos[currentIndex];
        }
    }
    
    public void OnNextButtonClick()
    {
        currentIndex++;
        
        // If we've shown all combos, go to the target scene
        if (currentIndex >= bonusCombos.Count)
        {
            SceneManager.LoadScene(targetScene);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // Otherwise, show the next combo
            DisplayCurrentCombo();
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Text pointsText = GameObject.Find("finalPoints")?.GetComponent<Text>();
        if (pointsText != null)
        {
            pointsText.text = sceneData.TotalPoints.ToString();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}