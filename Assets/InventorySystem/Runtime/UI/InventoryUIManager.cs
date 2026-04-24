using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    public UIDocument UIDocument => uiDocument;
    public bool IsTooltipVisible => isTooltipVisible;

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float tooltipDelay = 1f;

    private VisualElement root;
    private IPanel panel;

    // Tooltip
    private VisualElement tooltipContainer;
    private VisualElement tooltipImage;
    private VisualElement tooltipBackground;
    private Label tooltipTitle;
    private Label tooltipDescription;
    private InventorySlot currentTooltipSlot;
    private float hoverTimer;
    private bool isTooltipVisible;
    private float tooltipRarityOffset;

    // Drag
    private VisualElement dragElement;
    private bool isDragVisible;

    // Registered inventories
    private readonly List<VisualElement> registeredInventoryUIs = new();

    private bool isSetUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        EnsureSetup();
    }

    private void Update()
    {
        EnsureSetup();
        AnimateTooltipRarity();
    }

    public void EnsureSetup()
    {
        if (isSetUp) return;
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if (root == null) return;

        panel = root.panel;

        SetupTooltip();
        SetupDragElement();
        isSetUp = true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #region Setup

    private void SetupTooltip()
    {
        tooltipContainer = root.Q<VisualElement>("TooltipContainer");

        if (tooltipContainer != null)
        {
            tooltipImage = tooltipContainer.Q<VisualElement>("TooltipImage");
            tooltipBackground = tooltipContainer.Q<VisualElement>("TooltipBackground");
            tooltipTitle = tooltipContainer.Q<Label>("TooltipTitle");
            tooltipDescription = tooltipContainer.Q<Label>("TooltipDescription");
        }
        else
        {
            CreateDefaultTooltip();
        }

        ShowTooltipElement(false);
    }

    private void SetupDragElement()
    {
        dragElement = root.Q<VisualElement>("DragElement");

        if (dragElement == null)
        {
            CreateDefaultDragElement();
        }

        dragElement.style.position = Position.Absolute;
        dragElement.pickingMode = PickingMode.Ignore;
        ShowDragElement(false);
    }

    private void CreateDefaultTooltip()
    {
        tooltipContainer = new VisualElement();
        tooltipContainer.name = "TooltipContainer";
        tooltipContainer.style.position = Position.Absolute;
        tooltipContainer.pickingMode = PickingMode.Ignore;
        tooltipContainer.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        tooltipContainer.style.borderTopLeftRadius = 8;
        tooltipContainer.style.borderTopRightRadius = 8;
        tooltipContainer.style.borderBottomLeftRadius = 8;
        tooltipContainer.style.borderBottomRightRadius = 8;
        tooltipContainer.style.paddingTop = 10;
        tooltipContainer.style.paddingBottom = 10;
        tooltipContainer.style.paddingLeft = 10;
        tooltipContainer.style.paddingRight = 10;
        tooltipContainer.style.width = 200;
        tooltipContainer.style.borderTopWidth = 1;
        tooltipContainer.style.borderRightWidth = 1;
        tooltipContainer.style.borderBottomWidth = 1;
        tooltipContainer.style.borderLeftWidth = 1;
        tooltipContainer.style.borderTopColor = new Color(1f, 1f, 1f, 0.5f);
        tooltipContainer.style.borderRightColor = new Color(1f, 1f, 1f, 0.5f);
        tooltipContainer.style.borderBottomColor = new Color(1f, 1f, 1f, 0.5f);
        tooltipContainer.style.borderLeftColor = new Color(1f, 1f, 1f, 0.5f);

        tooltipBackground = new VisualElement();
        tooltipBackground.name = "TooltipBackground";
        tooltipBackground.style.position = Position.Absolute;
        tooltipBackground.style.top = 0;
        tooltipBackground.style.left = 0;
        tooltipBackground.style.right = 0;
        tooltipBackground.style.bottom = 0;
        tooltipBackground.style.borderTopLeftRadius = 8;
        tooltipBackground.style.borderTopRightRadius = 8;
        tooltipBackground.style.borderBottomLeftRadius = 8;
        tooltipBackground.style.borderBottomRightRadius = 8;
        tooltipBackground.style.opacity = 0.3f;
        tooltipBackground.pickingMode = PickingMode.Ignore;
        tooltipContainer.Add(tooltipBackground);

        tooltipImage = new VisualElement();
        tooltipImage.name = "TooltipImage";
        tooltipImage.style.width = 64;
        tooltipImage.style.height = 64;
        tooltipImage.style.alignSelf = Align.Center;
        tooltipImage.style.marginBottom = 4;
        tooltipContainer.Add(tooltipImage);

        tooltipTitle = new Label();
        tooltipTitle.name = "TooltipTitle";
        tooltipTitle.style.color = Color.white;
        tooltipTitle.style.fontSize = 16;
        tooltipTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        tooltipTitle.style.marginTop = 4;
        tooltipContainer.Add(tooltipTitle);

        tooltipDescription = new Label();
        tooltipDescription.name = "TooltipDescription";
        tooltipDescription.style.color = new Color(0.8f, 0.8f, 0.8f);
        tooltipDescription.style.fontSize = 12;
        tooltipDescription.style.marginTop = 4;
        tooltipDescription.style.whiteSpace = WhiteSpace.Normal;
        tooltipContainer.Add(tooltipDescription);

        root.Add(tooltipContainer);
    }

    private void CreateDefaultDragElement()
    {
        dragElement = new VisualElement();
        dragElement.name = "DragElement";
        dragElement.style.width = 48;
        dragElement.style.height = 48;
        root.Add(dragElement);
    }

    #endregion

    #region Inventory Registration

    public void RegisterInventoryUI(VisualElement inventoryUI)
    {
        if (inventoryUI != null && !registeredInventoryUIs.Contains(inventoryUI))
        {
            registeredInventoryUIs.Add(inventoryUI);
        }
    }

    public void UnregisterInventoryUI(VisualElement inventoryUI)
    {
        registeredInventoryUIs.Remove(inventoryUI);
    }

    public bool IsInAnyInventoryBounds(Vector2 panelPos)
    {
        for (int i = registeredInventoryUIs.Count - 1; i >= 0; i--)
        {
            var inventoryUI = registeredInventoryUIs[i];
            if (inventoryUI == null)
            {
                registeredInventoryUIs.RemoveAt(i);
                continue;
            }

            if (inventoryUI.resolvedStyle.display == DisplayStyle.Flex
                && inventoryUI.worldBound.Contains(panelPos))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Tooltip

    public void TooltipUpdate(InventorySlot slot, Vector2 mousePosPanel, float deltaTime)
    {
        if (currentTooltipSlot != slot)
        {
            currentTooltipSlot = slot;
            hoverTimer = 0f;
            HideTooltip();
            return;
        }

        if (slot == null || slot.Item == null)
        {
            HideTooltip();
            currentTooltipSlot = null;
            return;
        }

        hoverTimer += deltaTime;

        if (!isTooltipVisible && hoverTimer >= tooltipDelay)
        {
            ShowTooltip(slot, mousePosPanel);
        }
    }

    public void HideTooltip()
    {
        if (isTooltipVisible)
        {
            ShowTooltipElement(false);
            isTooltipVisible = false;
            tooltipRarityOffset = 0f;
            currentTooltipSlot = null;
        }
    }

    private void ShowTooltip(InventorySlot slot, Vector2 mousePosPanel)
    {
        if (slot == null || slot.Item == null)
        {
            return;
        }

        tooltipRarityOffset = 0f;
        tooltipImage.style.backgroundImage = slot.Item.Icon != null
            ? new StyleBackground(slot.Item.Icon)
            : new StyleBackground();
        tooltipBackground.style.backgroundImage = slot.Item.RarityBackground != null
            ? new StyleBackground(slot.Item.RarityBackground)
            : new StyleBackground();
        tooltipTitle.text = slot.Item.ItemName;
        tooltipDescription.text = slot.Item.Description;

        ShowTooltipElement(true);

        float tooltipWidth = tooltipContainer.resolvedStyle.width;
        float tooltipHeight = tooltipContainer.resolvedStyle.height;

        float offset = 12f;
        float left = mousePosPanel.x + offset;
        float top = mousePosPanel.y + offset;

        if (panel != null)
        {
            float panelWidth = panel.visualTree.resolvedStyle.width;
            float panelHeight = panel.visualTree.resolvedStyle.height;

            if (left + tooltipWidth > panelWidth)
            {
                left = Mathf.Max(panelWidth - tooltipWidth, 0);
            }

            if (top + tooltipHeight > panelHeight)
            {
                top = Mathf.Max(panelHeight - tooltipHeight, 0);
            }
        }

        tooltipContainer.style.left = Mathf.Max(left, 0);
        tooltipContainer.style.top = Mathf.Max(top, 0);

        isTooltipVisible = true;
    }

    private void ShowTooltipElement(bool show)
    {
        if (tooltipContainer != null)
        {
            tooltipContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void AnimateTooltipRarity()
    {
        if (!isTooltipVisible || tooltipBackground == null) return;
        if (tooltipBackground.style.backgroundImage.value.texture == null) return;

        tooltipRarityOffset += Time.deltaTime * 10f;
        tooltipBackground.style.backgroundPositionY = new StyleBackgroundPosition(
            new BackgroundPosition(BackgroundPositionKeyword.Top, new Length(tooltipRarityOffset, LengthUnit.Pixel))
        );
    }

    #endregion

    #region Drag

    public void ShowDrag(Texture2D icon, Vector2 mousePosPanel)
    {
        if (dragElement == null)
        {
            return;
        }

        if (icon != null)
        {
            dragElement.style.backgroundImage = new StyleBackground(icon);
            dragElement.style.backgroundColor = StyleKeyword.None;
        }
        else
        {
            dragElement.style.backgroundImage = new StyleBackground();
            dragElement.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        }

        MoveDrag(mousePosPanel);
        ShowDragElement(true);
        isDragVisible = true;
    }

    public void MoveDrag(Vector2 mousePosPanel)
    {
        if (!isDragVisible || dragElement == null)
        {
            return;
        }

        float w = dragElement.resolvedStyle.width > 0 ? dragElement.resolvedStyle.width : 48f;
        float h = dragElement.resolvedStyle.height > 0 ? dragElement.resolvedStyle.height : 48f;

        dragElement.style.left = mousePosPanel.x - w * 0.5f;
        dragElement.style.top = mousePosPanel.y - h * 0.5f;
    }

    public void HideDrag()
    {
        if (!isDragVisible || dragElement == null)
        {
            return;
        }

        dragElement.style.backgroundImage = new StyleBackground();
        ShowDragElement(false);
        isDragVisible = false;
    }

    private void ShowDragElement(bool show)
    {
        if (dragElement != null)
        {
            dragElement.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    #endregion

    #region Overlay Management

    public void BringOverlaysToFront()
    {
        EnsureSetup();

        if (tooltipContainer != null)
        {
            if (tooltipContainer.parent == null)
                root.Add(tooltipContainer);
            tooltipContainer.BringToFront();
        }

        if (dragElement != null)
        {
            if (dragElement.parent == null)
                root.Add(dragElement);
            dragElement.BringToFront();
        }
    }

    #endregion
}
