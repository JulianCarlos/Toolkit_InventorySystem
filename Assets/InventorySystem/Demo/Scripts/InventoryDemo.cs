using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class InventoryDemo : SlotManagerBase<InventorySlot>
{
    [Header("Inventories (each has its own Inventory component)")]
    [SerializeField] private Inventory inventoryA;
    [SerializeField] private Inventory inventoryB;

    [Header("Random Item Generation")]
    [SerializeField] private SO_Item[] itemPool;
    [SerializeField] private SO_RarityCatalog rarityCatalog;
    [SerializeField] private int itemsToGenerate = 12;

    private VisualElement root;
    private VisualElement containerA;
    private VisualElement containerB;
    private VisualElement statusBar;
    private Label statusLabel;

    private static readonly string[] DefaultRarityNames =
    {
        "Common",
        "Uncommon",
        "Rare",
        "Very Rare",
        "Legendary",
        "Unique"
    };

    protected override string SlotCssClass => "inventory-slot";

    protected override void Awake()
    {
        // no singleton needed
    }

    protected override void Start()
    {
        base.Start();

        if (inventoryA == null || inventoryB == null)
        {
            Debug.LogError("[InventoryDemo] Both inventoryA and inventoryB must be assigned!", this);
            enabled = false;
            return;
        }

        BuildDemoUI();
        PopulateWithRandomItems();
        UpdateStatus();
    }

    #region UI Build

    private void BuildDemoUI()
    {
        root = uiDocument.rootVisualElement;

        // Full-screen root
        var fullscreen = new VisualElement();
        fullscreen.name = "DemoRoot";
        fullscreen.style.flexGrow = 1;
        fullscreen.style.flexDirection = FlexDirection.Column;
        fullscreen.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
        root.Add(fullscreen);

        // Title
        var title = new Label("Inventory System Demo");
        title.style.fontSize = 28;
        title.style.color = Color.white;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        title.style.paddingTop = 20;
        title.style.paddingBottom = 10;
        fullscreen.Add(title);

        // Help text
        var help = new Label("Left-click drag to move items  |  Right-click to transfer  |  Middle-click to split stack");
        help.style.fontSize = 13;
        help.style.color = new Color(0.7f, 0.7f, 0.7f);
        help.style.unityTextAlign = TextAnchor.MiddleCenter;
        help.style.paddingBottom = 15;
        fullscreen.Add(help);

        // Inventory panel (side-by-side)
        var invPanel = new VisualElement();
        invPanel.style.flexDirection = FlexDirection.Row;
        invPanel.style.justifyContent = Justify.Center;
        invPanel.style.alignItems = Align.FlexStart;
        invPanel.style.paddingTop = 20;
        invPanel.style.paddingBottom = 10;
        fullscreen.Add(invPanel);

        // Container A
        containerA = new VisualElement();
        containerA.name = "ContainerA";
        containerA.style.marginRight = 20;
        invPanel.Add(containerA);

        // Container B
        containerB = new VisualElement();
        containerB.name = "ContainerB";
        containerB.style.marginLeft = 20;
        invPanel.Add(containerB);

        // Place inventory UIs
        var uiA = inventoryA.GetInventoryUI();
        containerA.Add(uiA);

        var uiB = inventoryB.GetInventoryUI();
        containerB.Add(uiB);

        // Bottom toolbar
        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.justifyContent = Justify.Center;
        toolbar.style.paddingTop = 15;
        toolbar.style.paddingBottom = 20;
        fullscreen.Add(toolbar);

        AddToolbarButton(toolbar, "Randomize Items", () =>
        {
            ClearBothInventories();
            PopulateWithRandomItems();
            UpdateStatus();
        });

        AddToolbarButton(toolbar, "Sort A", () =>
        {
            inventoryA.CompactAndSort();
            UpdateStatus();
        });

        AddToolbarButton(toolbar, "Sort B", () =>
        {
            inventoryB.CompactAndSort();
            UpdateStatus();
        });

        AddToolbarButton(toolbar, "Transfer All A → B", () =>
        {
            TransferAll(inventoryA, inventoryB);
            UpdateStatus();
        });

        AddToolbarButton(toolbar, "Transfer All B → A", () =>
        {
            TransferAll(inventoryB, inventoryA);
            UpdateStatus();
        });

        AddToolbarButton(toolbar, "Clear All", () =>
        {
            ClearBothInventories();
            UpdateStatus();
        });

        // Status bar
        statusBar = new VisualElement();
        statusBar.style.flexDirection = FlexDirection.Row;
        statusBar.style.justifyContent = Justify.Center;
        statusBar.style.paddingBottom = 15;
        fullscreen.Add(statusBar);

        statusLabel = new Label();
        statusLabel.style.fontSize = 14;
        statusLabel.style.color = new Color(0.8f, 0.8f, 0.6f);
        statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        statusBar.Add(statusLabel);

        // Ensure drag/tooltip overlays render on top of demo content
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.BringOverlaysToFront();
        }
    }

    private void AddToolbarButton(VisualElement parent, string text, Action onClick)
    {
        var btn = new Button(onClick);
        btn.text = text;
        btn.style.height = 32;
        btn.style.paddingLeft = 12;
        btn.style.paddingRight = 12;
        btn.style.marginLeft = 4;
        btn.style.marginRight = 4;
        btn.style.backgroundColor = new Color(0.25f, 0.25f, 0.28f);
        btn.style.color = Color.white;
        btn.style.borderTopLeftRadius = 5;
        btn.style.borderTopRightRadius = 5;
        btn.style.borderBottomLeftRadius = 5;
        btn.style.borderBottomRightRadius = 5;
        btn.style.borderTopWidth = 1;
        btn.style.borderRightWidth = 1;
        btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth = 1;
        btn.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
        btn.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f);
        btn.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);
        btn.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f);
        parent.Add(btn);
    }

    #endregion

    #region Item Generation

    private void PopulateWithRandomItems()
    {
        if (itemPool == null || itemPool.Length == 0)
        {
            PopulateWithCodeItems();
            return;
        }

        PopulateWithSOItems();
    }

    private void PopulateWithSOItems()
    {
        for (int i = 0; i < itemsToGenerate; i++)
        {
            SO_Item so = itemPool[UnityEngine.Random.Range(0, itemPool.Length)];

            if (so == null)
            {
                continue;
            }

            Item item = ItemFactory.CreateItem(so);
            int amount = UnityEngine.Random.Range(1, item.StackSize + 1);

            Inventory target = GetRandomInventory();
            target.TryAddItem(item, amount);
        }
    }

    private void PopulateWithCodeItems()
    {
        var defs = new (string guid, string name, string desc, int stack, ItemType type)[]
        {
            ("iron",   "Iron Ore",       "Raw iron.",           20, ItemType.Resource),
            ("gold",   "Gold Ore",       "Shiny gold.",         20, ItemType.Resource),
            ("wood",   "Wood",           "Sturdy log.",         50, ItemType.Resource),
            ("stone",  "Stone",          "Heavy stone.",        30, ItemType.Resource),
            ("hpot",   "Health Potion",  "Restores HP.",        10, ItemType.Consumable),
            ("mpot",   "Mana Potion",    "Restores MP.",        10, ItemType.Consumable),
            ("ruby",   "Ruby",           "Red gem.",             5, ItemType.Misc),
            ("relic",  "Ancient Relic",  "Mysterious artifact.", 1, ItemType.Quest),
        };

        for (int i = 0; i < itemsToGenerate; i++)
        {
            var d = defs[UnityEngine.Random.Range(0, defs.Length)];
            string rarity = GetRandomRarityName();

            Item item = new Item(d.guid, d.name, d.desc, "", d.stack, rarity, d.type);
            int amount = UnityEngine.Random.Range(1, item.StackSize + 1);

            Inventory target = GetRandomInventory();
            target.TryAddItem(item, amount);
        }
    }

    private string GetRandomRarityName()
    {
        SO_RarityCatalog catalog = rarityCatalog;

        if (catalog == null && inventoryA != null)
        {
            catalog = inventoryA.RarityCatalog;
        }

        string[] rarityNames = DefaultRarityNames;

        if (catalog != null)
        {
            rarityNames = catalog.GetRarityNames();
        }

        if (rarityNames == null || rarityNames.Length == 0)
        {
            return RarityDefinition.DefaultName;
        }

        return rarityNames[UnityEngine.Random.Range(0, rarityNames.Length)];
    }

    #endregion

    #region Interactions (SlotManagerBase overrides)

    protected override bool HasItem(InventorySlot slot)
    {
        return slot != null && !slot.IsEmpty;
    }

    protected override void ShowDragIcon(InventorySlot slot)
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.ShowDrag(slot.Item?.Icon, mousePositionPanel);
            InventoryUIManager.Instance.HideTooltip();
        }
    }

    protected override void HideDragIcon()
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.HideDrag();
        }
    }

    protected override void ShowTooltip(InventorySlot slot)
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.TooltipUpdate(slot, mousePositionPanel, Time.deltaTime);
        }
    }

    protected override void HideTooltip()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsTooltipVisible)
        {
            InventoryUIManager.Instance.HideTooltip();
        }
    }

    protected override void OnExtraButtons(InventorySlot slot)
    {
        if (slot.Item == null)
        {
            return;
        }

        // Right-click: transfer to the other inventory
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Inventory other = GetOtherInventory(slot.Parent);
            if (other != null)
            {
                Inventory.TransferSlot(slot, other);
                UpdateStatus();
            }
        }

        // Middle-click: split stack
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            slot.Parent.SplitSlotItem(slot);
            UpdateStatus();
        }
    }

    protected override void OnDragRelease(InventorySlot from, InventorySlot to, Vector2 panelPosition)
    {
        // Drop outside = destroy
        if (InventoryUIManager.Instance != null && !InventoryUIManager.Instance.IsInAnyInventoryBounds(panelPosition))
        {
            from.UpdateSlot(null, 0);
            UpdateStatus();
            return;
        }

        if (to != null)
        {
            Inventory.MoveOrSwapSlots(from, to);
            UpdateStatus();
        }
    }

    #endregion

    #region Helpers

    private Inventory GetOtherInventory(Inventory origin)
    {
        if (origin == inventoryA)
        {
            return inventoryB;
        }

        if (origin == inventoryB)
        {
            return inventoryA;
        }

        return null;
    }

    private Inventory GetRandomInventory()
    {
        if (UnityEngine.Random.value > 0.5f)
        {
            return inventoryA;
        }

        return inventoryB;
    }

    private void TransferAll(Inventory from, Inventory to)
    {
        from.TransferAllTo(to);
    }

    private void ClearBothInventories()
    {
        inventoryA.Clear();
        inventoryB.Clear();
    }

    private void UpdateStatus()
    {
        if (statusLabel == null)
        {
            return;
        }

        string nameA = inventoryA.InventoryName;
        string nameB = inventoryB.InventoryName;
        int usedA = inventoryA.UsedSlotCount;
        int totalA = inventoryA.SlotCount;
        int itemsA = inventoryA.TotalItemCount;
        int usedB = inventoryB.UsedSlotCount;
        int totalB = inventoryB.SlotCount;
        int itemsB = inventoryB.TotalItemCount;

        statusLabel.text = $"{nameA}: {usedA}/{totalA} slots, {itemsA} items   |   {nameB}: {usedB}/{totalB} slots, {itemsB} items";
    }

    #endregion
}
