using UnityEngine;

public class PortraitLock : MonoBehaviour
{
    private void Awake()
    {
        EnforcePortrait();
    }

    public void EnforcePortrait()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToPortrait = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_ANDROID
        if (hasFocus)
            EnforcePortrait();
#endif
    }
}
