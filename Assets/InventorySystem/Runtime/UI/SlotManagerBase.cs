using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class SlotManagerBase<TSlot> : MonoBehaviour where TSlot : class
{
    [SerializeField] protected UIDocument uiDocument;

    protected IPanel panel;
    protected Vector2 mousePositionPanel;

    protected TSlot draggedSlot;
    protected bool isDragging;

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError($"[{GetType().Name}] UIDocument is not assigned!", this);
            enabled = false;
            return;
        }

        panel = uiDocument.rootVisualElement.panel;
    }

    protected virtual void Update()
    {
        if (panel == null || Mouse.current == null) return;

        CalculateMousePosition();

        TSlot slotUnderMouse = PickSlot(mousePositionPanel);

        HandleDrag();
        HandlePointerButtons(slotUnderMouse);
        HandleHoverTooltip(slotUnderMouse);
    }

    private void HandlePointerButtons(TSlot slotUnderMouse)
    {
        var mouse = Mouse.current;

        if (slotUnderMouse != null && HasItem(slotUnderMouse) && mouse.leftButton.wasPressedThisFrame)
        {
            BeginDrag(slotUnderMouse);
        }

        if (slotUnderMouse != null)
        {
            OnExtraButtons(slotUnderMouse);
        }

        if (mouse.leftButton.wasReleasedThisFrame && draggedSlot != null)
        {
            OnDragRelease(draggedSlot, slotUnderMouse, mousePositionPanel);
            EndDrag();
        }
    }

    private void BeginDrag(TSlot slot)
    {
        draggedSlot = slot;
        isDragging = true;
        ShowDragIcon(slot);
        HideTooltip();
    }

    private void EndDrag()
    {
        isDragging = false;
        draggedSlot = null;
        HideDragIcon();
    }

    private void HandleDrag()
    {
        if (isDragging && InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.MoveDrag(mousePositionPanel);
        }
    }

    private void HandleHoverTooltip(TSlot slotUnderMouse)
    {
        if (isDragging) return;

        if (slotUnderMouse != null)
        {
            ShowTooltip(slotUnderMouse);
        }
        else
        {
            HideTooltip();
        }
    }

    private void CalculateMousePosition()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        screenPos.y = Screen.height - screenPos.y;
        mousePositionPanel = RuntimePanelUtils.ScreenToPanel(panel, screenPos);
    }

    private TSlot PickSlot(Vector2 panelPosition)
    {
        VisualElement element = panel.Pick(panelPosition);

        while (element != null && !element.ClassListContains(SlotCssClass))
        {
            element = element.parent;
        }

        if (element == null)
        {
            return null;
        }

        if (element is TSlot slot)
        {
            return slot;
        }

        return element.userData as TSlot;
    }

    protected abstract string SlotCssClass { get; }
    protected abstract bool HasItem(TSlot slot);
    protected abstract void ShowDragIcon(TSlot slot);
    protected abstract void HideDragIcon();
    protected abstract void ShowTooltip(TSlot slot);
    protected abstract void HideTooltip();
    protected abstract void OnDragRelease(TSlot from, TSlot to, Vector2 mousePanelPosition);
    protected virtual void OnExtraButtons(TSlot slot) { }

    protected void ForceEndDrag()
    {
        EndDrag();
    }
}
