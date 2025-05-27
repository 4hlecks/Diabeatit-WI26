using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LunchBoxDisplay : MonoBehaviour
{
    public FoodSlot coasterSlot;
    public FoodSlot[] FoodSlots;  // Array of the FoodSlots in finalScene
    public GameObject inventoryItemPrefab; // Prefab for InventoryItem
    public Text food1, food2, food3, food4, food5, food6; //declare foods
    public Text finalPointsText; // Reference to the final score text UI element
    public GameObject dialogPopupPrefab; // Prefab for the dialog popup

    private GameObject dialogInstance;
    private Text dialogText;

    // Gives us the ability to have each item have its own clickable dialong in the display scene
    void Awake() 
    {
        if (dialogPopupPrefab != null)
        {

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                dialogInstance = Instantiate(dialogPopupPrefab, canvas.transform);
                
                RectTransform rectTransform = dialogInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.7f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.7f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = new Vector2(-162f, -75f);
                }

                dialogText = dialogInstance.GetComponentInChildren<Text>();
                
                // Hide the dialog initially
                dialogInstance.SetActive(false);
            }
        }
    }

    void Start() { LoadDrink(); LoadItemsFromSceneData();}


    void LoadDrink()
    {
        //make sure there is a item in the slot
        if (sceneData.drinkInSlot.Count == 1)
            SpawnDrinkInSlot(sceneData.drinkInSlot[0], coasterSlot);
    }

    void LoadItemsFromSceneData()
    {
        // Make sure the number of items matches the number of slots
        if (sceneData.foodInSlots.Count == sceneData.slotPositions.Count)
        {
            for (int i = 0; i < sceneData.foodInSlots.Count; i++)
            {
                // Get the index of the slot where the item was placed
                int slotIndex = sceneData.slotPositions[i];
                // Ensure the slot exists and the index is valid
                if (slotIndex >= 0 && slotIndex < FoodSlots.Length)
                {
                    FoodSlot slot = FoodSlots[slotIndex]; // Get the corresponding slot
                    // Instantiate and place the item in the slot
                    SpawnItemInSlot(sceneData.foodInSlots[i], slot);
                }
            }

            //print in receipt
            if (sceneData.foodInSlots.Count == sceneData.slotPositions.Count)
            {
                List<Item> receiptItems = new List<Item>();
                receiptItems.AddRange(sceneData.foodInSlots);
                receiptItems.AddRange(sceneData.drinkInSlot); // add drink if any

                for (int i = 0; i < sceneData.receiptFood.Count; i++)
                {
                    string foodObjectName = $"food{i + 1}_txt"; //dynamically build food1_txt, food2_txt, etc.
                    Text foodText = GameObject.Find(foodObjectName)?.GetComponent<Text>(); //find text gameObject if u cannot find it manually
                    if (foodText == null) { Debug.LogError($"{foodObjectName} not found in scene!"); continue; }

                    string foodName = sceneData.receiptFood[i];
                    Item currentItem = receiptItems.Find(item => foodName.StartsWith(item.Food));
                    bool isPenalized = false;

                    if (currentItem != null && currentItem.type != ItemType.Drink)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            string prevFoodName = sceneData.receiptFood[j];
                            Item previousItem = receiptItems.Find(item => prevFoodName.StartsWith(item.Food));
                            if (previousItem != null && previousItem.type == currentItem.type)
                            {
                                isPenalized = true;
                                break;
                            }
                        }
                    }

                    if (isPenalized)
                    {
                        foodText.text = foodName + " (-0.5x)";
                        foodText.color = Color.red;
                    }
                    else
                    {
                        foodText.text = foodName;
                        foodText.color = Color.black;
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Mismatch between number of items and slot positions! foodInSlots.Count: {sceneData.foodInSlots.Count}, slotPositions.Count: {sceneData.slotPositions.Count}");
        }
    }

    void SpawnItemInSlot(Item item, FoodSlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item); // Initialize the item in the slot
        newItemGo.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        AddClickHandler(newItemGo, item);
    }

    void SpawnDrinkInSlot(Item item, FoodSlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item); // Initialize the item in the slot
        newItemGo.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        // Add the click handler
        AddClickHandler(newItemGo, item);
    }

    void AddClickHandler(GameObject itemObject, Item itemData)
    {
        ItemClickHandler clickHandler = itemObject.AddComponent<ItemClickHandler>();
        clickHandler.itemData = itemData;
        
        clickHandler.dialogPopup = dialogInstance;
        clickHandler.dialogText = dialogText;
    }
}
