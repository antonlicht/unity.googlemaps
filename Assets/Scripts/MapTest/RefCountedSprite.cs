using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class RefCountedSprite : RefCounted
{

    public Sprite Sprite;
    public event Action<Sprite> SpriteChanged;

    public void SetSprite(Sprite sprite)
    {
        Sprite = sprite;
        if (SpriteChanged != null)
            SpriteChanged(sprite);
    }

    public override void Destroy()
    {
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        if (Sprite != null && Sprite.texture != null)
            Object.DestroyImmediate(Sprite.texture);
        if (Sprite)
            Object.DestroyImmediate(Sprite);
    }
}
