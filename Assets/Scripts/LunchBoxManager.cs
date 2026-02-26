using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LunchBoxManager : MonoBehaviour
{
    public FoodSlot[] FoodSlots;
    public GameObject inventoryItemPrefab;
    public Text totalPointsTxt; //update this to display final points
    public Text scriptName; //"local" variable you're updating
    public GameObject speechBubble; // Reference to the speech bubble GameObject
    public Text allFoodText; // Reference to the allFood Text component
    //private bool hasProteinItem = false;

    // add coasterSlot
    public FoodSlot coasterSlot;

    public bool AddItem(Item item)
    {
        // ADDED CODE HERE
        // If the item is a drink, force it into the coaster slot
        if (item.type == ItemType.Drink)
        {
            InventoryItem itemInCoaster = coasterSlot.GetComponentInChildren<InventoryItem>();
            if (itemInCoaster == null) // Ensure it's empty before placing
            {
                SpawnNewItem(item, coasterSlot);
                sceneData.drinkInSlot.Add(item);
                sceneData.receiptFood.Add(item.Food);
                sceneData.receiptItems.Add(item);

                //Gluccy's script
                scriptName = GameObject.Find("allFood").GetComponent<Text>(); //JOANN EDIT 
                scriptName.text = item.script;

                updateTotalPoints(); // Update total points visually
                Debug.Log($"Added {item.type} with {item.points} points. Total points: {sceneData.TotalPoints}");
                Debug.Log($"Item text: {item.Food}");

                return true;
            }
            else
            {
                Debug.Log("Coaster is already occupied!");
                return false; // Prevent multiple drinks on the coaster
            }
        }

        // Find any empty slot
        for (int i = 0; i < FoodSlots.Length; i++)
        {
            FoodSlot slot = FoodSlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                sceneData.slotPositions.Add(i); // Save the index of the filled slot

                SpawnNewItem(item, slot); // Add item to the slot
                sceneData.foodInSlots.Add(item);
                sceneData.receiptFood.Add(item.Food);
                sceneData.receiptItems.Add(item);

                //Gluccy's script
                scriptName = GameObject.Find("allFood").GetComponent<Text>(); //JOANN EDIT 
                scriptName.text = item.script;

                updateTotalPoints(); //update total points visually in THIS scene

                Debug.Log($"Added {item.type} with {item.points} points. Total points: {sceneData.TotalPoints}"); //debug
                Debug.Log($"Item text: {item.Food}");

                return true;
            }
        }
        return false;
    }

    /*
    void SpawnNewItem(Item item, FoodSlot slot)
    need to drag the slot in the unity interface to use

    Instantiate
    spawns item at slot position

    GetComponent
    gets the spawned items characteristics

    Initialize 
    allocates the spawned items characteristics to the item
    */
    void SpawnNewItem(Item item, FoodSlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }


    public void updateTotalPoints()
    {
        // Use centralized scoring rules
        var result = LunchboxScoring.RecalculateAndStore();
        int normalizedPoints = result.FinalPoints;

        if (totalPointsTxt != null)
        { //update total points
            totalPointsTxt.text = ($"{normalizedPoints}");
            Debug.Log("set text");
        }
        else
        {
            Debug.Log("failed to set text");
            totalPointsTxt = GameObject.Find("tmpPoints").GetComponent<Text>(); //find text gameObject if u cannot find it manually
            totalPointsTxt.text = ($"{normalizedPoints}"); //then update total points
        }
    }
}