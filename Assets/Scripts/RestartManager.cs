using System;
using System.Collections;
using System.Collections.Generic;
using Menu;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterInfo
{
    public string Name;
    public Texture2D Texture;
    public bool Locked;

    public CharacterInfo(string name, Texture2D texture)
    {
        Name = name;
        Texture = texture;
        Locked = PlayerPrefs.GetInt(name, 1) == 1;
    }

    public void Unlock()
    {
        Locked = false;
        PlayerPrefs.SetInt(Name, 0);
    }
}

public class RestartManager : MonoBehaviourSingletonPersistent<RestartManager>
{
    public Action BeforeRestart;
    public bool CameFromGame { get; set; }

    //public Dictionary<string, CharacterInfo> CharacterTextures { get; set; } = new Dictionary<string, CharacterInfo>();
    public string SelectedCharacterName { get; set; }
    //public CharacterInfo SelectedCharacter => CharacterTextures[SelectedCharacterName];

    public void Restart(float time)
    {
        StartCoroutine(RestartCoroutine(time));
    }

    private IEnumerator RestartCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        BeforeRestart?.Invoke();
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CameFromGame = true;
        SceneManager.LoadScene("menu");
    }

    //public IEnumerator DownloadAll(List<CharacterData> characters, Action<List<CharacterData>> onFinish)
    //{
    //    List<string> waitingForDownload = new List<string>();
    //    List<string> gonnaRemove = new List<string>();

    //    foreach (var character in characters)
    //    {
    //        var dict = new();//CharacterTextures;
    //        if (!dict.ContainsKey(character.name))
    //        {
    //            if (character.inGameTexture == null &&
    //                string.IsNullOrEmpty(character.inGameTextureUrl))
    //            {
    //                gonnaRemove.Add(character.name);
    //                Debug.LogWarning("Character url/texture is not set");
    //                continue;
    //            }

    //            if (character.inGameTexture != null)
    //            {
    //                var ch = new CharacterInfo(character.name, character.inGameTexture);
    //                if (!character.locked)
    //                {
    //                    ch.Unlock();
    //                }

    //                character.locked = ch.Locked;


    //                dict.Add(character.name, ch);
    //            }
    //            else
    //            {
    //                waitingForDownload.Add(character.name);
    //                StartCoroutine(TextureDownloader.Instance.DownloadTexture(character.inGameTextureUrl,
    //                    texture =>
    //                    {
    //                        var ch = new CharacterInfo(character.name, texture);
    //                        character.locked = ch.Locked;
    //                        if (!character.locked)
    //                        {
    //                            ch.Unlock();
    //                        }

    //                        dict.Add(character.name, ch);
    //                        waitingForDownload.Remove(character.name);
    //                    },
    //                    () =>
    //                    {
    //                        gonnaRemove.Add(character.name);
    //                        waitingForDownload.Remove(character.name);
    //                    }));
    //            }
    //        }
    //        else
    //        {
    //            character.locked = dict[character.name].Locked;
    //        }
    //    }

    //    while (waitingForDownload.Count > 0)
    //    {
    //        yield return null;
    //    }

    //    foreach (var _name in gonnaRemove)
    //    {
    //        characters.Remove(characters.Find(c => c.name == _name));
    //    }

    //    onFinish?.Invoke(characters);

    //    yield return null;
    //}
}