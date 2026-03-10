using UnityEngine;
using UnityEngine.UI;

public class ReceiptLoader : MonoBehaviour
{
    public Transform receiptRoot;

    void Start()
    {
        LunchboxScoring.RecalculateAndStore();

        if (receiptRoot == null)
        {
            GameObject obj = GameObject.Find("Receipt");
            if (obj != null)
                receiptRoot = obj.transform;
        }

        if (receiptRoot == null)
        {
            Debug.LogWarning("ReceiptLoader: receiptRoot not assigned.");
            return;
        }

        LoadPointsBreakdown();
        LoadReceiptLines();
    }

    void LoadPointsBreakdown()
    {
        SetTextIfExists("basePoints_txt", "Base Total: " + sceneData.BasePoints + " points");
        SetTextIfExists("bonusPoints_txt", "Bonus Total: " + sceneData.BonusPoints + " points");
        SetTextIfExists("finalPoints_txt", "Final Total: " + sceneData.FinalPoints + " points");
    }

    void LoadReceiptLines()
    {
        for (int i = 1; i <= 7; i++)
            SetTextIfExists("food" + i + "_txt", "");

        if (sceneData.receiptLines == null) return;

        for (int i = 0; i < sceneData.receiptLines.Count && i < 7; i++)
            SetTextIfExists("food" + (i + 1) + "_txt", sceneData.receiptLines[i]);
    }

    void SetTextIfExists(string childName, string value)
    {
        Transform t = receiptRoot.Find(childName);
        if (t == null) return;

        Text txt = t.GetComponent<Text>();
        if (txt == null) return;

        txt.text = value;
    }
}