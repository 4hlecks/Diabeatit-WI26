using UnityEngine;

public class delete : MonoBehaviour
{
    private LunchBoxManager lunchBoxManager;

    void Start()
    {
        lunchBoxManager = FindObjectOfType<LunchBoxManager>();

        if (lunchBoxManager == null)
            Debug.LogError("delete.cs: Could not find LunchBoxManager in scene.");
    }

    public void RemoveItem(int num)
    {
        if (lunchBoxManager == null)
            lunchBoxManager = FindObjectOfType<LunchBoxManager>();

        if (lunchBoxManager == null)
            return;

        FoodSlot[] slots = lunchBoxManager.FoodSlots;
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("delete.cs: LunchBoxManager.FoodSlots is empty. Layout may not be loaded yet.");
            return;
        }

        if (num < 0 || num >= slots.Length)
        {
            Debug.LogWarning("delete.cs: RemoveItem num out of range: " + num);
            return;
        }

        FoodSlot slot = slots[num];
        if (slot == null)
            return;

        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null || itemInSlot.item == null)
            return;

        Item removedItem = itemInSlot.item;

        int removedFoodIndex = sceneData.foodInSlots.IndexOf(removedItem);
        if (removedFoodIndex >= 0)
        {
            sceneData.foodInSlots.RemoveAt(removedFoodIndex);

            if (removedFoodIndex < sceneData.slotPositions.Count)
                sceneData.slotPositions.RemoveAt(removedFoodIndex);
        }
        else
        {
            sceneData.foodInSlots.Remove(removedItem);

            int posIndex = sceneData.slotPositions.IndexOf(num);
            if (posIndex >= 0)
                sceneData.slotPositions.RemoveAt(posIndex);
        }

        RemoveOneMatching(sceneData.receiptItems, removedItem);

        if (!string.IsNullOrEmpty(removedItem.Food))
            sceneData.receiptFood.Remove(removedItem.Food);

        Destroy(itemInSlot.gameObject);

        lunchBoxManager.updateTotalPoints();
        Debug.Log("Remove Item: Deleted " + removedItem.type + " with " + removedItem.points + " points. Total points: " + sceneData.TotalPoints);
    }

    public void RemoveDrink()
    {
        if (lunchBoxManager == null)
            lunchBoxManager = FindObjectOfType<LunchBoxManager>();

        if (lunchBoxManager == null)
            return;

        FoodSlot coaster = lunchBoxManager.coasterSlot;
        if (coaster == null)
        {
            Debug.LogWarning("delete.cs: LunchBoxManager.coasterSlot is null. Layout may not be loaded yet.");
            return;
        }

        InventoryItem itemInSlot = coaster.GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null || itemInSlot.item == null)
            return;

        Item removedDrink = itemInSlot.item;

        RemoveOneMatching(sceneData.drinkInSlot, removedDrink);
        RemoveOneMatching(sceneData.receiptItems, removedDrink);

        if (!string.IsNullOrEmpty(removedDrink.Food))
            sceneData.receiptFood.Remove(removedDrink.Food);

        Destroy(itemInSlot.gameObject);

        lunchBoxManager.updateTotalPoints();
        Debug.Log("Remove Drink: Deleted " + removedDrink.type + " with " + removedDrink.points + " points. Total points: " + sceneData.TotalPoints);
    }

    private void RemoveOneMatching<T>(System.Collections.Generic.List<T> list, T value)
    {
        if (list == null) return;

        int index = list.IndexOf(value);
        if (index >= 0)
            list.RemoveAt(index);
    }
}