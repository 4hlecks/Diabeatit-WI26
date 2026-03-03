using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishLayoutLoader : MonoBehaviour
{
    public Transform finishRoot;
    public GameObject[] finishLayoutPrefabs;

    public GameObject inventoryItemPrefab;
    public Text totalPointsTxt;

    GameObject spawnedLayout;

    void Start()
    {
        int index = sceneData.SelectedLunchboxIndex;

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

        int pts =
            (sceneData.FinalPoints != 0 || sceneData.BonusPoints != 0 || sceneData.BasePoints != 0)
            ? sceneData.FinalPoints
            : sceneData.TotalPoints;

        totalPointsTxt.text = pts.ToString();
    }

    void PopulateDrink()
    {
        Transform coasterTf = spawnedLayout.transform.Find("CoasterSlot");
        if (coasterTf == null) return;

        FoodSlot coasterSlot = coasterTf.GetComponent<FoodSlot>();
        if (coasterSlot == null) return;

        if (sceneData.drinkInSlot.Count == 0) return;

        Item drink = sceneData.drinkInSlot[0];

        GameObject newItemGo = Instantiate(inventoryItemPrefab);
        newItemGo.transform.SetParent(coasterSlot.transform, false);

        InventoryItem inv = newItemGo.GetComponent<InventoryItem>();
        inv.InitializeItem(drink);
    }

    void PopulateFoods()
    {
        FoodSlot[] allSlots = spawnedLayout.GetComponentsInChildren<FoodSlot>(true);

        List<FoodSlot> normalSlots = new List<FoodSlot>();

        Transform coasterTf = spawnedLayout.transform.Find("CoasterSlot");
        FoodSlot coasterSlot = coasterTf != null ? coasterTf.GetComponent<FoodSlot>() : null;

        foreach (FoodSlot slot in allSlots)
        {
            if (slot != coasterSlot)
                normalSlots.Add(slot);
        }

        for (int i = 0; i < sceneData.foodInSlots.Count; i++)
        {
            if (i >= normalSlots.Count) break;

            GameObject newItemGo = Instantiate(inventoryItemPrefab);
            newItemGo.transform.SetParent(normalSlots[i].transform, false);

            InventoryItem inv = newItemGo.GetComponent<InventoryItem>();
            inv.InitializeItem(sceneData.foodInSlots[i]);
        }
    }
}