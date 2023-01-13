using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LoadSprite : MonoBehaviour
{

    Image image;

    IEnumerator Start()
    {
        const string kSpritePath = "Assets/AddrFiles/Sprites/Icons.spriteatlasv2";
        yield return LoadUtil.Load<SpriteAtlas>(kSpritePath);

        var atlas = LoadUtil.GetPrototype<SpriteAtlas>(kSpritePath);

        Debug.Log(atlas.name);
        Debug.Log(atlas.spriteCount);
        Debug.Log(atlas.GetSprite("Weapon_AnlhNMtL").name);

        image = GetComponent<Image>();
        image.sprite = atlas.GetSprite("Weapon_AnlhNMtL");
    }
}
