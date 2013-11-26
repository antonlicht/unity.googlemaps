using UnityEngine;

public class RenderCell : MonoBehaviour, ICell<RefCountedSprite>
{
    public RefCountedSprite Content;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (!_renderer)
            _renderer = gameObject.AddComponent<SpriteRenderer>();
    }

    public RefCountedSprite GetContent()
    {
        return Content;
    }

    public void SetContent(RefCountedSprite content)
    {
        if (content == Content)
            return;
        if (Content != null)
        {
            Content.Release();
            Content.SpriteChanged -= OnSpriteLoaded;
        }
        Content = content;
        if (Content == null)
        {
            _renderer.sprite = null;
            return;
        }
        Content.SpriteChanged += OnSpriteLoaded;
        _renderer.sprite = Content.Sprite;
        Content.AddRef();
    }

    private void OnSpriteLoaded(Sprite sprite)
    {
        _renderer.sprite = Content.Sprite;
    }

    public void Clear()
    {
        //Content = null;
    }
}
