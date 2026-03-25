using UnityEngine;

// MapTransitionTrigger is a legacy tilemap concept — kept for now but not active in node-map mode.
public class MapTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string destinationScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        OverworldManager.GetInstanceST()?.SaveMapStateST();

        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        slm?.SaveActiveSlateST();

        GameManager.GetInstanceST()?.SceneTransitionManager
            .LoadSceneST(destinationScene);
    }
}
