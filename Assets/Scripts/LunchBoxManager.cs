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
        if (item == null) return false;

        if (item.type == ItemType.Drink)
        {
            if (coasterSlot == null) return false;

            InventoryItem itemInCoaster = coasterSlot.GetComponentInChildren<InventoryItem>();
            if (itemInCoaster == null)
            {
                SpawnNewItem(item, coasterSlot);

                sceneData.drinkInSlot.Add(item);
                sceneData.receiptItems.Add(item);
                sceneData.receiptFood.Add(item.Food);

                UpdateScriptText(item);
                updateTotalPoints();
                return true;
            }

            return false;
        }

        if (FoodSlots == null) return false;

        for (int i = 0; i < FoodSlots.Length; i++)
        {
            FoodSlot slot = FoodSlots[i];
            if (slot == null) continue;

            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                sceneData.slotPositions.Add(i);

                SpawnNewItem(item, slot);
                sceneData.foodInSlots.Add(item);
                sceneData.receiptItems.Add(item);
                sceneData.receiptFood.Add(item.Food);

                UpdateScriptText(item);
                updateTotalPoints();
                return true;
            }
        }

        return false;
    }

    private void SpawnNewItem(Item item, FoodSlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab);
        newItemGo.transform.SetParent(slot.transform, false);

        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    private void UpdateScriptText(Item item)
    {
        if (scriptName == null)
        {
            GameObject obj = GameObject.Find("allFood");
            if (obj != null)
                scriptName = obj.GetComponent<Text>();
        }

        if (scriptName != null)
            scriptName.text = item.script;
    }

    public void DeleteInventoryItem(InventoryItem inv)
    {
        if (inv == null || inv.item == null)
            return;

        Transform parent = inv.transform.parent;
        FoodSlot parentSlot = parent != null ? parent.GetComponent<FoodSlot>() : null;

        Item removed = inv.item;

        if (parentSlot != null && parentSlot == coasterSlot)
        {
            RemoveOneMatching(sceneData.drinkInSlot, removed);
            RemoveOneMatching(sceneData.receiptItems, removed);

            if (!string.IsNullOrEmpty(removed.Food))
                sceneData.receiptFood.Remove(removed.Food);

            Destroy(inv.gameObject);
            updateTotalPoints();
            return;
        }

        int removedFoodIndex = sceneData.foodInSlots.IndexOf(removed);
        if (removedFoodIndex >= 0)
        {
            sceneData.foodInSlots.RemoveAt(removedFoodIndex);

            if (removedFoodIndex < sceneData.slotPositions.Count)
                sceneData.slotPositions.RemoveAt(removedFoodIndex);
        }
        else
        {
            RemoveOneMatching(sceneData.foodInSlots, removed);

            if (parentSlot != null && FoodSlots != null)
            {
                for (int i = 0; i < FoodSlots.Length; i++)
                {
                    if (FoodSlots[i] == parentSlot)
                    {
                        int posIndex = sceneData.slotPositions.IndexOf(i);
                        if (posIndex >= 0)
                            sceneData.slotPositions.RemoveAt(posIndex);
                        break;
                    }
                }
            }
        }

        RemoveOneMatching(sceneData.receiptItems, removed);

        if (!string.IsNullOrEmpty(removed.Food))
            sceneData.receiptFood.Remove(removed.Food);

        Destroy(inv.gameObject);
        updateTotalPoints();
    }

    private void RemoveOneMatching<T>(System.Collections.Generic.List<T> list, T value)
    {
        if (list == null) return;

        int index = list.IndexOf(value);
        if (index >= 0)
            list.RemoveAt(index);
    }

    public void updateTotalPoints()
    {
        var result = LunchboxScoring.RecalculateAndStore();

        if (totalPointsTxt == null)
        {
            GameObject obj = GameObject.Find("tmpPoints");
            if (obj != null)
                totalPointsTxt = obj.GetComponent<Text>();
        }

        if (totalPointsTxt != null)
            totalPointsTxt.text = result.FinalPoints.ToString();
    }
}