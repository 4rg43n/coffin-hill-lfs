using System.Collections;
using UnityEngine;

public class NPCController : MonoBehaviour, INPCInteractable
{
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 1f;

    private int _patrolIndex;
    private bool _isPatrolling;

    private void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 1)
            StartCoroutine(PatrolRoutine());
    }

    public void InteractST()
    {
        // Face the player
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            Vector2 dir = (Vector2)(player.transform.position - transform.position);
            // Could update a sprite facing direction here
        }

        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            DialogueManager dm = DialogueManager.GetInstanceST();
            dm?.ShowDialogueST(dialogueLines);
        }
    }

    private IEnumerator PatrolRoutine()
    {
        _isPatrolling = true;
        while (_isPatrolling)
        {
            Transform dest = patrolPoints[_patrolIndex];
            while (Vector2.Distance(transform.position, dest.position) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, dest.position, patrolSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = dest.position;
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
