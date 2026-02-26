using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class changeScene : MonoBehaviour
{
    public FoodSlot[] FoodSlotsFinal; //food slots to update finishScene
    public GameObject inventoryItemPrefab;
    public Text totalPointsTxt;
    public static bool hasPlayed = false;
    public void GoToSceneTwo()
    {
        hasPlayed = true;
        sceneData.TotalPoints = 0;
        sceneData.BasePoints = 0;
        sceneData.BonusPoints = 0;
        sceneData.FinalPoints = 0;
        sceneData.receiptFood.Clear();
        sceneData.receiptItems.Clear();
        sceneData.slotPositions.Clear();
        sceneData.foodInSlots.Clear();
        sceneData.drinkInSlot.Clear();
        sceneData.feedbackMessages.Clear();
        SceneManager.LoadScene("mainMenu");
    }

    // Transition from main menu to tutorial1 when "Play Game" is clicked
    public void GoToTutorial1()
    {
        if (!hasPlayed)
        {
            SceneManager.LoadScene("Tutorial1");
        }
        else
        {
            SceneManager.LoadScene("pickBox");
        }

    }

    public void LoadTutorial2()
    {
        SceneManager.LoadScene("Tutorial2");
    }

    public void LoadTutorial3()
    {
        SceneManager.LoadScene("Tutorial3");
    }

    public void LoadTutorial4()
    {
        SceneManager.LoadScene("Tutorial4");
    }

    public void LoadTutorial5()
    {
        SceneManager.LoadScene("Tutorial5");
    }
    public void LoadTutorial6()
    {
        SceneManager.LoadScene("Tutorial6");
    }

    public void GoToSelectionMenu()
    { //unused now
        SceneManager.LoadScene("SelectionMenu");
    }

    public void GotoBoxRed()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxRed");
    }

    public void GotoBoxYellow()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxYellow");
    }

    public void GotoBoxBlue()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxBlue");
    }

    public void GotoBoxPurple()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxPurple");
    }

    public void GotoBoxPink()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxPink");
    }

    public void GotoFinishBlue()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("FinishBlue");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void GotoFinishRed()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("FinishRed");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void GotoFinishYellow()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("FinishYellow");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void GotoFinishPink()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("FinishPink");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void GotoFinishPurple()
    {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("FinishPurple");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void GoToSceneFour()
    { // unsused now
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("finishScene");
        SceneManager.sceneLoaded += OnSceneLoaded; //check scene is loaded
    }

    public void GoToPickBox()
    {
        SceneManager.LoadScene("pickBox");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        totalPointsTxt = GameObject.Find("finalPoints").GetComponent<Text>();
        displayTotalPoints();
        displayFinalFoods();
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe after handling
    }

    //NOTE: EVERY METHOD BELOW IS FOR finishScene
    public void displayTotalPoints()
    { //show final points in finishScene
        if (totalPointsTxt != null)
        {
            // Prefer FinalPoints (includes bonuses)
            int pts = (sceneData.FinalPoints != 0 || sceneData.BonusPoints != 0 || sceneData.BasePoints != 0)
                ? sceneData.FinalPoints
                : sceneData.TotalPoints;

            totalPointsTxt.text = ($"{pts}");
            Debug.Log("FINAL POINTS SUCCESS!");
        }
        else
        {
            Debug.Log("finalPoints is not assigned in the Inspector");
        }
    }

    public void displayFinalFoods()
    { //show all foods chosen
        foreach (var item in sceneData.foodInSlots)
        {
            AddItem(item);
            Debug.Log("added items"); //check if loop was run
        }
    }

    public bool AddItem(Item item)
    { //same as lunchBoxManager
        //Find any empty slot
        for (int i = 0; i < FoodSlotsFinal.Length; i++)
        {
            FoodSlot slot = FoodSlotsFinal[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item, FoodSlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    void ApplyFinalScoreWithBonus()
    {
        // Centralized scoring now handles ALL bonuses and penalties.
        var r = LunchboxScoring.RecalculateAndStore();
        Debug.Log($"Base: {r.BasePoints}, Bonus: {r.BonusPoints}, Final: {r.FinalPoints}");
    }
}