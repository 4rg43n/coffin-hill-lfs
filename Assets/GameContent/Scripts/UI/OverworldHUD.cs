using UnityEngine;

public class OverworldHUD : MonoBehaviour
{
    private void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        OnStateChanged(GameManager.GetInstanceST()?.CurrentState ?? GameState.Boot);
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        gameObject.SetActive(state == GameState.Overworld);
    }
}
