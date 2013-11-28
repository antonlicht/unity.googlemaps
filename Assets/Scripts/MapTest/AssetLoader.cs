using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{

    private static AssetLoader _singleton;
    private bool _isLoading;
    private Dictionary<string, Action<Sprite>> _queue;
    public int PixelsToUnit = 100;
    public Vector2 Pivot = new Vector2(0f, 1f);


    public static AssetLoader Loader
    {
        get { return _singleton; }
    }

    private void Awake()
    {
        if (!_singleton)
        {
            _singleton = this;
            return;
        }
        Destroy(this);
    }

    private void Update()
    {
        if (_isLoading || _queue == null || !_queue.Any())
            return;
        _isLoading = true;
        StartCoroutine(LoadSprite());

    }

    public int Count
    {
        get { return _queue == null ? 0 : _queue.Count(); }
    }

    public void Enqueue(string url, Action<Sprite> callback = null)
    {
        if (_queue == null)
            _queue = new Dictionary<string, Action<Sprite>>();
        _queue[url] = callback;
    }

    public void Dequeue(string url)
    {
        if (_queue != null)
            _queue.Remove(url);
    }

    private IEnumerator LoadSprite()
    {
        string url = _queue.Keys.First();
        Debug.Log("Load " + url);
        WWW webRequest = new WWW(url);
        yield return webRequest;
        if (!String.IsNullOrEmpty(webRequest.error))
        {
            Debug.LogError(webRequest.error);
            _isLoading = false;
            yield break;
        }
        Texture2D tex = webRequest.texture;
        Sprite loadedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Pivot, PixelsToUnit);
        if (_queue[url] != null)
            _queue[url](loadedSprite);
        Dequeue(url);
        _isLoading = false;
    }

}
