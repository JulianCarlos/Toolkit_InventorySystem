using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class InventorySlot : Slot<Item>
{
    #region Runtime Fields

    private int amount;
    private Inventory inventory;

    private Label stackLabel;
    private VisualElement spriteImage;
    private VisualElement stackContainer;
    private VisualElement inventorySlotUI;
    private VisualElement rarityContainer;

    private Texture2D placeholder;
    private bool isInitialized;

    #endregion

    #region Properties

    public override bool IsEmpty => item == null || amount <= 0;
    public int Amount => amount;
    public Inventory Parent => inventory;

    #endregion

    #region Constructors

    public InventorySlot(Inventory origin, Predicate<Item> filterPredicate = null)
        : base(filterPredicate ?? ItemFilterFactory.AllowAny())
    {
        inventory = origin;
    }

    #endregion

    #region UI Setup

    public void InitializeUI(VisualElement slotTemplate)
    {
        isInitialized = true;
        inventorySlotUI = slotTemplate;
        rarityContainer = inventorySlotUI.Q<VisualElement>("RarityContainer");
        spriteImage = inventorySlotUI.Q<VisualElement>("ItemSpriteImage");
        stackContainer = inventorySlotUI.Q<VisualElement>("StackContainer");
        stackLabel = inventorySlotUI.Q<Label>("StackLabel");
        UpdateUI();
    }

    public void SetPlaceholder(Texture2D texture)
    {
        placeholder = texture;
        UpdateUI();
    }

    #endregion

    #region Item State

    public bool CanStack(Item candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        return !IsEmpty && item.Guid == candidate.Guid && amount < item.StackSize;
    }

    public void UpdateSlot(Item newItem, int newAmount)
    {
        int clampedAmount = 0;

        if (newItem != null)
        {
            clampedAmount = Mathf.Max(0, newAmount);
        }

        bool changed = !Equals(item, newItem) || amount != clampedAmount;

        amount = clampedAmount;

        if (amount > 0)
        {
            item = newItem;
        }
        else
        {
            item = default;
        }

        UpdateUI();

        if (changed)
        {
            inventory?.NotifySlotChanged(this);
        }
    }

    public void UpdateSlot(int newAmount)
    {
        UpdateSlot(item, newAmount);
    }

    #endregion

    #region UI Updates

    public void UpdateUI()
    {
        if (!isInitialized)
        {
            return;
        }

        if (IsEmpty)
        {
            ApplyEmptyVisuals();
        }
        else
        {
            ApplyItemVisuals();
        }

        bool showStack = amount > 1;
        UpdateStackVisuals(showStack);
    }

    public void ShowSlot()
    {
        if (inventorySlotUI != null)
        {
            inventorySlotUI.style.display = DisplayStyle.Flex;
        }
    }

    public void HideSlot()
    {
        if (inventorySlotUI != null)
        {
            inventorySlotUI.style.display = DisplayStyle.None;
        }
    }

    private void ApplyEmptyVisuals()
    {
        if (placeholder != null)
        {
            spriteImage.style.backgroundImage = new StyleBackground(placeholder);
        }
        else
        {
            spriteImage.style.backgroundImage = new StyleBackground();
        }

        if (rarityContainer != null)
        {
            rarityContainer.style.backgroundImage = new StyleBackground();
        }
    }

    private void ApplyItemVisuals()
    {
        if (item.Icon != null)
        {
            spriteImage.style.backgroundImage = new StyleBackground(item.Icon);
        }
        else
        {
            spriteImage.style.backgroundImage = new StyleBackground();
        }

        if (rarityContainer == null)
        {
            return;
        }

        if (item.RarityBackground != null)
        {
            rarityContainer.style.backgroundImage = new StyleBackground(item.RarityBackground);
        }
        else
        {
            rarityContainer.style.backgroundImage = new StyleBackground();
        }
    }

    private void UpdateStackVisuals(bool showStack)
    {
        if (showStack)
        {
            stackLabel.text = amount.ToString();
            stackContainer.style.display = DisplayStyle.Flex;
            return;
        }

        stackLabel.text = string.Empty;
        stackContainer.style.display = DisplayStyle.None;
    }

    #endregion
}
