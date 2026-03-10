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

        if (string.IsNullOrWhiteSpace(summary))
            summary = "Great job! You earned " + sceneData.FinalPoints + " points!";

        bonusDescriptionText.text = summary;
    }

    public void OnNextButtonClick()
    {
        SceneManager.LoadScene(nextScene);
    }
}