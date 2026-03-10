using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LunchboxReportLoader : MonoBehaviour
{
    [Header("Report Text (use ONE of these)")]
    public TMP_Text reportTMP;
    public Text reportText;

    [Header("Totals")]
    public TMP_Text totalPointsTMP;
    public Text totalPointsText;

    [Header("Auto Size")]
    public bool enableAutoSize = true;
    public float fontSizeMax = 28f;
    public float fontSizeMin = 14f;

    private void Start()
    {
        LunchboxScoring.RecalculateAndStore();
        ApplyAutoSize();
        LoadReport();
        LoadTotal();
    }

    private void ApplyAutoSize()
    {
        if (!enableAutoSize) return;

        if (reportTMP != null)
        {
            reportTMP.enableAutoSizing = true;
            reportTMP.fontSizeMax = fontSizeMax;
            reportTMP.fontSizeMin = fontSizeMin;
            reportTMP.overflowMode = TextOverflowModes.Overflow;
        }

        if (reportText != null)
        {
            reportText.resizeTextForBestFit = true;
            reportText.resizeTextMaxSize = Mathf.RoundToInt(fontSizeMax);
            reportText.resizeTextMinSize = Mathf.RoundToInt(fontSizeMin);
            reportText.horizontalOverflow = HorizontalWrapMode.Wrap;
            reportText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        if (totalPointsTMP != null)
        {
            totalPointsTMP.enableAutoSizing = true;
            totalPointsTMP.fontSizeMax = fontSizeMax;
            totalPointsTMP.fontSizeMin = fontSizeMin;
        }

        if (totalPointsText != null)
        {
            totalPointsText.resizeTextForBestFit = true;
            totalPointsText.resizeTextMaxSize = Mathf.RoundToInt(fontSizeMax);
            totalPointsText.resizeTextMinSize = Mathf.RoundToInt(fontSizeMin);
        }
    }

    private void LoadReport()
    {
        StringBuilder sb = new StringBuilder();

        bool hasBonus = sceneData.BonusPoints > 0 &&
                        sceneData.bonusBreakdowns != null &&
                        sceneData.bonusBreakdowns.Count > 0;

        if (hasBonus)
        {
            for (int i = 0; i < sceneData.bonusBreakdowns.Count; i++)
            {
                var bonus = sceneData.bonusBreakdowns[i];

                sb.Append(bonus.title);
                sb.Append(" - Bonus +");
                sb.Append(bonus.bonus);
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(bonus.body))
                {
                    sb.AppendLine(bonus.body);
                }

                if (i < sceneData.bonusBreakdowns.Count - 1)
                    sb.AppendLine();
            }
        }
        else
        {
            if (sceneData.receiptEntries != null)
            {
                for (int i = 0; i < sceneData.receiptEntries.Count; i++)
                {
                    var entry = sceneData.receiptEntries[i];

                    sb.Append(entry.foodName);
                    sb.Append(" - ");
                    sb.Append(entry.points);
                    sb.Append(" points");

                    if (entry.penalized)
                        sb.Append(" (duplicate category)");

                    if (i < sceneData.receiptEntries.Count - 1)
                        sb.AppendLine();
                }
            }
        }

        string finalText = sb.ToString().Trim();

        if (string.IsNullOrWhiteSpace(finalText))
            finalText = "No report data available.";

        if (reportTMP != null)
            reportTMP.text = finalText;

        if (reportText != null)
            reportText.text = finalText;
    }

    private void LoadTotal()
    {
        string totalString = "Total Points: " + sceneData.FinalPoints.ToString();

        if (totalPointsTMP != null)
            totalPointsTMP.text = totalString;

        if (totalPointsText != null)
            totalPointsText.text = totalString;
    }
}