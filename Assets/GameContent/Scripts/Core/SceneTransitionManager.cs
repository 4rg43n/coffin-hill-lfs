using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private GameObject transitionFadePrefab;

    private Image _fadeImage;
    private const float FadeDuration = 0.3f;

    private void Awake()
    {
        if (transitionFadePrefab != null)
        {
            GameObject fadeObj = Instantiate(transitionFadePrefab);
            DontDestroyOnLoad(fadeObj);
            _fadeImage = fadeObj.GetComponentInChildren<Image>();
            if (_fadeImage != null)
                ApplyAlpha(0f);
        }
    }

    public void LoadSceneST(string sceneName, Action onComplete = null)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, onComplete));
    }

    public void LoadSceneAdditiveST(string sceneName, Action onComplete = null)
    {
        StartCoroutine(LoadAdditiveRoutine(sceneName, onComplete));
    }

    public void UnloadSceneST(string sceneName, Action onComplete = null)
    {
        StartCoroutine(UnloadSceneRoutine(sceneName, onComplete));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Action onComplete)
    {
        yield return DoFade(0f, 1f);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return DoFade(1f, 0f);
        onComplete?.Invoke();
    }

    private IEnumerator LoadAdditiveRoutine(string sceneName, Action onComplete)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        onComplete?.Invoke();
    }

    private IEnumerator UnloadSceneRoutine(string sceneName, Action onComplete)
    {
        yield return DoFade(0f, 1f);
        yield return SceneManager.UnloadSceneAsync(sceneName);
        yield return DoFade(1f, 0f);
        onComplete?.Invoke();
    }

    private IEnumerator DoFade(float from, float to)
    {
        if (_fadeImage == null) yield break;
        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.deltaTime;
            ApplyAlpha(Mathf.Lerp(from, to, elapsed / FadeDuration));
            yield return null;
        }
        ApplyAlpha(to);
    }

    private void ApplyAlpha(float alpha)
    {
        if (_fadeImage == null) return;
        Color c = _fadeImage.color;
        c.a = alpha;
        _fadeImage.color = c;
        _fadeImage.gameObject.SetActive(alpha > 0.001f);
    }
}
