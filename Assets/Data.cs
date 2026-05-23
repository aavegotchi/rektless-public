using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Menu;
using UnityEngine.Animations;

[System.Serializable]
public class CharacterProjectile 
{
    public string name;
    public RuntimeAnimatorController ProjetileAnimator;
}

public class Data : MonoBehaviourSingletonPersistent<Data>
{
    [SerializeField]
    public List<CharacterProjectile> CharacterProjectiles = new List<CharacterProjectile>();
    public CharacterSelector characterSelector;

    public RuntimeAnimatorController DefaultProjectileAnimator;

    [ContextMenu("FillCharacterNames")]
    public void FillCharacterProjectileNames() 
    {
        CharacterProjectiles.Clear();

       foreach(CharacterData characterInfo in characterSelector.characters) 
       {
            CharacterProjectile newCharcterProjectile = new CharacterProjectile();

            newCharcterProjectile.name = characterInfo.name;

            CharacterProjectiles.Add(newCharcterProjectile);
       }
    }

    public RuntimeAnimatorController GetProjectileAnimatorComponentByCharacterName(string CharacterName) 
    { 
        foreach(CharacterProjectile characterProjectile in CharacterProjectiles) 
        { 
            if(characterProjectile.name == CharacterName) 
            {
                return characterProjectile.ProjetileAnimator != null ? characterProjectile.ProjetileAnimator : DefaultProjectileAnimator;
            }
        }

        return DefaultProjectileAnimator;
    }
}
