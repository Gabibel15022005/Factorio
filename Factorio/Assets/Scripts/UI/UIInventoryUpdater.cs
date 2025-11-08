using UnityEngine;


public class UIInventoryUpdater : MonoBehaviour
{
    [SerializeField] private ListRessourcesUI ressourcesUI;

    private void Start()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnResourceChanged += OnInventoryChanged;

        // Afficher les ressources déjà présentes au démarrage
        ressourcesUI.UpdateRessources(InventoryManager.Instance.GetResources());
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnResourceChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged(ResourceData type, int newCount)
    {
        ressourcesUI.UpdateRessources(InventoryManager.Instance.GetResources());
    }
}