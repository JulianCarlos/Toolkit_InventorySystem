using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SO_Item), true)]
public class SO_ItemEditor : Editor
{
    // --- Layout constants ---
    private const float IconSize = 96f;
    private const float HeaderHeight = 112f;
    private const float SectionSpacing = 6f;
    private const float InnerPadding = 8f;
    private const float RarityBarHeight = 3f;
    private const float CatalogEditButtonWidth = 42f;

    private static bool foldIdentity = true;
    private static bool foldVisual = true;
    private static bool foldGameplay = true;
    private static bool foldEquipment = true;

    private static GUIStyle headerNameStyle;
    private static GUIStyle headerSubStyle;
    private static GUIStyle sectionHeaderStyle;
    private static GUIStyle centeredMiniLabel;
    private static bool stylesBuilt;

    // ---------------------------------------------------------------
    // Inspector
    // ---------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        BuildStyles();
        serializedObject.Update();

        var iconProp = serializedObject.FindProperty("icon");
        var nameProp = serializedObject.FindProperty("itemName");
        var descProp = serializedObject.FindProperty("description");
        var guidProp = serializedObject.FindProperty("guid");
        var stackProp = serializedObject.FindProperty("stackSize");
        var typeProp = serializedObject.FindProperty("itemType");
        var catalogProp = serializedObject.FindProperty("rarityCatalog");
        var rarityProp = serializedObject.FindProperty("defaultRarityName");

        if (string.IsNullOrWhiteSpace(rarityProp.stringValue))
        {
            rarityProp.stringValue = ((SO_Item)target).DefaultRarity;
        }

        var icon = iconProp.objectReferenceValue as Texture2D;
        var catalog = catalogProp.objectReferenceValue as SO_RarityCatalog;
        Color rarityCol = catalog != null
            ? catalog.GetColor(rarityProp.stringValue)
            : RarityDefinition.GetFallbackColor(rarityProp.stringValue);

        DrawHeaderCard(icon, nameProp.stringValue, typeProp, rarityProp, rarityCol);

        EditorGUILayout.Space(SectionSpacing);

        foldIdentity = DrawSectionFoldout("Identity", foldIdentity);
        if (foldIdentity)
        {
            BeginSection();
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
            EditorGUILayout.PropertyField(descProp, new GUIContent("Description"));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(guidProp, new GUIContent("GUID"));
            EditorGUI.EndDisabledGroup();
            EndSection();
        }

        EditorGUILayout.Space(SectionSpacing);

        foldVisual = DrawSectionFoldout("Visual", foldVisual);
        if (foldVisual)
        {
            BeginSection();
            EditorGUILayout.PropertyField(iconProp, new GUIContent("Icon"));
            EndSection();
        }

        EditorGUILayout.Space(SectionSpacing);

        foldGameplay = DrawSectionFoldout("Gameplay", foldGameplay);
        if (foldGameplay)
        {
            BeginSection();
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Type"));
            EditorGUILayout.PropertyField(catalogProp, new GUIContent("Rarity Catalog"));
            DrawRaritySelector(rarityProp, catalog);
            EditorGUILayout.PropertyField(stackProp, new GUIContent("Max Stack"));
            EndSection();
        }

        var equipProp = serializedObject.FindProperty("EquipmentType");
        if (equipProp != null)
        {
            EditorGUILayout.Space(SectionSpacing);
            foldEquipment = DrawSectionFoldout("Equipment", foldEquipment);
            if (foldEquipment)
            {
                BeginSection();
                EditorGUILayout.PropertyField(equipProp, new GUIContent("Slot"));
                EndSection();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderCard(Texture2D icon, string itemName, SerializedProperty typeProp, SerializedProperty rarityProp, Color rarityCol)
    {
        var headerRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(HeaderHeight));

        EditorGUILayout.BeginHorizontal();

        var iconRect = GUILayoutUtility.GetRect(IconSize, IconSize, GUILayout.Width(IconSize));
        if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawRect(iconRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.Label(iconRect, "No Icon", centeredMiniLabel);
        }

        GUILayout.Space(12);

        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        string displayName = itemName;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = target.name;
        }

        EditorGUILayout.LabelField(displayName, headerNameStyle);

        string subtitle = typeProp.enumDisplayNames[typeProp.enumValueIndex]
                          + "  •  "
                          + RarityDefinition.NormalizeName(rarityProp.stringValue);
            EditorGUILayout.LabelField(subtitle, headerSubStyle);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        var barRect = new Rect(headerRect.x, headerRect.yMax - RarityBarHeight, headerRect.width, RarityBarHeight);
        EditorGUI.DrawRect(barRect, rarityCol);
    }

    private static bool DrawSectionFoldout(string title, bool foldout)
    {
        EditorGUILayout.BeginHorizontal();
        foldout = EditorGUILayout.Foldout(foldout, title, true, sectionHeaderStyle);
        EditorGUILayout.EndHorizontal();
        return foldout;
    }

    private static void DrawRaritySelector(SerializedProperty rarityProp, SO_RarityCatalog catalog)
    {
        if (catalog == null)
        {
            EditorGUILayout.PropertyField(rarityProp, new GUIContent("Rarity"));
            return;
        }

        string[] rarityNames = catalog.GetRarityNames();
        if (rarityNames.Length == 0)
        {
            EditorGUILayout.PropertyField(rarityProp, new GUIContent("Rarity"));
            return;
        }

        string normalizedRarity = RarityDefinition.NormalizeName(rarityProp.stringValue);
        int selectedIndex = System.Array.FindIndex(rarityNames, rarityName => string.Equals(rarityName, normalizedRarity, System.StringComparison.OrdinalIgnoreCase));
        int customIndex = rarityNames.Length;
        string[] options = new string[rarityNames.Length + 1];

        System.Array.Copy(rarityNames, options, rarityNames.Length);
        options[customIndex] = "Custom...";

        EditorGUILayout.BeginHorizontal();
        int popupIndex = customIndex;

        if (selectedIndex >= 0)
        {
            popupIndex = selectedIndex;
        }

        int nextIndex = EditorGUILayout.Popup("Rarity", popupIndex, options);
        if (GUILayout.Button("Edit", GUILayout.Width(CatalogEditButtonWidth)))
        {
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
        }
        EditorGUILayout.EndHorizontal();

        if (nextIndex < rarityNames.Length)
        {
            rarityProp.stringValue = rarityNames[nextIndex];
        }
        else
        {
            EditorGUILayout.PropertyField(rarityProp, new GUIContent("Custom Rarity"));
        }
    }

    private static void BeginSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(InnerPadding * 0.5f);
        EditorGUI.indentLevel++;
    }

    private static void EndSection()
    {
        EditorGUI.indentLevel--;
        GUILayout.Space(InnerPadding * 0.5f);
        EditorGUILayout.EndVertical();
    }

    private static void BuildStyles()
    {
        if (stylesBuilt)
        {
            return;
        }

        stylesBuilt = true;

        headerNameStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
            margin = new RectOffset(0, 0, 0, 2)
        };

        headerSubStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 11,
            fontStyle = FontStyle.Italic,
            normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
        };

        sectionHeaderStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        centeredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
    }

    // ---------------------------------------------------------------
    // Preview panel
    // ---------------------------------------------------------------
    public override bool HasPreviewGUI() => ((SO_Item)target).Icon != null;

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        var item = (SO_Item)target;
        if (item.Icon != null)
            GUI.DrawTexture(r, item.Icon, ScaleMode.ScaleToFit);
    }

    public override GUIContent GetPreviewTitle()
    {
        return new GUIContent(((SO_Item)target).ItemName ?? "Item Preview");
    }
}
