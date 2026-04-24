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

    // --- Foldout state (persisted per-session via EditorPrefs) ---
    private static bool _foldIdentity = true;
    private static bool _foldVisual = true;
    private static bool _foldGameplay = true;
    private static bool _foldEquipment = true;

    // --- Cached styles (built once) ---
    private static GUIStyle _headerNameStyle;
    private static GUIStyle _headerSubStyle;
    private static GUIStyle _sectionHeaderStyle;
    private static GUIStyle _descriptionStyle;
    private static GUIStyle _centeredMiniLabel;
    private static bool _stylesBuilt;

    // --- Rarity palette ---
    private static readonly Color[] RarityColors = new Color[]
    {
        new Color(0.66f, 0.66f, 0.66f),  // Common       — grey
        new Color(0.18f, 0.80f, 0.25f),  // Uncommon     — green
        new Color(0.24f, 0.55f, 0.95f),  // Rare         — blue
        new Color(0.70f, 0.30f, 0.90f),  // VeryRare     — purple
        new Color(1.00f, 0.60f, 0.10f),  // Legendary    — orange
        new Color(0.95f, 0.85f, 0.15f),  // Unique       — gold
    };

    // ---------------------------------------------------------------
    // Inspector
    // ---------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        BuildStyles();
        serializedObject.Update();

        var iconProp    = serializedObject.FindProperty("Icon");
        var nameProp    = serializedObject.FindProperty("ItemName");
        var descProp    = serializedObject.FindProperty("Description");
        var guidProp    = serializedObject.FindProperty("Guid");
        var stackProp   = serializedObject.FindProperty("StackSize");
        var typeProp    = serializedObject.FindProperty("ItemType");
        var rarityProp  = serializedObject.FindProperty("DefaultRarity");

        var icon = iconProp.objectReferenceValue as Texture2D;
        Color rarityCol = GetRarityColor(rarityProp.enumValueIndex);

        // ── Header card ──────────────────────────────────────────
        DrawHeaderCard(icon, nameProp.stringValue, typeProp, rarityProp, rarityCol);

        EditorGUILayout.Space(SectionSpacing);

        // ── Identity section ─────────────────────────────────────
        _foldIdentity = DrawSectionFoldout("Identity", _foldIdentity);
        if (_foldIdentity)
        {
            BeginSection();
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
            EditorGUILayout.PropertyField(descProp, new GUIContent("Description"));

            // Draw Guid as read-only, copiable
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(guidProp, new GUIContent("GUID"));
            EditorGUI.EndDisabledGroup();
            EndSection();
        }

        EditorGUILayout.Space(SectionSpacing);

        // ── Visual section ───────────────────────────────────────
        _foldVisual = DrawSectionFoldout("Visual", _foldVisual);
        if (_foldVisual)
        {
            BeginSection();
            EditorGUILayout.PropertyField(iconProp, new GUIContent("Icon"));
            EndSection();
        }

        EditorGUILayout.Space(SectionSpacing);

        // ── Gameplay section ─────────────────────────────────────
        _foldGameplay = DrawSectionFoldout("Gameplay", _foldGameplay);
        if (_foldGameplay)
        {
            BeginSection();
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Type"));
            EditorGUILayout.PropertyField(rarityProp, new GUIContent("Rarity"));
            EditorGUILayout.PropertyField(stackProp, new GUIContent("Max Stack"));
            EndSection();
        }

        // ── Equipment section (only for SO_Equipment) ────────────
        var equipProp = serializedObject.FindProperty("EquipmentType");
        if (equipProp != null)
        {
            EditorGUILayout.Space(SectionSpacing);
            _foldEquipment = DrawSectionFoldout("Equipment", _foldEquipment);
            if (_foldEquipment)
            {
                BeginSection();
                EditorGUILayout.PropertyField(equipProp, new GUIContent("Slot"));
                EndSection();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------
    // Header card
    // ---------------------------------------------------------------
    private void DrawHeaderCard(Texture2D icon, string itemName, SerializedProperty typeProp, SerializedProperty rarityProp, Color rarityCol)
    {
        // Outer box
        var headerRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(HeaderHeight));

        EditorGUILayout.BeginHorizontal();

        // Icon thumbnail
        var iconRect = GUILayoutUtility.GetRect(IconSize, IconSize, GUILayout.Width(IconSize));
        if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawRect(iconRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.Label(iconRect, "No Icon", _centeredMiniLabel);
        }

        GUILayout.Space(12);

        // Right side: name + subtitle
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        string displayName = string.IsNullOrWhiteSpace(itemName) ? target.name : itemName;
        EditorGUILayout.LabelField(displayName, _headerNameStyle);

        string subtitle = typeProp.enumDisplayNames[typeProp.enumValueIndex]
                          + "  •  "
                          + rarityProp.enumDisplayNames[rarityProp.enumValueIndex];
        EditorGUILayout.LabelField(subtitle, _headerSubStyle);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // Rarity accent bar along the bottom of the header
        var barRect = new Rect(headerRect.x, headerRect.yMax - RarityBarHeight, headerRect.width, RarityBarHeight);
        EditorGUI.DrawRect(barRect, rarityCol);
    }

    // ---------------------------------------------------------------
    // Section helpers
    // ---------------------------------------------------------------
    private static bool DrawSectionFoldout(string title, bool foldout)
    {
        EditorGUILayout.BeginHorizontal();
        foldout = EditorGUILayout.Foldout(foldout, title, true, _sectionHeaderStyle);
        EditorGUILayout.EndHorizontal();
        return foldout;
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

    // ---------------------------------------------------------------
    // Utility
    // ---------------------------------------------------------------
    private static Color GetRarityColor(int index)
    {
        if (index >= 0 && index < RarityColors.Length)
            return RarityColors[index];
        return Color.white;
    }

    private static Rect CenterRect(Rect area, float w, float h)
    {
        return new Rect(area.x + (area.width - w) * 0.5f,
                        area.y + (area.height - h) * 0.5f,
                        w, h);
    }

    private static void BuildStyles()
    {
        if (_stylesBuilt) return;
        _stylesBuilt = true;

        _headerNameStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
            margin = new RectOffset(0, 0, 0, 2)
        };

        _headerSubStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 11,
            fontStyle = FontStyle.Italic,
            normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
        };

        _sectionHeaderStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        _descriptionStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true
        };

        _centeredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
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
