using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Pattern))]
public class PatternEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector layout
        base.OnInspectorGUI();

        // Get the reference to the Pattern component
        Pattern pattern = (Pattern)target;

        // Add a button to the inspector
        if (GUILayout.Button("Save Pattern"))
        {
            Pattern[] patterns = Resources.LoadAll<Pattern>("PatternPrefabs");
            foreach (Pattern p in patterns)
                p.SetUpArray();

            // Call ConfigureDictionary method on the pattern instance
            SetUpArray();

            void SetUpArray()
            {
                List<D_Spawnable> spawnablesList = new();
                List<Vector2> positionsList = new();
                List<Quaternion> rotationsList = new();
                for (int i = 0; i < pattern.transform.childCount; i++)
                {
                    if (pattern.transform.GetChild(i).gameObject && pattern.transform.GetChild(i).gameObject.TryGetComponent(out Spawnable spawnable))
                    {
                        spawnablesList.Add(spawnable.SpawnableType);
                        positionsList.Add(pattern.transform.GetChild(i).position);
                        rotationsList.Add(pattern.transform.GetChild(i).rotation);
                    }
                }
                pattern.spawnables = spawnablesList.ToArray();
                pattern.positions = positionsList.ToArray();
                pattern.rotations = rotationsList.ToArray();
            }

            // Mark the object as dirty to save changes in the editor
            EditorUtility.SetDirty(pattern);
        }
    }


}


[CustomEditor(typeof(BossPattern))]
public class BossPatternEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector layout
        base.OnInspectorGUI();

        // Get the reference to the Pattern component
        BossPattern pattern = (BossPattern)target;

        // Add a button to the inspector
        if (GUILayout.Button("Save Pattern"))
        {
            // Call ConfigureDictionary method on the pattern instance
            SetUpArray();

            void SetUpArray()
            {
                List<D_Spawnable> spawnablesList = new();
                List<Vector2> positionsList = new();
                List<Quaternion> rotationsList = new();
                for (int i = 0; i < pattern.transform.childCount; i++)
                {
                    if (pattern.transform.GetChild(i).gameObject && pattern.transform.GetChild(i).gameObject.TryGetComponent(out Spawnable spawnable))
                    {
                        spawnablesList.Add(spawnable.SpawnableType);
                        positionsList.Add(pattern.transform.GetChild(i).position);
                        rotationsList.Add(pattern.transform.GetChild(i).rotation);
                    }
                }
                pattern.spawnables = spawnablesList.ToArray();
                pattern.positions = positionsList.ToArray();
                pattern.rotations = rotationsList.ToArray();
            }

            // Mark the object as dirty to save changes in the editor
            EditorUtility.SetDirty(pattern);
        }
    }
}


[CustomEditor(typeof(MeteorPattern))]
public class MeteorPatternEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector layout
        base.OnInspectorGUI();

        // Get the reference to the Pattern component
        MeteorPattern pattern = (MeteorPattern)target;

        // Add a button to the inspector
        if (GUILayout.Button("Save Pattern"))
        {
            // Call ConfigureDictionary method on the pattern instance
            SetUpArray();

            void SetUpArray()
            {
                List<D_Spawnable> spawnablesList = new();
                List<Vector2> positionsList = new();
                List<Quaternion> rotationsList = new();
                for (int i = 0; i < pattern.transform.childCount; i++)
                {
                    if (pattern.transform.GetChild(i).gameObject && pattern.transform.GetChild(i).gameObject.TryGetComponent(out Spawnable spawnable))
                    {
                        spawnablesList.Add(spawnable.SpawnableType);
                        positionsList.Add(pattern.transform.GetChild(i).position);
                        rotationsList.Add(pattern.transform.GetChild(i).rotation);
                    }
                }
                pattern.spawnables = spawnablesList.ToArray();
                pattern.positions = positionsList.ToArray();
                pattern.rotations = rotationsList.ToArray();
            }

            // Mark the object as dirty to save changes in the editor
            EditorUtility.SetDirty(pattern);
        }
    }
}