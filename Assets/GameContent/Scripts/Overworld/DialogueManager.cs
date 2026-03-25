using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager GetInstanceST() => _instance;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float charDelay = 0.04f;

    private string[] _currentLines;
    private int _lineIndex;
    private Action _onComplete;
    private bool _isTyping;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsShowing) return;

        InputManager input = GameManager.GetInstanceST()?.InputManager;
        if (input == null) return;

        if (input.OnConfirmST().WasPressedThisFrame())
            AdvanceST();
    }

    public void ShowDialogueST(string[] lines, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }
        _currentLines = lines;
        _lineIndex = 0;
        _onComplete = onComplete;
        IsShowing = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        StartCoroutine(TypeLineRoutine(_currentLines[_lineIndex]));
    }

    public void AdvanceST()
    {
        if (_isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = _currentLines[_lineIndex];
            _isTyping = false;
            return;
        }

        _lineIndex++;
        if (_lineIndex < _currentLines.Length)
        {
            StartCoroutine(TypeLineRoutine(_currentLines[_lineIndex]));
        }
        else
        {
            CloseST();
        }
    }

    private void CloseST()
    {
        IsShowing = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        _onComplete?.Invoke();
        _onComplete = null;
    }

    private IEnumerator TypeLineRoutine(string line)
    {
        _isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        _isTyping = false;
    }
}
