# Inventory System Documentation

## Overview

This package provides a UI Toolkit based inventory system for Unity projects. It is designed to be easy to drop into a scene, but the runtime API also works without UI for gameplay systems, tests, server-side logic, or custom front ends.

The package includes:

- Runtime `Inventory` components with stack handling, filters, sorting, transfer, split, swap, and save-data support.
- Runtime `Item` and `Equipment` models created from ScriptableObject item assets.
- UI Toolkit inventory templates, slot templates, tooltip support, drag visuals, and default interaction managers.
- Rarity catalogs with custom rarity names, sort order, colors, and optional slot backgrounds.
- Item filters and equipment slot filters.
- Editor tools for generating inventory UXML layouts and setting up playable demos.
- Demo assets for player inventories, equipment, hotbar, drops, and two-inventory transfer workflows.

## Requirements

- Unity 6 or newer is recommended for this project version.
- UI Toolkit is used for all included UI.
- The new Input System package is used by the included demo interaction managers.
- No `Resources` folder is required for item icons when using `SO_Item` assets. Icons are injected directly by `ItemFactory`.

## Folder Guide

- `Assets/InventorySystem/Runtime/Core`: inventory, slots, runtime item models, rarity definitions.
- `Assets/InventorySystem/Runtime/ScriptableObjects`: item assets, equipment assets, item filters, rarity catalogs.
- `Assets/InventorySystem/Runtime/UI`: UI manager, slot interaction base class, transfer-all button helper.
- `Assets/InventorySystem/Runtime/Factory`: item and filter factory helpers.
- `Assets/InventorySystem/Runtime/Data`: serializable inventory save data models.
- `Assets/InventorySystem/Editor`: inventory layout generator window.
- `Assets/InventorySystem/Demo`: demo scenes, setup tools, item assets, and demo scripts.
- `Assets/InventorySystem/UI`: default UI Toolkit templates.

## Quick Start

1. Create item assets from `Assets > Create > Inventory System > Item Data`.
2. Add an `Inventory` component to a GameObject.
3. Assign a `Layout Template`, such as `Assets/InventorySystem/UI/DefaultInventory.uxml`.
4. Assign a `Slot Template`, such as `Assets/InventorySystem/UI/InventorySlot.uxml`.
5. Set `Slot Count` to the number of slots you want.
6. Add a `UIDocument` to your UI root GameObject.
7. Add `InventoryUIManager` to the same UI root and assign the `UIDocument`.
8. Add the inventory UI to your document at runtime with `inventory.GetInventoryUI()` or use one of the demo setup tools.
9. Create runtime items with `ItemFactory.CreateItem(itemAsset)` and add them with `inventory.AddItem(item, amount)`.

Minimal runtime example:

```csharp
using UnityEngine;

public class LootExample : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private SO_Item itemAsset;

    public void AddLoot(int amount)
    {
        Item item = ItemFactory.CreateItem(itemAsset);
        InventoryOperationResult result = inventory.AddItem(item, amount);

        if (!result.FullyProcessed)
        {
            Debug.Log($"Inventory full. Leftover amount: {result.RemainingAmount}");
        }
    }
}
```

## Core Concepts

### Inventory

`Inventory` is a `MonoBehaviour` and the main component most projects interact with. It owns a fixed-size array of `InventorySlot` objects and can optionally build a UI Toolkit visual tree from a layout template.

An inventory can run in two modes:

- Visual mode: assign `Layout Template` and `Slot Template` for UI Toolkit output.
- Headless mode: leave `Layout Template` empty and set `Slot Count`. This is useful for tests, gameplay-only containers, save systems, or custom UI implementations.

### InventorySlot

`InventorySlot` stores one item stack. It tracks the runtime `Item`, the stack amount, its parent `Inventory`, optional slot filters, and optional UI Toolkit references.

Slots can:

- Accept or reject items through a predicate filter.
- Stack matching items when the item guid matches and stack size allows it.
- Update slot visuals when attached to a UI template.
- Notify the parent inventory when their contents change.

### Item and Equipment

`Item` is the runtime data object stored in slots. It contains guid, name, description, icon path, stack size, rarity, item type, icon texture, and rarity background texture.

`Equipment` extends `Item` with an `EquipmentSlot` value. Equipment assets are created with `SO_Equipment` and generate runtime `Equipment` instances through `ItemFactory`.

### ScriptableObject Item Assets

`SO_Item` and `SO_Equipment` are authoring assets. They are not stored directly in inventory slots; use `ItemFactory` to convert them into runtime `Item` or `Equipment` instances.

## Inventory Inspector Fields

- `Unique Id`: stable id used by save data. Generated automatically when empty.
- `Inventory Name`: display name used by templates that contain a label named `InventoryTitle`.
- `Layout Template`: optional UI Toolkit layout for the full inventory panel.
- `Slot Template`: optional UI Toolkit slot template cloned into the layout.
- `Slot Count`: number of slots to create when using a slot template or headless inventory.
- `Filters`: item filters that must all accept an item before it can be added.
- `Rarity Catalog`: optional rarity catalog used for sorting.
- `Primary Sort`: first sort comparison used by `CompactAndSort()`.
- `Secondary Sort`: second sort comparison used when the primary comparison is equal.
- `Sort Rarity Descending`: when enabled, higher rarity sort values appear first.

## Runtime API

`Inventory` implements two small interfaces so gameplay code can depend on only what it needs.

### IReadOnlyInventory

Use `IReadOnlyInventory` when a system should inspect an inventory but not mutate it.

Available members:

- `UniqueId`
- `InventoryName`
- `SlotCount`
- `UsedSlotCount`
- `TotalItemCount`
- `Slots`
- `IsInitialized`
- `CanAddItem(Item item, int amount = 1)`
- `GetItemCount(string itemGuid)`
- `HasItem(string itemGuid, int amount = 1)`
- `GetInventoryData()`

### IInventory

Use `IInventory` when a system needs to modify inventory contents.

Available members:

- `Changed`
- `SlotChanged`
- `AddItem(Item item, int amount)`
- `RemoveItems(string itemGuid, int amount)`
- `TransferSlotTo(InventorySlot sourceSlot, Inventory targetInventory)`
- `TransferAllTo(Inventory targetInventory)`
- `TryMoveOrSwap(InventorySlot sourceSlot, InventorySlot destinationSlot)`
- `TrySplitSlot(InventorySlot slot, out InventorySlot createdSlot)`
- `Clear()`

### InventoryOperationResult

Add, remove, and transfer operations return `InventoryOperationResult`.

- `RequestedAmount`: amount requested by the caller.
- `ProcessedAmount`: amount that was added, removed, or transferred.
- `RemainingAmount`: amount that could not be processed.
- `Succeeded`: true when at least one item was processed.
- `FullyProcessed`: true when no amount remains.

Example:

```csharp
InventoryOperationResult result = inventory.AddItem(item, 10);

if (result.Succeeded)
{
    Debug.Log($"Added {result.ProcessedAmount} items.");
}

if (!result.FullyProcessed)
{
    Debug.Log($"Could not add {result.RemainingAmount} items.");
}
```

### Compatibility Methods

Older integrations can still use:

- `TryAddItem(Item item, int amount)`: returns the amount that did not fit.
- `RemoveItem(string itemGuid, int amount)`: returns the amount that could not be removed.
- `TransferItem(InventorySlot sourceSlot, Inventory targetInventory)`: compatibility wrapper around `TransferSlot`.
- `ItemSlots`: direct array access for existing projects.

New code should prefer result-based methods and safer slot accessors such as `Slots`, `GetSlot(index)`, and `TryGetSlot(index, out slot)`.

## Common API Examples

### Add Items

```csharp
Item item = ItemFactory.CreateItem(itemAsset);
InventoryOperationResult result = inventory.AddItem(item, 5);
```

### Check Capacity Before Adding

```csharp
if (inventory.CanAddItem(item, amount))
{
    inventory.AddItem(item, amount);
}
```

### Remove Items

```csharp
InventoryOperationResult result = inventory.RemoveItems(item.Guid, 3);

if (!result.FullyProcessed)
{
    Debug.Log($"Missing {result.RemainingAmount} items.");
}
```

### Count and Query Items

```csharp
int potionCount = inventory.GetItemCount(healthPotionGuid);
bool hasEnough = inventory.HasItem(healthPotionGuid, 3);
int usedSlots = inventory.UsedSlotCount;
int totalItems = inventory.TotalItemCount;
```

### Get a Slot Safely

```csharp
if (inventory.TryGetSlot(0, out InventorySlot firstSlot))
{
    Debug.Log(firstSlot.Amount);
}
```

### Split a Stack

```csharp
if (inventory.TrySplitSlot(sourceSlot, out InventorySlot newStackSlot))
{
    Debug.Log($"Created stack with {newStackSlot.Amount} items.");
}
```

### Move, Stack, or Swap Slots

```csharp
InventoryOperationResult result = Inventory.MoveOrSwapSlots(sourceSlot, destinationSlot);
```

The operation first tries to stack matching items. If stacking is not possible, it tries to swap items while respecting both slot filters.

### Transfer One Slot

```csharp
InventoryOperationResult result = Inventory.TransferSlot(sourceSlot, targetInventory);
```

### Transfer All Items

```csharp
InventoryOperationResult result = sourceInventory.TransferAllTo(targetInventory);
```

### Clear an Inventory

```csharp
inventory.Clear();
```

### Listen for Changes

```csharp
private void OnEnable()
{
    inventory.Changed += HandleInventoryChanged;
    inventory.SlotChanged += HandleSlotChanged;
}

private void OnDisable()
{
    inventory.Changed -= HandleInventoryChanged;
    inventory.SlotChanged -= HandleSlotChanged;
}

private void HandleInventoryChanged(Inventory changedInventory)
{
    Debug.Log($"Inventory changed: {changedInventory.InventoryName}");
}

private void HandleSlotChanged(InventorySlot slot)
{
    Debug.Log($"Slot changed: {slot.Amount}");
}
```

## Item Assets

Create items from `Assets > Create > Inventory System > Item Data`.

Important fields:

- `Guid`: stable item id. Generated automatically when empty.
- `Item Name`: display name used by tooltips and UI.
- `Description`: tooltip description.
- `Icon`: texture shown in slots and drag visuals.
- `Stack Size`: maximum amount per stack. Clamped to at least `1`.
- `Item Type`: one of `Generic`, `Resource`, `Equipment`, `Consumable`, `Quest`, or `Misc`.
- `Rarity Catalog`: optional catalog used by the item inspector and background lookup.
- `Rarity`: string rarity name stored on created runtime items.

When a rarity catalog is assigned, the custom item inspector displays catalog rarity names in a dropdown and keeps a `Custom...` option for one-off rarity names.

Create runtime items with:

```csharp
Item item = ItemFactory.CreateItem(itemAsset);
```

Create an item with a custom runtime rarity:

```csharp
Item eventItem = ItemFactory.CreateItem(itemAsset, "Mythic");
```

Create an item entirely from code:

```csharp
Item item = ItemFactory.CreateItem(
    "wood",
    "Wood",
    "A sturdy log.",
    string.Empty,
    50,
    "Common",
    ItemType.Resource);
```

## Equipment Assets

Create equipment from `Assets > Create > Inventory System > Equipment Data`.

Equipment assets use the same fields as item assets and add an equipment slot type:

- `Helmet`
- `Chestpiece`
- `Gloves`
- `Leggings`
- `Boots`
- `Belt`
- `Ring1`
- `Ring2`
- `Amulet`

At runtime, `SO_Equipment` creates an `Equipment` item with `ItemType.Equipment` and an `EquipmentSlot` value. Editor and generator code can configure equipment assets with `SetEquipmentType(type)`.

## Rarity Catalogs

Use `Assets/InventorySystem/Runtime/ScriptableObjects/DefaultRarityCatalog.asset` or create another catalog from `Assets > Create > Inventory System > Rarity Catalog`.

New catalogs start with:

- Common, sort order `0`
- Uncommon, sort order `10`
- Rare, sort order `20`
- Very Rare, sort order `30`
- Legendary, sort order `40`
- Unique, sort order `50`

Each rarity entry has:

- Name
- Sort order
- Inspector accent color
- Optional background texture

Items store rarity as a string, so projects can add any number of rarity names. Higher sort order means higher rarity when `Sort Rarity Descending` is enabled. Unknown rarity names sort after catalog entries in descending mode because their fallback sort order is `int.MinValue`.

Slot rarity backgrounds are resolved in this order:

1. The assigned rarity catalog background for the item rarity.
2. A fallback `Resources/Images/Rarity/{rarity-name}` texture, where spaces and punctuation are removed and the name is lowercased.
3. No rarity background.

## Filters

Create item filters from `Assets > Create > Inventory System > Item Filter`.

An item filter with no allowed types accepts every non-null item. A filter with one or more allowed types only accepts matching `ItemType` values.

`SO_ItemFilter` implements `IItemFilter`, so gameplay code can depend on the small `Allows(Item item)` contract.

Inventory-level filters are configured in the `Filters` array on `Inventory`. All assigned filters must pass before an item can be added.

Slot-level filters are configured in code through the `InventorySlot` constructor. The included equipment demo uses slot filters so each equipment slot only accepts a matching `EquipmentType`.

Example code filter:

```csharp
InventorySlot helmetSlot = new InventorySlot(
    inventory,
    ItemFilterFactory.ByEquipmentType(EquipmentType.Helmet));
```

Available filter helpers:

- `ItemFilterFactory.AllowAny()`
- `ItemFilterFactory.ByItemType(ItemType type)`
- `ItemFilterFactory.ByEquipmentType(EquipmentType type)`
- `ItemFilterFactory.ByRarity(string minRarity, SO_RarityCatalog rarityCatalog = null)`
- `ItemFilterFactory.Combine(params Predicate<Item>[] predicates)`

## Sorting

Call `CompactAndSort()` to merge matching stacks, clear the inventory, and write sorted stacks back into slots.

Supported sort keys:

- `Name`
- `Rarity`
- `Type`
- `Guid`

Configure sorting in the Inspector or through code:

```csharp
inventory.SetRarityCatalog(rarityCatalog);
inventory.SetSortOptions(
    InventorySortKey.Rarity,
    InventorySortKey.Name,
    rarityDescending: true);
inventory.CompactAndSort();
```

## UI Toolkit Setup

### Inventory Layout Template

An inventory layout should contain these optional or required named elements:

- `InventorySlotContainer`: required for visual inventories. Slots are read from or cloned into this element.
- `InventoryTitle`: optional label. If present, it is set to `InventoryName`.
- `SortButton`: optional button. If present, it calls `CompactAndSort()`.

### Slot Template

The default slot template uses these element names:

- `RarityContainer`: optional background layer for rarity textures.
- `ItemSpriteImage`: item icon background image.
- `StackContainer`: shown when stack amount is greater than `1`.
- `StackLabel`: displays the stack amount.

### InventoryUIManager

Add `InventoryUIManager` to the GameObject that owns your main `UIDocument`. Assign the same `UIDocument` to the manager.

`InventoryUIManager` handles:

- Tooltip creation and positioning.
- Drag icon creation and movement.
- Registered inventory UI bounds for drop detection.
- Overlay ordering with `BringOverlaysToFront()`.

The manager looks for these optional UI elements in the root document:

- `TooltipContainer`
- `TooltipImage`
- `TooltipBackground`
- `TooltipTitle`
- `TooltipDescription`
- `DragElement`

If tooltip or drag elements are missing, default elements are created automatically.

### Getting Inventory UI

Use `GetInventoryUI()` to clone or return the inventory visual tree:

```csharp
VisualElement inventoryElement = inventory.GetInventoryUI();
rootVisualElement.Add(inventoryElement);
```

If `Layout Template` is empty, `GetInventoryUI()` returns `null`.

### Runtime Slot Interaction

`SlotManagerBase<TSlot>` is the base class for pointer-driven slot interactions. It handles mouse position, picking, drag start/release, extra mouse buttons, and tooltip dispatch. Create a subclass when you want custom interaction rules.

The included demos use this pattern for:

- Left-click drag to move, stack, or swap items.
- Right-click transfer to another inventory.
- Middle-click split stack.
- Drop outside inventory bounds to destroy the dragged stack in the demo.

## Transfer-All Button Helper

`InventoryTransferAllButton` is a small component for UI Toolkit buttons.

Setup:

1. Add `InventoryTransferAllButton` to a GameObject.
2. Assign the `UIDocument`.
3. Set `Button Name` to the name of a `Button` in the document. The default is `TransferAllButton`.
4. Assign `Source Inventory` and `Target Inventory`.

When clicked, it calls `sourceInventory.TransferAllTo(targetInventory)`.

You can also configure it from code:

```csharp
transferButton.Configure(sourceInventory, targetInventory);
transferButton.Bind();
```

## Saving and Loading

Use `GetInventoryData()` to create serializable inventory data and `SetInventoryData(data)` to restore it.

`InventoryData` contains:

- `InventoryGuid`
- `InventoryName`
- `Position`
- `Rotation`
- `Scale`
- `PrefabName`
- `InventorySlots`

`InventorySlotData` contains:

- `InventoryItem`
- `Amount`

Example:

```csharp
InventoryData data = inventory.GetInventoryData();
string json = JsonUtility.ToJson(data, prettyPrint: true);

InventoryData loadedData = JsonUtility.FromJson<InventoryData>(json);
inventory.SetInventoryData(loadedData);
```

Save systems should keep item guids stable. If you replace item assets or regenerate guids, old save data may no longer match your item database.

## World Items

`WorldItem` is a simple pickup helper component. It stores:

- `ItemData`: the `SO_Item` asset to create at pickup time.
- `Amount`: amount to add.

`UpdateItemAmount(amount)` updates the remaining amount and destroys the GameObject when the amount reaches zero.

Example pickup:

```csharp
public void Pickup(WorldItem worldItem)
{
    Item item = ItemFactory.CreateItem(worldItem.ItemData);
    InventoryOperationResult result = inventory.AddItem(item, worldItem.Amount);
    worldItem.UpdateItemAmount(result.RemainingAmount);
}
```

## Editor Tools

### Inventory Generator

Open `Tools > Inventory System > Inventory Generator`.

The generator creates a UI Toolkit inventory layout from rows, columns, sizing, padding, colors, title settings, and sort-button settings.

Options:

- `Inventory Name`: title text and generated scene object name.
- `Asset Name`: generated UXML file name.
- `Output Folder`: generated UXML folder. Defaults to `Assets/InventorySystem/UI/Generated`.
- `Show Title`: creates an `InventoryTitle` label.
- `Enable Sort Button`: creates a `SortButton`.
- `Horizontal Slots` and `Vertical Slots`: grid dimensions.
- `Slot Size`, `Slot Gap`, `Padding`, `Header Height`: layout sizing.
- `Background`, `Border`, `Preview Slots`: layout colors.
- `Create Scene Object`: creates a GameObject with an `Inventory` component and assigns the generated layout.

Generated layouts include the required `InventorySlotContainer` and slot child elements.

### Demo Item Generator

Use `Tools > Inventory System > Generate Demo Items` to create demo item assets in `Assets/InventorySystem/Demo/ItemData`. It uses the default rarity catalog when available.

### Demo Scene Setup

Use `Tools > Inventory System > Setup Interactive Demo` to create a two-inventory demo with 24-slot Backpack and Chest inventories.

Use `Tools > Inventory System > Setup Full HUD Demo` to create a HUD-style setup with:

- Player inventory, 36 slots
- Equipment inventory, 9 filtered equipment slots
- Hotbar, 6 slots
- Inventory toggle with the `I` key

## Demo Controls

Interactive demo controls:

- Left-click and drag: move, stack, or swap items.
- Right-click a slot: transfer to the other inventory.
- Middle-click a slot: split the stack.

Full HUD demo controls:

- `I`: open or close the inventory.
- Left-click and drag: move, stack, or swap items.
- Right-click: transfer between player, target, hotbar, and equipment inventories.
- Middle-click: split a stack.

## Extension Points

### Custom Inventory Subclasses

Subclass `Inventory` when you need specialized slots, custom buttons, or custom behavior. Override `InitializeInventory()` to create filtered slots, and override `QueryUIReferences()` or `LinkCallbacks()` to bind custom UI controls.

Example:

```csharp
public class WeaponInventory : Inventory
{
    protected override void InitializeInventory()
    {
        slots = new InventorySlot[containerSize];

        for (int i = 0; i < containerSize; i++)
        {
            slots[i] = new InventorySlot(this, ItemFilterFactory.ByItemType(ItemType.Equipment));
        }

        isInitialized = true;
    }
}
```

### Custom Slot Interaction

Subclass `SlotManagerBase<InventorySlot>` if you want project-specific drag, drop, transfer, or tooltip behavior. Use the static runtime operations on `Inventory` to keep the same stack and filter rules:

```csharp
Inventory.MoveOrSwapSlots(sourceSlot, destinationSlot);
Inventory.TransferSlot(sourceSlot, targetInventory);
```

### Custom Item Filters

For code-only filters, pass a `Predicate<Item>` to `InventorySlot`.

For designer-authored filters, create new ScriptableObject filters that implement `IItemFilter` or extend the existing `SO_ItemFilter` pattern.

## Best Practices

- Prefer `AddItem`, `RemoveItems`, `TransferSlot`, and `TransferAllTo` over older leftover-integer methods.
- Depend on `IReadOnlyInventory` when a system only needs to inspect an inventory.
- Depend on `IInventory` when a system needs mutation methods.
- Use `TryGetSlot` instead of direct indexing when slot indexes can come from user input or save data.
- Use `Changed` or `SlotChanged` to update custom UI, quest counters, or save dirty flags.
- Keep item guids stable once a game has save data.
- Put business rules in filters or inventory subclasses, not in UI click handlers.
- Use the shared runtime slot operations from `Inventory` so demos, custom UI, and gameplay all follow the same rules.
- Use headless inventories for automated tests and non-visual containers.

## Troubleshooting

### Inventory UI does not appear

- Make sure `Layout Template` is assigned.
- Make sure the layout contains a `VisualElement` named `InventorySlotContainer`.
- Make sure the returned `inventory.GetInventoryUI()` element is added to a `UIDocument` root.
- Make sure an `InventoryUIManager` exists if you need drag visuals, tooltips, or bounds checks.

### Slots do not show icons

- Make sure the slot template contains `ItemSpriteImage`.
- Make sure the `SO_Item` asset has an icon assigned.
- Make sure items are created through `ItemFactory.CreateItem(itemAsset)` so textures are injected.

### Stack amounts do not show

- Stack labels only show when amount is greater than `1`.
- Make sure the slot template contains `StackContainer` and `StackLabel`.

### Sort button does nothing

- Make sure the inventory layout has a `Button` named `SortButton`.
- Make sure the inventory UI has been built by calling `GetInventoryUI()`.

### Items cannot be added

- Check inventory-level `Filters`.
- Check slot-level filters on custom inventory subclasses.
- Check `Stack Size` on the item asset.
- Use `CanAddItem(item, amount)` before adding to test capacity and filters.

### Equipment cannot be placed in an equipment slot

- Make sure the item asset is `SO_Equipment`.
- Make sure its equipment type matches the target slot filter.
- Make sure it was created through `ItemFactory.CreateItem(equipmentAsset)`.

### Drag and tooltip do not work

- Make sure the project has the Input System package installed and active.
- Make sure your interaction manager has a `UIDocument` assigned.
- Make sure an `InventoryUIManager` exists and points at the same `UIDocument`.
- Make sure slot visuals have the `inventory-slot` class. `Inventory.BuildUI()` adds this automatically for generated inventory slots.

## Verification

For this Unity project, compile-check the package from the repository root with:

```powershell
dotnet build .\Toolkit_InventorySystem.sln
```

Unity may regenerate `.csproj` files. If that happens, reopen the project or regenerate project files from Unity before running the build again.