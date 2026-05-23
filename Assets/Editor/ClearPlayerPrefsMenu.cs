using UnityEngine;
using UnityEditor;

public class ClearPlayerPrefsMenu : MonoBehaviour
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs have been cleared.");
    }
}