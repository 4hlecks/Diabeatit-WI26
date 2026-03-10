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
        sceneData.feedbackEntries.Clear();
        sceneData.receiptLines.Clear();
        sceneData.receiptEntries.Clear();
        sceneData.bonusBreakdowns.Clear();
        sceneData.bonusSummaryText = "";

        SceneManager.LoadScene("mainMenu");
    }

    public void PlayButtonGo()
    {
        if (!hasPlayed)
            SceneManager.LoadScene("Tutorial1");
        else
            SceneManager.LoadScene("pickBox");
    }

    public void GoToTutorial1() { SceneManager.LoadScene("Tutorial1"); }
    public void LoadTutorial2() { SceneManager.LoadScene("Tutorial2"); }
    public void LoadTutorial3() { SceneManager.LoadScene("Tutorial3"); }
    public void LoadTutorial4() { SceneManager.LoadScene("Tutorial4"); }
    public void LoadTutorial5() { SceneManager.LoadScene("Tutorial5"); }
    public void LoadTutorial6() { SceneManager.LoadScene("Tutorial6"); }
    public void GoToWhatis1() { SceneManager.LoadScene("Whatis1"); }
    public void GoToWhatis2() { SceneManager.LoadScene("Whatis2"); }
    public void GoToWhatis3() { SceneManager.LoadScene("Whatis3"); }
    public void GoToWhatis4() { SceneManager.LoadScene("Whatis4"); }
    public void GoToWhatis5() { SceneManager.LoadScene("Whatis5"); }
    public void GoToWhatis6() { SceneManager.LoadScene("Whatis6"); }
    public void GoToWhatis7() { SceneManager.LoadScene("Whatis7"); }
    public void GoToWhatis8() { SceneManager.LoadScene("Whatis8"); }
    public void GoToWhatis9() { SceneManager.LoadScene("Whatis9"); }
    public void GoToWhatis10() { SceneManager.LoadScene("Whatis10"); }
    public void GoToWhatis11() { SceneManager.LoadScene("Whatis11"); }
    public void GoToWhatis12() { SceneManager.LoadScene("Whatis12"); }
    public void GoToMainMenu() { SceneManager.LoadScene("mainMenu"); }

    public void GoToPickBox()
    {
        SceneManager.LoadScene("pickBox");
    }

    public void ChooseBoxAndStart(int lunchboxIndex)
    {
        sceneData.SelectedLunchboxIndex = lunchboxIndex;
        SceneManager.LoadScene("BuildLunchbox");
    }

    public void GoBackToBuildLunchbox()
    {
        SceneManager.LoadScene("BuildLunchbox");
    }

    public void GoToBonusPoints()
    {
        var r = LunchboxScoring.RecalculateAndStore();
        Debug.Log("Base: " + r.BasePoints + ", Bonus: " + r.BonusPoints + ", Final: " + r.FinalPoints);
        SceneManager.LoadScene("BonusPoints");
    }

    public void GoToFinish()
    {
        var r = LunchboxScoring.RecalculateAndStore();
        Debug.Log("Base: " + r.BasePoints + ", Bonus: " + r.BonusPoints + ", Final: " + r.FinalPoints);
        SceneManager.LoadScene("FinishLunchbox");
    }

    public void GoToLunchboxReport()
    {
        var r = LunchboxScoring.RecalculateAndStore();
        Debug.Log("Opening report. Base: " + r.BasePoints + ", Bonus: " + r.BonusPoints + ", Final: " + r.FinalPoints);
        SceneManager.LoadScene("LunchboxReport");
    }

    public void GoBackToFinish()
    {
        SceneManager.LoadScene("FinishLunchbox");
    }
}