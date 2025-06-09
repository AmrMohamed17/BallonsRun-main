using System;
using UnityEngine;

[Serializable]
public class CharacterItem
{
    public string Name;
    public Texture2D Icon;
    public Texture2D Texture;
    public RuntimeAnimatorController AnimatorController;
    public bool Unlocked;
    public int Price;
    public int Speed;
    public int Jump;
    public int HP;
}
