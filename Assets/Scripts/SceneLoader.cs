// Assets/Scripts/SceneLoader.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;          // ← TextMeshPro namespace
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // Hagyományos UI.Text helyett TextMeshProUGUI
    public TMP_Text loadingText;

    void Start()
    {
        StartCoroutine(AttemptStart());
    }

    private IEnumerator AttemptStart()
    {
        yield return new WaitForSeconds(0.5f);

        yield return BardAPIManager.Instance.CheckCanStart(canStart =>
        {
            if (canStart)
            {
                SceneManager.LoadScene("Main");
            }
            else
            {
                loadingText.text = "Cannot start test.";
            }
        });
    }
}
