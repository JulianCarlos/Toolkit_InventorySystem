using UnityEngine;
using UnityEngine.UIElements;

public class DemoUIManager : MonoBehaviour
{
    public static DemoUIManager Instance { get; private set; }

    [SerializeField] private UIDocument playerHUD;

    private VisualElement rootElement;

    // Inventory containers
    private VisualElement playerInventoryContainer;
    private VisualElement playerEquipmentContainer;
    private VisualElement hotbarContainer;
    private VisualElement targetInventoryContainer;

    // Inventory visual elements (cached)
    private VisualElement playerInventoryUI;
    private VisualElement playerEquipmentUI;
    private VisualElement hotbarUI;
    private VisualElement targetInventoryUI;

    private bool isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;
        if (playerHUD == null) return;

        rootElement = playerHUD.rootVisualElement;
        if (rootElement == null) return;

        playerInventoryContainer = rootElement.Q<VisualElement>("PlayerInventoryContainer");
        playerEquipmentContainer = rootElement.Q<VisualElement>("PlayerEquipmentContainer");
        hotbarContainer = rootElement.Q<VisualElement>("HotbarContainer");
        targetInventoryContainer = rootElement.Q<VisualElement>("TargetInventoryContainer");

        isInitialized = playerInventoryContainer != null;
    }

    public void SetupPlayerInventories(Inventory playerInventory, Inventory equipmentInventory, Inventory hotbarInventory)
    {
        EnsureInitialized();

        if (playerInventoryUI == null && playerInventory != null && playerInventoryContainer != null)
        {
            playerInventoryUI = playerInventory.GetInventoryUI();
            playerInventoryContainer.Clear();
            playerInventoryContainer.Add(playerInventoryUI);
        }

        if (playerEquipmentUI == null && equipmentInventory != null && playerEquipmentContainer != null)
        {
            playerEquipmentUI = equipmentInventory.GetInventoryUI();
            playerEquipmentContainer.Clear();
            playerEquipmentContainer.Add(playerEquipmentUI);
        }

        if (hotbarUI == null && hotbarInventory != null && hotbarContainer != null)
        {
            hotbarUI = hotbarInventory.GetInventoryUI();
            hotbarContainer.Clear();
            hotbarContainer.Add(hotbarUI);
        }
    }

    public void ShowPlayerInventory()
    {
        EnsureInitialized();
        ShowElement(playerInventoryContainer, true);
        ShowElement(playerEquipmentContainer, true);
        ShowElement(targetInventoryContainer, false);
    }

    public void ShowTargetInventory(Inventory target)
    {
        if (target == null) return;

        EnsureInitialized();
        if (targetInventoryContainer == null) return;

        targetInventoryContainer.Clear();
        targetInventoryUI = target.GetInventoryUI();
        targetInventoryContainer.Add(targetInventoryUI);

        ShowElement(playerInventoryContainer, true);
        ShowElement(playerEquipmentContainer, false);
        ShowElement(targetInventoryContainer, true);
    }

    public void ShowHotbarOnly()
    {
        EnsureInitialized();
        ShowElement(playerInventoryContainer, false);
        ShowElement(playerEquipmentContainer, false);
        ShowElement(targetInventoryContainer, false);
        ShowElement(hotbarContainer, true);
    }

    public void HideAllInventories()
    {
        EnsureInitialized();
        ShowElement(playerInventoryContainer, false);
        ShowElement(playerEquipmentContainer, false);
        ShowElement(hotbarContainer, false);
        ShowElement(targetInventoryContainer, false);
    }

    private void ShowElement(VisualElement element, bool show)
    {
        if (element != null)
        {
            element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
