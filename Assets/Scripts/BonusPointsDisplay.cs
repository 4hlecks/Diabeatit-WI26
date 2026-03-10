using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BonusPointsDisplay : MonoBehaviour
{
    [Header("Score Text")]
    public Text basePointsText;
    public Text bonusPointsText;
    public Text finalPointsText;

    [Header("Bonus Message")]
    public Text bonusDescriptionText;

    [Header("Next Scene")]
    public string nextScene = "FinishLunchbox";

    private readonly string[] noItemsMessages =
    {
        "Your body needs food to stay strong and energized!",
        "Make sure to add food to your lunchbox next time!",
        "Don’t forget to pack something to eat next time!",
        "Try adding food next time so your lunchbox can fuel you up!"
    };

    private readonly string[] snackMessages =
    {
        "Your lunchbox has a lot of snacks. Try adding other food groups too!",
        "Snacks can be fun, but try mixing in more balanced foods next time.",
        "You picked a lot of snacks this round. See if you can add more variety next time!",
        "Too many snacks can make your lunch less balanced. Try adding other foods too.",
        "Snacks are okay, but your lunchbox could use more than just snack foods."
    };

    private readonly string[] fewItemsMessages =
    {
        "Try adding a few more items next time.",
        "You could still add some more to your lunchbox.",
        "Your lunchbox could use a few more foods.",
        "There’s still room to add more items for a fuller meal."
    };

    private readonly string[] neutralMessages =
    {
        "You didn’t earn a bonus combo this time, but keep experimenting with different foods!",
        "No combo bonus this round, but you still made some food choices.",
        "You didn’t get a combo this time. Try mixing up your lunchbox next round!",
        "No bonus combo yet. Try different food pairings next time!"
    };

    void Start()
    {
        LunchboxScoring.RecalculateAndStore();
        DisplayPoints();
        DisplayBonusSummary();
    }

    void DisplayPoints()
    {
        if (basePointsText != null)
            basePointsText.text = sceneData.BasePoints.ToString();

        if (bonusPointsText != null)
            bonusPointsText.text = sceneData.BonusPoints.ToString();

        if (finalPointsText != null)
            finalPointsText.text = sceneData.FinalPoints.ToString();
    }

    void DisplayBonusSummary()
    {
        if (bonusDescriptionText == null) return;

        string summary = sceneData.bonusSummaryText;

        // If there IS a combo/bonus, always show the combo summary text.
        if (sceneData.BonusPoints > 0 && !string.IsNullOrWhiteSpace(summary))
        {
            bonusDescriptionText.text = summary;
            return;
        }

        int itemCount = GetPackedItemCount();

        if (itemCount == 0)
        {
            bonusDescriptionText.text = GetRandomMessage(noItemsMessages);
            return;
        }

        if (HasOnlySnacks() || HasDuplicateSnacks())
        {
            bonusDescriptionText.text = GetRandomMessage(snackMessages);
            return;
        }

        if (itemCount <= 3)
        {
            bonusDescriptionText.text = GetRandomMessage(fewItemsMessages);
            return;
        }

        bonusDescriptionText.text = GetRandomMessage(neutralMessages);
    }

    int GetPackedItemCount()
    {
        int foodCount = sceneData.foodInSlots != null ? sceneData.foodInSlots.Count : 0;
        int drinkCount = sceneData.drinkInSlot != null ? sceneData.drinkInSlot.Count : 0;
        return foodCount + drinkCount;
    }

    bool HasOnlySnacks()
    {
        if (sceneData.foodInSlots == null || sceneData.foodInSlots.Count == 0)
            return false;

        for (int i = 0; i < sceneData.foodInSlots.Count; i++)
        {
            Item item = sceneData.foodInSlots[i];
            if (item == null) continue;

            if (item.type != ItemType.Snack)
                return false;
        }

        return true;
    }

    bool HasDuplicateSnacks()
    {
        if (sceneData.foodInSlots == null || sceneData.foodInSlots.Count == 0)
            return false;

        int snackCount = 0;

        for (int i = 0; i < sceneData.foodInSlots.Count; i++)
        {
            Item item = sceneData.foodInSlots[i];
            if (item == null) continue;

            if (item.type == ItemType.Snack)
                snackCount++;
        }

        return snackCount >= 2;
    }

    string GetRandomMessage(string[] messages)
    {
        if (messages == null || messages.Length == 0)
            return "";

        int randomIndex = Random.Range(0, messages.Length);
        return messages[randomIndex];
    }

    public void OnNextButtonClick()
    {
        SceneManager.LoadScene(nextScene);
    }
}