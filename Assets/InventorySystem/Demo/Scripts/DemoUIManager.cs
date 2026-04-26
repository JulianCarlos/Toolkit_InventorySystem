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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        rootElement = playerHUD.rootVisualElement;

        playerInventoryContainer = rootElement.Q<VisualElement>("PlayerInventoryContainer");
        playerEquipmentContainer = rootElement.Q<VisualElement>("PlayerEquipmentContainer");
        hotbarContainer = rootElement.Q<VisualElement>("HotbarContainer");
        targetInventoryContainer = rootElement.Q<VisualElement>("TargetInventoryContainer");
    }

    public void SetupPlayerInventories(Inventory playerInventory, Inventory equipmentInventory, Inventory hotbarInventory)
    {
        if (playerInventoryUI == null && playerInventory != null)
        {
            playerInventoryUI = playerInventory.GetInventoryUI();
            playerInventoryContainer.Clear();
            playerInventoryContainer.Add(playerInventoryUI);
        }

        if (playerEquipmentUI == null && equipmentInventory != null)
        {
            playerEquipmentUI = equipmentInventory.GetInventoryUI();
            playerEquipmentContainer.Clear();
            playerEquipmentContainer.Add(playerEquipmentUI);
        }

        if (hotbarUI == null && hotbarInventory != null)
        {
            hotbarUI = hotbarInventory.GetInventoryUI();
            hotbarContainer.Clear();
            hotbarContainer.Add(hotbarUI);
        }
    }

    public void ShowPlayerInventory()
    {
        ShowElement(playerInventoryContainer, true);
        ShowElement(playerEquipmentContainer, true);
        ShowElement(targetInventoryContainer, false);
    }

    public void ShowTargetInventory(Inventory target)
    {
        if (target == null)
        {
            return;
        }

        targetInventoryContainer.Clear();
        targetInventoryUI = target.GetInventoryUI();
        targetInventoryContainer.Add(targetInventoryUI);

        ShowElement(playerInventoryContainer, true);
        ShowElement(playerEquipmentContainer, false);
        ShowElement(targetInventoryContainer, true);
    }

    public void ShowHotbarOnly()
    {
        ShowElement(playerInventoryContainer, false);
        ShowElement(playerEquipmentContainer, false);
        ShowElement(targetInventoryContainer, false);
        ShowElement(hotbarContainer, true);
    }

    public void HideAllInventories()
    {
        ShowElement(playerInventoryContainer, false);
        ShowElement(playerEquipmentContainer, false);
        ShowElement(hotbarContainer, false);
        ShowElement(targetInventoryContainer, false);
    }

    private void ShowElement(VisualElement element, bool show)
    {
        if (element != null)
        {
            if (show)
            {
                element.style.display = DisplayStyle.Flex;
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }
}
