using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InventorySystemInventoryGeneratorWindow : EditorWindow
{
    private const string DefaultOutputFolder = "Assets/InventorySystem/UI/Generated";

    private string inventoryName = "Inventory";
    private string assetName = "GeneratedInventory";
    private string outputFolder = DefaultOutputFolder;
    private int horizontalSlots = 5;
    private int verticalSlots = 4;
    private int slotSize = 55;
    private int slotGap = 5;
    private int padding = 10;
    private int titleHeight = 50;
    private bool showTitle = true;
    private bool showSortButton = true;
    private bool createSceneObject = true;
    private Color backgroundColor = new Color32(41, 41, 41, 255);
    private Color borderColor = Color.white;
    private Color slotPreviewColor = new Color32(36, 36, 36, 255);
    private Vector2 settingsScroll;

    private GUIStyle paneStyle;
    private GUIStyle sectionStyle;
    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;

    private int SlotCount => horizontalSlots * verticalSlots;
    private int SlotOuterSize => slotSize + (slotGap * 2);
    private int GridWidth => (horizontalSlots * SlotOuterSize) + (padding * 2);
    private int GridHeight => (verticalSlots * SlotOuterSize) + (padding * 2);
    private int LayoutWidth => GridWidth;
    private bool ShowHeader => showTitle || showSortButton;
    private int LayoutHeight
    {
        get
        {
            int headerHeight = 0;

            if (ShowHeader)
            {
                headerHeight = titleHeight;
            }

            return GridHeight + headerHeight;
        }
    }

    [MenuItem("Tools/Inventory System/Inventory Generator")]
    public static void Open()
    {
        var window = GetWindow<InventorySystemInventoryGeneratorWindow>("Inventory Generator");
        window.minSize = new Vector2(780f, 520f);
        window.Show();
    }

    private void OnGUI()
    {
        EnsureStyles();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawSettingsPane();
        GUILayout.Space(10f);
        DrawPreviewPane();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSettingsPane()
    {
        EditorGUILayout.BeginVertical(paneStyle, GUILayout.Width(340f), GUILayout.ExpandHeight(true));
        EditorGUILayout.LabelField("Inventory Generator", titleStyle);
        EditorGUILayout.LabelField("Build a UI Toolkit inventory layout from rows and columns.", subtitleStyle);
        EditorGUILayout.Space(8f);

        settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, false, false);

        DrawSection("Layout");
        inventoryName = EditorGUILayout.TextField("Inventory Name", inventoryName);
        assetName = EditorGUILayout.TextField("Asset Name", assetName);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        showTitle = EditorGUILayout.Toggle("Show Title", showTitle);
        showSortButton = EditorGUILayout.Toggle("Enable Sort Button", showSortButton);
        EndSection();

        DrawSection("Grid");
        horizontalSlots = EditorGUILayout.IntSlider("Horizontal Slots", horizontalSlots, 1, 20);
        verticalSlots = EditorGUILayout.IntSlider("Vertical Slots", verticalSlots, 1, 20);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("Total Slots", SlotCount);
        }
        EndSection();

        DrawSection("Sizing");
        slotSize = EditorGUILayout.IntSlider("Slot Size", slotSize, 24, 120);
        slotGap = EditorGUILayout.IntSlider("Slot Gap", slotGap, 0, 24);
        padding = EditorGUILayout.IntSlider("Padding", padding, 0, 40);
        using (new EditorGUI.DisabledScope(!ShowHeader))
        {
            titleHeight = EditorGUILayout.IntSlider("Header Height", titleHeight, 32, 100);
        }
        EndSection();

        DrawSection("Colors");
        backgroundColor = EditorGUILayout.ColorField("Background", backgroundColor);
        borderColor = EditorGUILayout.ColorField("Border", borderColor);
        slotPreviewColor = EditorGUILayout.ColorField("Preview Slots", slotPreviewColor);
        EndSection();

        DrawSection("Output");
        createSceneObject = EditorGUILayout.Toggle("Create Scene Object", createSceneObject);
        EndSection();

        EditorGUILayout.EndScrollView();

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(assetName)))
        {
            if (GUILayout.Button("Generate Inventory", GUILayout.Height(32f)))
            {
                GenerateInventory();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPreviewPane()
    {
        EditorGUILayout.BeginVertical(paneStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUILayout.LabelField("Live Preview", titleStyle);
        EditorGUILayout.LabelField($"{horizontalSlots} horizontal x {verticalSlots} vertical slots ({SlotCount} total) - {LayoutWidth} x {LayoutHeight}px", subtitleStyle);
        EditorGUILayout.Space(8f);
        DrawPreview();
        EditorGUILayout.EndVertical();
    }

    private void DrawPreview()
    {
        Rect previewArea = GUILayoutUtility.GetRect(240f, 10000f, 220f, 10000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(previewArea, new Color32(24, 24, 26, 255));

        Rect paddedArea = Shrink(previewArea, 18f);
        float scale = Mathf.Min(1f, paddedArea.width / LayoutWidth, paddedArea.height / LayoutHeight);
        float previewWidth = LayoutWidth * scale;
        float previewHeight = LayoutHeight * scale;
        Rect outerRect = new Rect(
            paddedArea.x + ((paddedArea.width - previewWidth) * 0.5f),
            paddedArea.y + ((paddedArea.height - previewHeight) * 0.5f),
            previewWidth,
            previewHeight);

        EditorGUI.DrawRect(outerRect, borderColor);

        Rect panelRect = Shrink(outerRect, Mathf.Max(1f, 2f * scale));
        EditorGUI.DrawRect(panelRect, backgroundColor);

        float y = panelRect.y;
        if (ShowHeader)
        {
            Rect titleRect = new Rect(panelRect.x, y, panelRect.width, titleHeight * scale);
            EditorGUI.DrawRect(titleRect, new Color(0.16f, 0.16f, 0.16f, 1f));
            DrawPreviewTitle(titleRect, scale);
            y += titleRect.height;
        }

        float scaledPadding = padding * scale;
        float scaledSlotOuter = SlotOuterSize * scale;
        float scaledSlotSize = slotSize * scale;
        float scaledGap = slotGap * scale;

        for (int row = 0; row < verticalSlots; row++)
        {
            for (int column = 0; column < horizontalSlots; column++)
            {
                float x = panelRect.x + scaledPadding + (column * scaledSlotOuter) + scaledGap;
                float slotY = y + scaledPadding + (row * scaledSlotOuter) + scaledGap;
                Rect slotRect = new Rect(x, slotY, scaledSlotSize, scaledSlotSize);
                EditorGUI.DrawRect(slotRect, slotPreviewColor);
                Handles.color = new Color(borderColor.r, borderColor.g, borderColor.b, 0.75f);
                Handles.DrawAAPolyLine(1f,
                    new Vector3(slotRect.xMin, slotRect.yMin),
                    new Vector3(slotRect.xMax, slotRect.yMin),
                    new Vector3(slotRect.xMax, slotRect.yMax),
                    new Vector3(slotRect.xMin, slotRect.yMax),
                    new Vector3(slotRect.xMin, slotRect.yMin));
            }
        }
    }

    private void DrawSection(string title)
    {
        EditorGUILayout.BeginVertical(sectionStyle);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(2f);
    }

    private static void EndSection()
    {
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6f);
    }

    private void EnsureStyles()
    {
        paneStyle ??= new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(8, 8, 8, 8)
        };

        sectionStyle ??= new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 10),
            margin = new RectOffset(0, 0, 0, 0)
        };

        titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            fixedHeight = 22
        };

        subtitleStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap = true,
            fixedHeight = 18
        };
    }

    private void DrawPreviewTitle(Rect titleRect, float scale)
    {
        if (showTitle)
        {
            GUI.Label(titleRect, inventoryName, CenteredPreviewLabel());
        }

        if (!showSortButton)
        {
            return;
        }

        float buttonWidth = 50f * scale;
        float buttonHeight = 30f * scale;
        float margin = 10f * scale;
        float buttonX = showTitle
            ? titleRect.center.x + (58f * scale) + margin
            : titleRect.center.x - (buttonWidth * 0.5f);
        Rect buttonRect = new Rect(buttonX, titleRect.center.y - (buttonHeight * 0.5f), buttonWidth, buttonHeight);
        EditorGUI.DrawRect(buttonRect, new Color32(80, 80, 80, 255));
        Handles.color = new Color(1f, 1f, 1f, 0.65f);
        Handles.DrawAAPolyLine(1f,
            new Vector3(buttonRect.xMin, buttonRect.yMin),
            new Vector3(buttonRect.xMax, buttonRect.yMin),
            new Vector3(buttonRect.xMax, buttonRect.yMax),
            new Vector3(buttonRect.xMin, buttonRect.yMax),
            new Vector3(buttonRect.xMin, buttonRect.yMin));
        GUI.Label(buttonRect, "Sort", CenteredPreviewLabel());
    }

    private void GenerateInventory()
    {
        string folder = NormalizeAssetFolder(outputFolder);
        EnsureFolder(folder);

        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{SafeFileName(assetName)}.uxml");
        File.WriteAllText(path, BuildUxml(), Encoding.UTF8);
        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh();

        VisualTreeAsset layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        if (createSceneObject)
        {
            CreateInventoryObject(layout);
        }

        EditorGUIUtility.PingObject(layout);
        Debug.Log($"[Inventory System] Generated inventory layout '{path}' with {SlotCount} slots.");
    }

    private string BuildUxml()
    {
        var builder = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = true,
            NewLineChars = "\n"
        };

        using (XmlWriter writer = XmlWriter.Create(builder, settings))
        {
            writer.WriteStartElement("ui", "UXML", "UnityEngine.UIElements");
            writer.WriteAttributeString("xmlns", "uie", null, "UnityEditor.UIElements");
            writer.WriteAttributeString("editor-extension-mode", "False");

            writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
            writer.WriteAttributeString("name", "InventoryMainContainer");
            writer.WriteAttributeString("style", BuildMainContainerStyle());

            if (ShowHeader)
            {
                writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
                writer.WriteAttributeString("name", "InventoryTitleContainer");
                writer.WriteAttributeString("style", BuildTitleContainerStyle());

                if (showTitle)
                {
                    writer.WriteStartElement("ui", "Label", "UnityEngine.UIElements");
                    writer.WriteAttributeString("text", GetDisplayInventoryName());
                    writer.WriteAttributeString("name", "InventoryTitle");
                    writer.WriteAttributeString("style", "font-size: 24px; color: rgb(255, 255, 255);");
                    writer.WriteEndElement();
                }

                if (showSortButton)
                {
                    writer.WriteStartElement("ui", "Button", "UnityEngine.UIElements");
                    writer.WriteAttributeString("name", "SortButton");
                    writer.WriteAttributeString("text", "Sort");
                    writer.WriteAttributeString("style", "width: 50px; height: 30px; background-color: rgb(80, 80, 80); color: rgb(255, 255, 255); margin-left: 10px; border-top-left-radius: 5px; border-top-right-radius: 5px; border-bottom-right-radius: 5px; border-bottom-left-radius: 5px;");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
            writer.WriteAttributeString("name", "InventorySlotContainer");
            writer.WriteAttributeString("style", BuildSlotContainerStyle());

            for (int row = 0; row < verticalSlots; row++)
            {
                for (int column = 0; column < horizontalSlots; column++)
                {
                    WriteSlotElement(writer, row, column);
                }
            }

            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        return builder.ToString();
    }

    private string BuildMainContainerStyle()
    {
        return $"background-color: {ToRgb(backgroundColor)}; border-top-left-radius: 10px; border-top-right-radius: 10px; border-bottom-right-radius: 10px; border-bottom-left-radius: 10px; border-top-width: 2px; border-right-width: 2px; border-bottom-width: 2px; border-left-width: 2px; border-left-color: {ToRgb(borderColor)}; border-right-color: {ToRgb(borderColor)}; border-top-color: {ToRgb(borderColor)}; border-bottom-color: {ToRgb(borderColor)}; width: {LayoutWidth}px; min-width: {LayoutWidth}px; max-width: {LayoutWidth}px; height: {LayoutHeight}px; min-height: {LayoutHeight}px; max-height: {LayoutHeight}px; flex-direction: column; align-items: center; justify-content: flex-start;";
    }

    private string BuildTitleContainerStyle()
    {
        return $"flex-shrink: 0; width: 100%; height: {titleHeight}px; min-height: {titleHeight}px; max-height: {titleHeight}px; flex-direction: row; align-items: center; justify-content: center; border-bottom-color: {ToRgb(borderColor)}; border-bottom-width: 2px;";
    }

    private string BuildSlotContainerStyle()
    {
        return $"position: relative; width: {GridWidth}px; min-width: {GridWidth}px; max-width: {GridWidth}px; height: {GridHeight}px; min-height: {GridHeight}px; max-height: {GridHeight}px;";
    }

    private void WriteSlotElement(XmlWriter writer, int row, int column)
    {
        int stackBadgeSize = Mathf.Clamp(Mathf.RoundToInt(slotSize * 0.33f), 14, 32);
        int stackFontSize = Mathf.Clamp(Mathf.RoundToInt(slotSize * 0.2f), 9, 16);

        writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
        writer.WriteAttributeString("name", "SlotContainer");
        writer.WriteAttributeString("style", BuildSlotStyle(row, column));

        writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
        writer.WriteAttributeString("name", "RarityContainer");
        writer.WriteAttributeString("style", "position: absolute; width: 100%; height: 100%; border-top-left-radius: 5px; border-top-right-radius: 5px; border-bottom-right-radius: 5px; border-bottom-left-radius: 5px; background-repeat: repeat repeat; background-size: contain;");
        writer.WriteEndElement();

        writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
        writer.WriteAttributeString("name", "ItemSpriteImage");
        writer.WriteAttributeString("style", "position: absolute; top: 0; right: 0; bottom: 0; left: 0;");
        writer.WriteEndElement();

        writer.WriteStartElement("ui", "VisualElement", "UnityEngine.UIElements");
        writer.WriteAttributeString("name", "StackContainer");
        writer.WriteAttributeString("style", $"width: {stackBadgeSize}px; height: {stackBadgeSize}px; max-width: {stackBadgeSize}px; max-height: {stackBadgeSize}px; min-width: {stackBadgeSize}px; min-height: {stackBadgeSize}px; position: absolute; right: 3px; bottom: 3px; background-color: rgb(14, 14, 14); border-top-left-radius: 5px; border-top-right-radius: 5px; border-bottom-right-radius: 5px; border-bottom-left-radius: 5px; display: none; align-items: center; justify-content: center;");

        writer.WriteStartElement("ui", "Label", "UnityEngine.UIElements");
        writer.WriteAttributeString("tabindex", "-1");
        writer.WriteAttributeString("text", "1");
        writer.WriteAttributeString("display-tooltip-when-elided", "true");
        writer.WriteAttributeString("name", "StackLabel");
        writer.WriteAttributeString("style", $"color: rgb(255, 255, 255); margin-top: 0; margin-right: 0; margin-bottom: 0; margin-left: 0; padding-top: 0; padding-right: 0; padding-bottom: 0; padding-left: 0; -unity-text-align: middle-center; font-size: {stackFontSize}px;");
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private string BuildSlotStyle(int row, int column)
    {
        int left = padding + (column * SlotOuterSize) + slotGap;
        int top = padding + (row * SlotOuterSize) + slotGap;
        return $"position: absolute; left: {left}px; top: {top}px; width: {slotSize}px; height: {slotSize}px; max-width: {slotSize}px; max-height: {slotSize}px; min-width: {slotSize}px; min-height: {slotSize}px; border-top-width: 1px; border-right-width: 1px; border-bottom-width: 1px; border-left-width: 1px; border-top-left-radius: 5px; border-top-right-radius: 5px; border-bottom-right-radius: 5px; border-bottom-left-radius: 5px; background-color: {ToRgb(slotPreviewColor)}; border-left-color: {ToRgb(borderColor)}; border-right-color: {ToRgb(borderColor)}; border-top-color: {ToRgb(borderColor)}; border-bottom-color: {ToRgb(borderColor)}; align-items: center; justify-content: center;";
    }

    private void CreateInventoryObject(VisualTreeAsset layout)
    {
        if (layout == null)
        {
            EditorUtility.DisplayDialog("Inventory Generator", "The layout was generated, but Unity could not load it yet. Try creating the scene object after Unity imports the asset.", "OK");
            return;
        }

        string displayInventoryName = GetDisplayInventoryName();
        var gameObject = new GameObject(displayInventoryName);
        Undo.RegisterCreatedObjectUndo(gameObject, "Create Generated Inventory");

        if (Selection.activeTransform != null)
        {
            gameObject.transform.SetParent(Selection.activeTransform, false);
        }

        Inventory inventory = gameObject.AddComponent<Inventory>();
        Set(inventory, "layoutTemplate", layout);
        Set(inventory, "slotCount", SlotCount);
        Set(inventory, "inventoryName", displayInventoryName);

        EditorUtility.SetDirty(inventory);
        Selection.activeGameObject = gameObject;
    }

    private static GUIStyle CenteredPreviewLabel()
    {
        return new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            clipping = TextClipping.Clip
        };
    }

    private static Rect Shrink(Rect rect, float amount)
    {
        return new Rect(rect.x + amount, rect.y + amount, rect.width - (amount * 2f), rect.height - (amount * 2f));
    }

    private static string ToRgb(Color color)
    {
        Color32 value = color;
        return $"rgb({value.r}, {value.g}, {value.b})";
    }

    private static string SafeFileName(string value)
    {
        string safeName = "GeneratedInventory";

        if (!string.IsNullOrWhiteSpace(value))
        {
            safeName = value.Trim();
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            safeName = safeName.Replace(invalidChar.ToString(), string.Empty);
        }

        if (string.IsNullOrWhiteSpace(safeName))
        {
            return "GeneratedInventory";
        }

        return safeName;
    }

    private static string NormalizeAssetFolder(string folder)
    {
        string normalized = DefaultOutputFolder;

        if (!string.IsNullOrWhiteSpace(folder))
        {
            normalized = folder.Trim().Replace('\\', '/');
        }

        normalized = normalized.TrimEnd('/');

        if (normalized.StartsWith("Assets"))
        {
            return normalized;
        }

        return DefaultOutputFolder;
    }

    private string GetDisplayInventoryName()
    {
        if (string.IsNullOrWhiteSpace(inventoryName))
        {
            return "Inventory";
        }

        return inventoryName.Trim();
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static void Set(Object target, string fieldName, Object value)
    {
        var serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void Set(Object target, string fieldName, int value)
    {
        var serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property != null)
        {
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void Set(Object target, string fieldName, string value)
    {
        var serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property != null)
        {
            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}