using System.Collections.Generic;
using UnityEngine;

public class LunchboxLayoutLoader : MonoBehaviour
{
    public Transform layoutRoot;
    public GameObject[] layoutPrefabs;
    public LunchBoxManager lunchBoxManager;

    GameObject spawnedLayout;

    void Start()
    {
        int index = sceneData.SelectedLunchboxIndex;

        if (layoutRoot == null)
        {
            Debug.LogError("LunchboxLayoutLoader: layoutRoot not assigned.");
            return;
        }

        if (layoutPrefabs == null || layoutPrefabs.Length == 0)
        {
            Debug.LogError("LunchboxLayoutLoader: layoutPrefabs empty.");
            return;
        }

        if (index < 0 || index >= layoutPrefabs.Length)
        {
            Debug.LogError("LunchboxLayoutLoader: SelectedLunchboxIndex out of range: " + index);
            return;
        }

        if (lunchBoxManager == null)
        {
            Debug.LogError("LunchboxLayoutLoader: lunchBoxManager not assigned.");
            return;
        }

        // Clear existing children to prevent duplicates if something was left in the scene
        for (int i = layoutRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(layoutRoot.GetChild(i).gameObject);
        }

        spawnedLayout = Instantiate(layoutPrefabs[index], layoutRoot);

        // Find coaster slot inside the spawned layout
        Transform coasterTf = spawnedLayout.transform.Find("CoasterSlot");
        if (coasterTf == null)
        {
            Debug.LogError("LunchboxLayoutLoader: Could not find 'CoasterSlot' in spawned layout prefab.");
            return;
        }

        FoodSlot coasterSlot = coasterTf.GetComponent<FoodSlot>();
        if (coasterSlot == null)
        {
            Debug.LogError("LunchboxLayoutLoader: 'CoasterSlot' has no FoodSlot component.");
            return;
        }

        // Find all FoodSlots, then exclude coaster slot from the normal FoodSlots array
        FoodSlot[] allSlots = spawnedLayout.GetComponentsInChildren<FoodSlot>(true);
        if (allSlots == null || allSlots.Length == 0)
        {
            Debug.LogError("LunchboxLayoutLoader: No FoodSlot found in spawned layout.");
            return;
        }

        List<FoodSlot> normalSlots = new List<FoodSlot>();
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] == coasterSlot) continue;
            normalSlots.Add(allSlots[i]);
        }

        lunchBoxManager.coasterSlot = coasterSlot;
        lunchBoxManager.FoodSlots = normalSlots.ToArray();

        // Restore previously placed food items (e.g. when going back from FinishLunchbox)
        for (int i = 0; i < sceneData.slotPositions.Count && i < sceneData.foodInSlots.Count; i++)
        {
            int slotIdx = sceneData.slotPositions[i];
            if (slotIdx >= 0 && slotIdx < normalSlots.Count)
            {
                FoodSlot slot = normalSlots[slotIdx];
                if (slot.GetComponentInChildren<InventoryItem>() == null)
                {
                    GameObject newItemGo = Instantiate(lunchBoxManager.inventoryItemPrefab);
                    newItemGo.transform.SetParent(slot.transform, false);
                    InventoryItem inv = newItemGo.GetComponent<InventoryItem>();
                    inv.InitializeItem(sceneData.foodInSlots[i]);
                }
            }
        }

        // Restore drink in coaster
        for (int i = 0; i < sceneData.drinkInSlot.Count; i++)
        {
            if (coasterSlot.GetComponentInChildren<InventoryItem>() == null)
            {
                GameObject newItemGo = Instantiate(lunchBoxManager.inventoryItemPrefab);
                newItemGo.transform.SetParent(coasterSlot.transform, false);
                InventoryItem inv = newItemGo.GetComponent<InventoryItem>();
                inv.InitializeItem(sceneData.drinkInSlot[i]);
            }
        }

        // Update the points display
        lunchBoxManager.updateTotalPoints();
    }
}