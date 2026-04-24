using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class InventorySystemSceneSetup
{
    private const string SlotUxmlPath = "Assets/InventorySystem/UI/InventorySlot.uxml";
    private const string DefaultInvUxmlPath = "Assets/InventorySystem/UI/DefaultInventory.uxml";
    private const string HudUxmlPath = "Assets/InventorySystem/Demo/UI/DemoHUD.uxml";
    private const string InteractiveUxmlPath = "Assets/InventorySystem/Demo/UI/DemoInteractive.uxml";
    private const string PlayerInvUxmlPath = "Assets/InventorySystem/Demo/UI/DemoPlayerInventory.uxml";
    private const string EquipmentInvUxmlPath = "Assets/InventorySystem/Demo/UI/DemoEquipmentInventory.uxml";
    private const string HotbarInvUxmlPath = "Assets/InventorySystem/Demo/UI/DemoHotbarInventory.uxml";
    private const string ItemDataFolder = "Assets/InventorySystem/Demo/ItemData";

    // ================================================================
    //  Interactive Demo (two inventories side by side)
    // ================================================================

    [MenuItem("Tools/Inventory System/Setup Interactive Demo")]
    public static void SetupInteractiveDemo()
    {
        var slotUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SlotUxmlPath);
        var defaultInvUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(DefaultInvUxmlPath);
        var interactiveUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(InteractiveUxmlPath);

        if (slotUxml == null || defaultInvUxml == null || interactiveUxml == null)
        {
            EditorUtility.DisplayDialog("Missing Assets",
                $"Cannot find required UXML:\n  - {SlotUxmlPath}\n  - {DefaultInvUxmlPath}\n  - {InteractiveUxmlPath}",
                "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Setup Interactive Demo",
            "Creates a ready-to-play interactive demo:\n\n" +
            "  [InventoryDemo]\n" +
            "    - InventoryA (24 slots)\n" +
            "    - InventoryB (24 slots)\n\n" +
            "Press Play to interact.\n\nContinue?", "Setup", "Cancel"))
        {
            return;
        }

        // Root
        var root = new GameObject("[InventoryDemo]");

        // UIDocument
        var uiDoc = root.AddComponent<UIDocument>();
        uiDoc.visualTreeAsset = interactiveUxml;
        uiDoc.sortingOrder = 0;
        uiDoc.panelSettings = FindOrCreatePanelSettings();

        // InventoryUIManager
        var uiManager = root.AddComponent<InventoryUIManager>();
        Set(uiManager, "uiDocument", uiDoc);

        // Inventory A
        var invAGO = new GameObject("InventoryA");
        invAGO.transform.SetParent(root.transform);
        var invA = invAGO.AddComponent<Inventory>();
        Set(invA, "layoutTemplate", defaultInvUxml);
        Set(invA, "slotTemplate", slotUxml);
        Set(invA, "slotCount", 24);
        Set(invA, "inventoryName", "Backpack");

        // Inventory B
        var invBGO = new GameObject("InventoryB");
        invBGO.transform.SetParent(root.transform);
        var invB = invBGO.AddComponent<Inventory>();
        Set(invB, "layoutTemplate", defaultInvUxml);
        Set(invB, "slotTemplate", slotUxml);
        Set(invB, "slotCount", 24);
        Set(invB, "inventoryName", "Chest");

        // InventoryDemo
        var demo = root.AddComponent<InventoryDemo>();
        Set(demo, "uiDocument", uiDoc);
        Set(demo, "inventoryA", invA);
        Set(demo, "inventoryB", invB);
        Set(demo, "itemsToGenerate", 16);
        WireDemoItemPool(demo);

        Undo.RegisterCreatedObjectUndo(root, "Setup Interactive Demo");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log(
            "[Inventory System] Interactive demo ready!\n" +
            "  Press Play to start\n" +
            "  Left-drag to move items | Right-click to transfer | Middle-click to split"
        );

        EditorApplication.delayCall += () => Selection.activeGameObject = root;
    }

    // ================================================================
    //  Full HUD Demo (player + equipment + hotbar)
    // ================================================================

    [MenuItem("Tools/Inventory System/Setup Full HUD Demo")]
    public static void SetupFullHUDDemo()
    {
        var slotUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SlotUxmlPath);
        var hudUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudUxmlPath);
        var playerInvUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PlayerInvUxmlPath);
        var equipInvUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(EquipmentInvUxmlPath);
        var hotbarInvUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HotbarInvUxmlPath);

        if (slotUxml == null || hudUxml == null || playerInvUxml == null || equipInvUxml == null || hotbarInvUxml == null)
        {
            EditorUtility.DisplayDialog("Missing Assets",
                "Cannot find one or more required UXML files.\nCheck the InventorySystem folder.",
                "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Setup Full HUD Demo",
            "Creates a game-like HUD:\n\n" +
            "  [InventorySystem]\n" +
            "    - HUD\n" +
            "    - PlayerInventory (36 slots)\n" +
            "    - EquipmentInventory (9 slots)\n" +
            "    - Hotbar (6 slots)\n\n" +
            "Press 'I' to open/close.\n\nContinue?", "Setup", "Cancel"))
        {
            return;
        }

        // Root
        var root = new GameObject("[InventorySystem]");

        // HUD
        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(root.transform);

        var uiDoc = hudGO.AddComponent<UIDocument>();
        uiDoc.visualTreeAsset = hudUxml;
        uiDoc.sortingOrder = 0;
        uiDoc.panelSettings = FindOrCreatePanelSettings();

        var uiManager = hudGO.AddComponent<InventoryUIManager>();
        Set(uiManager, "uiDocument", uiDoc);

        var demoUIManager = hudGO.AddComponent<DemoUIManager>();
        Set(demoUIManager, "playerHUD", uiDoc);

        hudGO.AddComponent<DemoGameStateManager>();

        // Player Inventory
        var playerGO = new GameObject("PlayerInventory");
        playerGO.transform.SetParent(root.transform);
        var playerInv = playerGO.AddComponent<DemoPlayerInventory>();
        Set(playerInv, "layoutTemplate", playerInvUxml);
        Set(playerInv, "slotTemplate", slotUxml);
        Set(playerInv, "slotCount", 36);
        Set(playerInv, "inventoryName", "Player Inventory");

        // Equipment
        var equipGO = new GameObject("EquipmentInventory");
        equipGO.transform.SetParent(root.transform);
        var equipInv = equipGO.AddComponent<DemoEquipmentInventory>();
        Set(equipInv, "layoutTemplate", equipInvUxml);
        Set(equipInv, "slotTemplate", slotUxml);
        Set(equipInv, "slotCount", 9);
        Set(equipInv, "inventoryName", "Equipment");

        // Hotbar
        var hotbarGO = new GameObject("HotbarInventory");
        hotbarGO.transform.SetParent(root.transform);
        var hotbarInv = hotbarGO.AddComponent<Inventory>();
        Set(hotbarInv, "layoutTemplate", hotbarInvUxml);
        Set(hotbarInv, "slotTemplate", slotUxml);
        Set(hotbarInv, "slotCount", 6);
        Set(hotbarInv, "inventoryName", "Hotbar");

        // DemoInventoryManager
        var invManager = hudGO.AddComponent<DemoInventoryManager>();
        Set(invManager, "uiDocument", uiDoc);
        Set(invManager, "playerInventory", playerInv);
        Set(invManager, "equipmentInventory", equipInv);
        Set(invManager, "hotbarInventory", hotbarInv);
        WireDemoItemArray(invManager, "demoItems");

        Undo.RegisterCreatedObjectUndo(root, "Setup Full HUD Demo");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[Inventory System] Full HUD demo ready! Press Play, then 'I' to open inventory.");

        EditorApplication.delayCall += () => Selection.activeGameObject = root;
    }

    // ================================================================
    //  Helpers
    // ================================================================

    private static PanelSettings FindOrCreatePanelSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps != null) return ps;
        }

        const string panelPath = "Assets/InventorySystem/Demo/DemoPanelSettings.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/InventorySystem/Demo"))
        {
            AssetDatabase.CreateFolder("Assets/InventorySystem", "Demo");
        }

        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        AssetDatabase.CreateAsset(panelSettings, panelPath);
        AssetDatabase.SaveAssets();
        return panelSettings;
    }

    private static void WireDemoItemPool(InventoryDemo demo)
    {
        if (!AssetDatabase.IsValidFolder(ItemDataFolder)) return;

        string[] guids = AssetDatabase.FindAssets("t:SO_Item", new[] { ItemDataFolder });
        if (guids.Length == 0) return;

        var so = new SerializedObject(demo);
        var prop = so.FindProperty("itemPool");
        if (prop != null && prop.isArray)
        {
            prop.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SO_Item>(path);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void WireDemoItemArray(Object target, string fieldName)
    {
        if (!AssetDatabase.IsValidFolder(ItemDataFolder)) return;

        string[] guids = AssetDatabase.FindAssets("t:SO_Item", new[] { ItemDataFolder });
        if (guids.Length == 0) return;

        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null && prop.isArray)
        {
            prop.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SO_Item>(path);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void Set(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void Set(Object target, string fieldName, int value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void Set(Object target, string fieldName, string value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
