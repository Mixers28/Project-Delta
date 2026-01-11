using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    [SerializeField] private Text statusLabel;

    private void Start()
    {
#if UNITY_WEBGL
        WebPersistenceManager.Initialize(this, SetStatus);
#else
        SetStatus("Login skipped (non-WebGL platform).");
#endif
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
        }
        Debug.Log($"[LoginScreen] {message}");
    }
}
