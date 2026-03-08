using UnityEngine;
using UnityEngine.SceneManagement;

public class changeScene : MonoBehaviour
{
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
        sceneData.receiptLines.Clear();

        SceneManager.LoadScene("mainMenu");
    }

    public void PlayButtonGo()
    {
        if (!hasPlayed)
            SceneManager.LoadScene("Tutorial1");
        else
            SceneManager.LoadScene("pickBox");
    }
    public void GoToTutorial1()
    {
        SceneManager.LoadScene("Tutorial1");
    }

    public void LoadTutorial2() { SceneManager.LoadScene("Tutorial2"); }
    public void LoadTutorial3() { SceneManager.LoadScene("Tutorial3"); }
    public void LoadTutorial4() { SceneManager.LoadScene("Tutorial4"); }
    public void LoadTutorial5() { SceneManager.LoadScene("Tutorial5"); }
    public void LoadTutorial6() { SceneManager.LoadScene("Tutorial6"); }

    public void GoToPickBox()
    {
        SceneManager.LoadScene("pickBox");
    }

    public void ChooseBoxAndStart(int lunchboxIndex)
    {
        sceneData.SelectedLunchboxIndex = lunchboxIndex;
        SceneManager.LoadScene("BuildLunchbox");
    }

    public void GoToFinish()
    {
        var r = LunchboxScoring.RecalculateAndStore();
        Debug.Log("Base: " + r.BasePoints + ", Bonus: " + r.BonusPoints + ", Final: " + r.FinalPoints);

        SceneManager.LoadScene("FinishLunchbox");
    }
}