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
    public void GoToSceneTwo() {
        hasPlayed = true;
        sceneData.TotalPoints = 0;
        sceneData.receiptFood.Clear();
        sceneData.slotPositions.Clear();
        sceneData.foodInSlots.Clear();
        sceneData.drinkInSlot.Clear();
        SceneManager.LoadScene("mainMenu");
    }

        // Transition from main menu to tutorial1 when "Play Game" is clicked
    public void GoToTutorial1() {
        if (!hasPlayed) {
            SceneManager.LoadScene("Tutorial1");
        } else {
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




    public void GoToSelectionMenu() { //unused now
        SceneManager.LoadScene("SelectionMenu");
    }
    
    public void GotoBoxRed() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxRed");
    }

    public void GotoBoxYellow() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxYellow");
    }

    public void GotoBoxBlue() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxBlue");
    }

    public void GotoBoxPurple() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxPurple");
    }

    public void GotoBoxPink() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BoxPink");
    }

    public void GotoFinishBlue() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BonusPointsBlue");
    }

    public void GotoFinishRed() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BonusPointsRed");
    }

    public void GotoFinishYellow() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BonusPointsYellow");
    }

    public void GotoFinishPink() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BonusPointsPink");
    }

    public void GotoFinishPurple() {
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("BonusPointsPurple");
    }

    public void GoToSceneFour() { // unsused now
        ApplyFinalScoreWithBonus();
        SceneManager.LoadScene("finishScene");
        SceneManager.sceneLoaded += OnSceneLoaded; //check scene is loaded
    }

    public void GoToPickBox()
    {
        SceneManager.LoadScene("pickBox");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    totalPointsTxt = GameObject.Find("finalPoints").GetComponent<Text>();
    displayTotalPoints();
    displayFinalFoods();
    SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe after handling
}
    
    //NOTE: EVERY METHOD BELOW IS FOR finishScene
    public void displayTotalPoints() { //show final points in finishScene
        if (totalPointsTxt != null) {
            totalPointsTxt.text = ($"{sceneData.TotalPoints}");
            Debug.Log("FINAL POINTS SUCCESS!");
        } else {
            Debug.Log("finalPoints is not assigned in the Inspector");
        }
    }

    public void displayFinalFoods() { //show all foods chosen
        foreach(var item in sceneData.foodInSlots ) {
            AddItem(item);
            Debug.Log("added items"); //check if loop was run
        }
    }

    public bool AddItem(Item item){ //same as lunchBoxManager
        //Find any empty slot
        for (int i = 0; i < FoodSlotsFinal.Length; i++){
            FoodSlot slot = FoodSlotsFinal[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null){
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item, FoodSlot slot){
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    void ApplyFinalScoreWithBonus()
    {

        int bonus = CalculateBonusPairingPoints();
        sceneData.TotalPoints += bonus;



        Debug.Log($"Base: {sceneData.TotalPoints}, Bonus: {bonus}, Final: {sceneData.TotalPoints}");
    }

    /* 
    This is a function created to implement bonus points for lunch boxes with strategic food pairings.
    This is done to encourage combining foods with complementary nutrients.
    */
    int CalculateBonusPairingPoints()
    {
        int bonus = 0;
        List<string> selected = sceneData.receiptFood;

        for (int i = 0; i < selected.Count; i++)
        {
            for (int j = i + 1; j < selected.Count; j++)
            {
                string a = selected[i];
                string b = selected[j];

                if ((a.StartsWith("Steak") && b.StartsWith("Bell Pepper")) || (a.StartsWith("Bell Pepper") && b.StartsWith("Steak")))
                    bonus += 3;
                else if ((a.StartsWith("Tofu") && b.StartsWith("Orange")) || (a.StartsWith("Orange") && b.StartsWith("Tofu")))
                    bonus += 3;
                else if ((a.StartsWith("Avocado") && b.StartsWith("Carrots")) || (a.StartsWith("Carrots") && b.StartsWith("Avocado")))
                    bonus += 3;
                else if ((a.StartsWith("Cheese") && b.StartsWith("Whole Grain Bread")) || (a.StartsWith("Whole Grain Bread") && b.StartsWith("Cheese")))
                    bonus += 3;
                else if ((a.StartsWith("Eggs") && b.StartsWith("Quinoa")) || (a.StartsWith("Quinoa") && b.StartsWith("Eggs")))
                    bonus += 3;
                else if ((a.StartsWith("Yogurt") && b.StartsWith("Banana")) || (a.StartsWith("Banana") && b.StartsWith("Yogurt")))
                    bonus += 3;
                else if ((a.StartsWith("Banana") && b.StartsWith("Water")) || (a.StartsWith("Water") && b.StartsWith("Banana")))
                    bonus += 3;
                else if ((a.StartsWith("Fish") && b.StartsWith("Grapes")) || (a.StartsWith("Grapes") && b.StartsWith("Fish")))
                    bonus += 3;
            }
        }

        return bonus;
    }


}