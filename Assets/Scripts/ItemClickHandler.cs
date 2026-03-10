using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Item itemData;
    public GameObject dialogPopup;
    public Text dialogText;
    private bool isDialogActive = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dialogPopup != null)
        {
            if (isDialogActive)
            {
                // Hide the dialog if it's already showing
                dialogPopup.SetActive(false);
                isDialogActive = false;
            }
            else
            {
                // Show the dialog with item-specific text
                dialogPopup.SetActive(true);
                if (dialogText != null && itemData != null)
                {
                    dialogText.text = itemData.script; // Use the script from the item
                }
                isDialogActive = true;
                
                // Hide other active dialogs (optional)
                HideOtherDialogs();
            }
        }
    }
    
    // Hide other active dialogs when this one is shown
    private void HideOtherDialogs()
    {
        ItemClickHandler[] handlers = FindObjectsOfType<ItemClickHandler>();
        foreach (ItemClickHandler handler in handlers)
        {
            if (handler != this && handler.isDialogActive)
            {
                handler.dialogPopup.SetActive(false);
                handler.isDialogActive = false;
            }
        }
    }
}