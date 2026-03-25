using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI placeholderText;

    private void OnEnable()
    {
        if (placeholderText)
            placeholderText.text = "Bag (coming soon)";
    }
}
