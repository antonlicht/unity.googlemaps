using UnityEngine;

public class RenderCell : MonoBehaviour, ICell<RefCountedSprite>
{
    public RefCountedSprite Content;
    private SpriteRenderer _renderer;
    private static Sprite _defaultSprite;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (!_renderer)
            _renderer = gameObject.AddComponent<SpriteRenderer>();
        if (_defaultSprite == null)
            _defaultSprite = (Sprite)Resources.Load("Materials/DefaultTile", typeof(Sprite));
        _renderer.sprite = _defaultSprite;
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
            _renderer.sprite = _defaultSprite;
            return;
        }
        Content.SpriteChanged += OnSpriteLoaded;
        _renderer.sprite = Content.Sprite;
        if (_renderer.sprite == null)
            _renderer.sprite = _defaultSprite;
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
