using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class InventorySlot : Slot<Item>
{
    public override bool IsEmpty => item == null || amount <= 0;
    public int Amount => amount;
    public Inventory Parent => inventory;

    private int amount;
    private Inventory inventory;

    private Label stackLabel;
    private VisualElement spriteImage;
    private VisualElement stackContainer;
    private VisualElement inventorySlotUI;
    private VisualElement rarityContainer;

    private Texture2D placeholder;
    private bool isInitialized;

    public InventorySlot(Inventory origin, Predicate<Item> filterPredicate = null)
        : base(filterPredicate ?? ItemFilterFactory.AllowAny())
    {
        inventory = origin;
    }

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
        amount = newAmount;
        item = amount > 0 ? newItem : default;
        UpdateUI();
    }

    public void UpdateSlot(int newAmount)
    {
        UpdateSlot(item, newAmount);
    }

    public void UpdateUI()
    {
        if (!isInitialized)
        {
            return;
        }

        if (IsEmpty)
        {
            spriteImage.style.backgroundImage = placeholder != null
                ? new StyleBackground(placeholder)
                : new StyleBackground();
            if (rarityContainer != null)
            {
                rarityContainer.style.backgroundImage = new StyleBackground();
            }
        }
        else
        {
            spriteImage.style.backgroundImage = item.Icon != null
                ? new StyleBackground(item.Icon)
                : new StyleBackground();
            if (rarityContainer != null)
            {
                rarityContainer.style.backgroundImage = item.RarityBackground != null
                    ? new StyleBackground(item.RarityBackground)
                    : new StyleBackground();
            }
        }

        bool showStack = amount > 1;
        stackLabel.text = showStack ? amount.ToString() : string.Empty;
        stackContainer.style.display = showStack ? DisplayStyle.Flex : DisplayStyle.None;
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
}
