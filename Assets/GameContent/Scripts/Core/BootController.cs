using UnityEngine;

public class BootController : MonoBehaviour
{
    [SerializeField] private GameManager gameManagerPrefab;

    private void Start()
    {
        if (GameManager.GetInstanceST() == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }

        // Wait one frame to allow GameManager Awake to run
        StartCoroutine(BootRoutine());
    }

    private System.Collections.IEnumerator BootRoutine()
    {
        yield return null;
        GameManager gm = GameManager.GetInstanceST();
        if (gm != null)
            gm.ChangeStateST(GameState.MainMenu);
    }
}
