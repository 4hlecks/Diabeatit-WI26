using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LunchBoxManager : MonoBehaviour
{
    public FoodSlot[] FoodSlots;
    public GameObject inventoryItemPrefab;
    public Text totalPointsTxt;
    public Text scriptName;

    public FoodSlot coasterSlot;

    public bool AddItem(Item item)
    {
        if (item.type == ItemType.Drink)
        {
            InventoryItem itemInCoaster = coasterSlot.GetComponentInChildren<InventoryItem>();
            if (itemInCoaster == null)
            {
                SpawnNewItem(item, coasterSlot);

                sceneData.drinkInSlot.Add(item);
                sceneData.receiptFood.Add(item.Food);

                UpdateScriptText(item);
                updateTotalPoints();
                return true;
            }
            return false;
        }

        for (int i = 0; i < FoodSlots.Length; i++)
        {
            FoodSlot slot = FoodSlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                sceneData.slotPositions.Add(i);

                SpawnNewItem(item, slot);
                sceneData.foodInSlots.Add(item);
                sceneData.receiptFood.Add(item.Food);

                UpdateScriptText(item);
                updateTotalPoints();
                return true;
            }
        }

        return false;
    }

    void SpawnNewItem(Item item, FoodSlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab);
        newItemGo.transform.SetParent(slot.transform, false);

        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    void UpdateScriptText(Item item)
    {
        if (scriptName == null)
        {
            GameObject obj = GameObject.Find("allFood");
            if (obj != null) scriptName = obj.GetComponent<Text>();
        }

        if (scriptName != null)
            scriptName.text = item.script;
    }

    // Called by InventoryItem when clicked
    public void DeleteInventoryItem(InventoryItem inv)
    {
        if (inv == null || inv.item == null)
            return;

        Transform parent = inv.transform.parent;
        FoodSlot parentSlot = parent != null ? parent.GetComponent<FoodSlot>() : null;

        Item removed = inv.item;

        // If it was in the coaster slot (drink)
        if (parentSlot != null && parentSlot == coasterSlot)
        {
            sceneData.drinkInSlot.Remove(removed);
            if (!string.IsNullOrEmpty(removed.Food))
                sceneData.receiptFood.Remove(removed.Food);

            Destroy(inv.gameObject);
            updateTotalPoints();
            return;
        }

        // Normal food slot
        sceneData.foodInSlots.Remove(removed);
        if (!string.IsNullOrEmpty(removed.Food))
            sceneData.receiptFood.Remove(removed.Food);

        // Remove matching slot position entry
        // Find which FoodSlots index this parentSlot is
        if (parentSlot != null && FoodSlots != null)
        {
            int slotIndex = -1;
            for (int i = 0; i < FoodSlots.Length; i++)
            {
                if (FoodSlots[i] == parentSlot)
                {
                    slotIndex = i;
                    break;
                }
            }

            if (slotIndex >= 0 && sceneData.slotPositions != null)
            {
                int posListIndex = sceneData.slotPositions.IndexOf(slotIndex);
                if (posListIndex >= 0)
                    sceneData.slotPositions.RemoveAt(posListIndex);
            }
        }

        Destroy(inv.gameObject);
        updateTotalPoints();
    }

    public void updateTotalPoints()
    {
        const int maxPoints = 100;
        int total = 0;

        foreach (Item it in sceneData.foodInSlots)
            total += it.points;

        foreach (Item it in sceneData.drinkInSlot)
            total += it.points;

        int normalizedPoints = Mathf.RoundToInt((total / (float)maxPoints) * 100);
        normalizedPoints = Mathf.Clamp(normalizedPoints, 0, 100);
        sceneData.TotalPoints = normalizedPoints;

        if (totalPointsTxt == null)
        {
            GameObject obj = GameObject.Find("tmpPoints");
            if (obj != null) totalPointsTxt = obj.GetComponent<Text>();
        }

        if (totalPointsTxt != null)
            totalPointsTxt.text = normalizedPoints.ToString();
    }
}