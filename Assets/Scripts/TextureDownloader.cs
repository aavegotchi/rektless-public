using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TextureDownloader : MonoBehaviourSingleton<TextureDownloader>
{
    public IEnumerator DownloadTexture(string textureUrl, Action<Texture2D> onDownloaded, Action onFailed = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(textureUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download texture: " + www.error);
                Debug.LogError("Texture URL: " + textureUrl);
                onFailed?.Invoke();
            }
            else
            {
                byte[] results = www.downloadHandler.data;
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(results);
                texture.filterMode = FilterMode.Point;
                onDownloaded?.Invoke(texture);
            }
        }
    }
}