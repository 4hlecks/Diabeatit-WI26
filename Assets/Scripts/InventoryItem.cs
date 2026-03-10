using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image image;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Item item;

    private Canvas rootCanvas;
    private RectTransform rt;
    private CanvasGroup canvasGroup;

    private Vector3 worldScaleBeforeDrag;
    private Vector2 savedSizeDelta;

    private bool isDragging = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void InitializeItem(Item newItem)
    {
        item = newItem;

        if (image != null)
        {
            image.sprite = newItem.image;
            image.preserveAspect = true;
            image.raycastTarget = true;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (rt != null)
            savedSizeDelta = rt.sizeDelta;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        LunchBoxManager manager = FindObjectOfType<LunchBoxManager>();
        if (manager == null)
        {
            Debug.LogWarning("InventoryItem: Could not find LunchBoxManager.");
            return;
        }

        manager.DeleteInventoryItem(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        parentAfterDrag = transform.parent;

        if (rt != null)
            savedSizeDelta = rt.sizeDelta;

        // Capture world scale so visual size stays the same after reparenting
        worldScaleBeforeDrag = transform.lossyScale;

        // Let raycasts pass through while dragging
        canvasGroup.blocksRaycasts = false;

        Transform dragParent = rootCanvas != null ? rootCanvas.transform : transform.root;

        // Reparent (keep world position)
        transform.SetParent(dragParent, true);
        transform.SetAsLastSibling();

        // Restore exact world scale under new parent
        ApplyWorldScale(worldScaleBeforeDrag);

        // Restore sizeDelta in case layout changes it
        if (rt != null)
            rt.sizeDelta = savedSizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        // Keep size stable
        if (rt != null)
            rt.sizeDelta = savedSizeDelta;

        // Keep world scale stable
        ApplyWorldScale(worldScaleBeforeDrag);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // If no drop handler moved it, return to original parent
        Transform dragParent = rootCanvas != null ? rootCanvas.transform : transform.root;
        if (transform.parent == dragParent && parentAfterDrag != null)
        {
            transform.SetParent(parentAfterDrag, true);
        }

        // After returning, keep the same world scale it had before drag
        ApplyWorldScale(worldScaleBeforeDrag);

        if (rt != null)
            rt.sizeDelta = savedSizeDelta;

        canvasGroup.blocksRaycasts = true;
        isDragging = false;
    }

    private void ApplyWorldScale(Vector3 desiredWorldScale)
    {
        Transform p = transform.parent;
        if (p == null) return;

        Vector3 parentWorldScale = p.lossyScale;

        float sx = parentWorldScale.x != 0f ? desiredWorldScale.x / parentWorldScale.x : 1f;
        float sy = parentWorldScale.y != 0f ? desiredWorldScale.y / parentWorldScale.y : 1f;
        float sz = parentWorldScale.z != 0f ? desiredWorldScale.z / parentWorldScale.z : 1f;

        transform.localScale = new Vector3(sx, sy, sz);
    }
}