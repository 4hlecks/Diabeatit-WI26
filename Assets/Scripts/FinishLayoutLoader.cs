using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishLayoutLoader : MonoBehaviour
{
    public Transform finishRoot;
    public GameObject[] finishLayoutPrefabs;

    public GameObject inventoryItemPrefab;
    public Text totalPointsTxt;

    private GameObject spawnedLayout;

    void Start()
    {
        int index = sceneData.SelectedLunchboxIndex;

        if (finishRoot == null || finishLayoutPrefabs == null || finishLayoutPrefabs.Length == 0)
            return;

        if (index < 0 || index >= finishLayoutPrefabs.Length)
            return;

        LunchboxScoring.RecalculateAndStore();

        spawnedLayout = Instantiate(finishLayoutPrefabs[index], finishRoot);

        UpdatePoints();
        PopulateDrink();
        PopulateFoods();
    }

    void UpdatePoints()
    {
        if (totalPointsTxt == null)
        {
            GameObject obj = GameObject.Find("finalPoints");
            if (obj != null)
                totalPointsTxt = obj.GetComponent<Text>();
        }

        if (totalPointsTxt == null) return;

        totalPointsTxt.text = sceneData.FinalPoints.ToString();
    }

    void PopulateDrink()
    {
        if (spawnedLayout == null) return;

        Transform coasterTf = spawnedLayout.transform.Find("CoasterSlot");
        if (coasterTf == null) return;

        FoodSlot coasterSlot = coasterTf.GetComponent<FoodSlot>();
        if (coasterSlot == null) return;

        if (sceneData.drinkInSlot == null || sceneData.drinkInSlot.Count == 0) return;

        Item drink = sceneData.drinkInSlot[0];
        SpawnItem(drink, coasterSlot);
    }

    void PopulateFoods()
    {
        if (spawnedLayout == null) return;

        FoodSlot[] allSlots = spawnedLayout.GetComponentsInChildren<FoodSlot>(true);
        if (allSlots == null || allSlots.Length == 0) return;

        List<FoodSlot> normalSlots = new List<FoodSlot>();

        Transform coasterTf = spawnedLayout.transform.Find("CoasterSlot");
        FoodSlot coasterSlot = coasterTf != null ? coasterTf.GetComponent<FoodSlot>() : null;

        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] != coasterSlot)
                normalSlots.Add(allSlots[i]);
        }

        int count = Mathf.Min(sceneData.foodInSlots.Count, sceneData.slotPositions.Count);
        for (int i = 0; i < count; i++)
        {
            int slotIndex = sceneData.slotPositions[i];
            if (slotIndex < 0 || slotIndex >= normalSlots.Count)
                continue;

            if (sceneData.foodInSlots[i] == null)
                continue;

            SpawnItem(sceneData.foodInSlots[i], normalSlots[slotIndex]);
        }
    }

    void SpawnItem(Item item, FoodSlot slot)
    {
        if (item == null || slot == null || inventoryItemPrefab == null) return;

        GameObject newItemGo = Instantiate(inventoryItemPrefab);
        newItemGo.transform.SetParent(slot.transform, false);

        InventoryItem inv = newItemGo.GetComponent<InventoryItem>();
        inv.InitializeItem(item);
    }
}