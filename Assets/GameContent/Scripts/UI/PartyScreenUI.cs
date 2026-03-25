using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject[] slots;
    [SerializeField] private Image[] slotSprites;
    [SerializeField] private TextMeshProUGUI[] slotNames;
    [SerializeField] private TextMeshProUGUI[] slotLevels;
    [SerializeField] private Image[] slotHPBars;
    [SerializeField] private TextMeshProUGUI[] slotStatusTexts;

    private void OnEnable()
    {
        RefreshST();
    }

    public void RefreshST()
    {
        // Party data would come from SaveLoadManager once wired
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]) slots[i].SetActive(false);
        }
    }
}
