using UnityEngine;
using UnityEngine.SceneManagement;

public class DefeatScreenController : MonoBehaviour
{
    public void OnMenuClick()
    {
        SceneManager.LoadScene("Menu");
    }
    public void OnExitClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
