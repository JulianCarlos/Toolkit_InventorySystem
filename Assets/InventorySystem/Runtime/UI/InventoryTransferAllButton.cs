using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryTransferAllButton : MonoBehaviour
{
    #region Inspector Fields

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string buttonName = "TransferAllButton";
    [SerializeField] private Inventory sourceInventory;
    [SerializeField] private Inventory targetInventory;

    #endregion

    #region Runtime Fields

    private Button transferButton;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        Bind();
    }

    private void OnDisable()
    {
        if (transferButton != null)
        {
            transferButton.clicked -= HandleTransferAllClicked;
        }
    }

    #endregion

    #region Binding

    public void Bind()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null || string.IsNullOrEmpty(buttonName))
        {
            return;
        }

        if (transferButton != null)
        {
            transferButton.clicked -= HandleTransferAllClicked;
        }

        transferButton = uiDocument.rootVisualElement.Q<Button>(buttonName);

        if (transferButton != null)
        {
            transferButton.clicked += HandleTransferAllClicked;
        }
    }

    public void Configure(Inventory source, Inventory target)
    {
        sourceInventory = source;
        targetInventory = target;
    }

    #endregion

    #region Transfer

    public InventoryOperationResult TransferAll()
    {
        if (sourceInventory == null || targetInventory == null)
        {
            return InventoryOperationResult.None(0);
        }

        return sourceInventory.TransferAllTo(targetInventory);
    }

    private void HandleTransferAllClicked()
    {
        TransferAll();
    }

    #endregion
}