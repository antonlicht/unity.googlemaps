using System;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    private const float MinRotationSpeed = 1f;
    private const float RotationDampTime = 4f;
    private const float RotationSpeedInterpolation = 8f;

    private static TouchInput _instance;
    private Vector2 _startInput;
    private float _startRotation;
    private float _lastRotation;
    [SerializeField]
    private float _rotationSpeed = 0;
    private float _startScale;
    [SerializeField]
    private int _touch1 = -1;
    [SerializeField]
    private int _touch2 = -1;
    [SerializeField]
    private int _singleTouch = -1;

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

    public float GetRotation(float startAngle,Vector3 worldPos = default(Vector3), bool singleTouch = false)
    {
        if (Input.touchCount < 1 || (Input.touchCount == 1 && !singleTouch))
        {
            _touch1 = -1;
            _touch2 = -1;
            _singleTouch = -1;

            if (_rotationSpeed > MinRotationSpeed || _rotationSpeed < -MinRotationSpeed)
            {
                startAngle += _rotationSpeed*Time.deltaTime;
                _lastRotation = startAngle;
                _rotationSpeed = Mathf.Lerp(_rotationSpeed, 0f, Time.deltaTime*RotationDampTime);
            }
            
            return startAngle;
        }

        Vector2 endInput = new Vector2();

        if (Input.touchCount == 1)
        {
            _touch1 = -1;
            _touch2 = -1;

            Touch t = Input.touches[0];
            
            if (t.fingerId != _singleTouch)
            {
                _startInput = (Vector2)Camera.main.WorldToScreenPoint(worldPos) - t.position;
                _startRotation = startAngle;
                _singleTouch = t.fingerId;
            }
            endInput = (Vector2)Camera.main.WorldToScreenPoint(worldPos) - t.position;
        }
        else
        {
            _singleTouch = -1;

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
                t1 = Input.touches[0];
                t2 = Input.touches[1];
                _startInput = t2.position - t1.position;
                _startRotation = startAngle;
                _touch1 = t1.fingerId;
                _touch2 = t2.fingerId;
            }
            endInput = t2.position - t1.position;
        }

        if (endInput != _startInput)
        {
            float a1 = Mathf.Atan2(_startInput.normalized.x, _startInput.normalized.y);
            float a2 = Mathf.Atan2(endInput.normalized.x, endInput.normalized.y);
            float a = a1 - a2;
            float newRotation = _startRotation + Mathf.Rad2Deg*a;
            _rotationSpeed = Mathf.Lerp(_rotationSpeed,(newRotation - _lastRotation)/Time.deltaTime,Time.deltaTime*RotationSpeedInterpolation);
            _lastRotation = newRotation;

            return newRotation;
        }
        return startAngle;
    }

    public float GetZoom(float startScale)
    {
        if (Input.touchCount < 1)
            return startScale;
        return 0f;
    }
}
