using System;
using UnityEngine;

public class TouchInput : MonoBehaviour 
{
    private static TouchInput _instance;
    private Vector2 _startInput;
    private float _startRotation;
    [SerializeField]
    private int _touch1 = -1;
    [SerializeField]
    private int _touch2 = -1;
    private float _startScale;

    public float RotationSpeed;
    public float ZoomSpeed;

    public static TouchInput Singleton
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("TouchInput");
                _instance = obj.AddComponent<TouchInput>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Debug.LogError("Second instance of TouchInput.");
    }

    public float GetRotation(float startAngle)
    {
        if (Input.touchCount <= 1)
        {
            _touch1 = -1;
            _touch2 = -1;
            //Debug.Log("Keine Rotation");
            return startAngle;
        }

        Touch t1 = new Touch();
        Touch t2 = new Touch();
        bool b1 = false;
        bool b2 = false;
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == _touch1)
            {
                b1 = true;
                t1 = touch;
            }
            if (touch.fingerId == _touch2)
            {
                b2 = true;
                t2 = touch;
            }
        }

        if (!b1 || !b2)
        {
            _touch1 = -1;
            _touch2 = -1;
            //Debug.Log("Starte Rotation!");
            t1 = Input.touches[0];
            t2 = Input.touches[1];
            _startInput = t2.position - t1.position;
            _startRotation = startAngle;
            _touch1 = t1.fingerId;
            _touch2 = t2.fingerId;
        }
        //Debug.Log(t1.position + " " + t2.position);
        Vector2 endInput = t2.position - t1.position;
        // sign = (_startInput.y / _startInput.x > endInput.y / endInput.x) ? -1f : 1f;
        //float sign = (_startInput.y / _startInput.x > endInput.y / endInput.x) ? -1f : 1f;
        //return (_startRotation + sign * Vector2.Angle(_startInput, endInput)+360f)%360f;

        float a1 = Mathf.Atan2(_startInput.normalized.x, _startInput.normalized.y);
        float a2 = Mathf.Atan2(endInput.normalized.x, endInput.normalized.y);
        //if (a1 < 0) {a1 += Mathf.PI*2;}
        //if (a2 < 0) {a2 += Mathf.PI*2;}
        float a = a1 - a2;
        //Debug.Log(">>>>"+a1+" "+a2+" "+a);

        return _startRotation+Mathf.Rad2Deg*a;
    }

    public float GetZoom(float startScale)
    {
        if (Input.touchCount < 1)
            return startScale;
        return 0f;
    }
}
