using UnityEngine;

public class CompassTest : MonoBehaviour
{
    public float Latitude;
    public float Longitude;
    public MapGrid Grid;
    public GameObject CameraRig;

    public const float MoveSpeed = 1f;
    public const float MoveRadius = 1f;


    void Awake()
    {
        Latitude = 52.50451f;
        Longitude = 13.39699f;

        Input.location.Start();
        Input.compass.enabled = true;

    }

    void Update()
    {
        //Rotation:
        Vector3 cameraRot = CameraRig.transform.eulerAngles;
        CameraRig.transform.eulerAngles = new Vector3(cameraRot.x, TouchInput.Singleton.GetRotation(cameraRot.y, CameraRig.transform.position, true), cameraRot.z);

        //Activate Location Service:
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
			Debug.Log("Test"+Input.GetAxis("Mouse X")+" "+Input.GetAxis("Mouse Y"));
			Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); 
			Longitude -= delta.x*0.0001f;
			Latitude -= delta.y*0.0001f;
        }
#else

        if (Input.location.status != LocationServiceStatus.Running) return;
        SetLocation();
#endif

        //New Position:
        MapUtils.ProjectedPos newPosition = MapUtils.GeographicToProjection(new Vector2(Longitude, Latitude), Grid.ZoomLevel);
        if ((newPosition - Grid.CurrentPosition).Magnitude < MoveRadius)
            Grid.CurrentPosition = MapUtils.ProjectedPos.Lerp(Grid.CurrentPosition, newPosition,
                                                              Time.deltaTime * MoveSpeed);
        else
            Grid.CurrentPosition = newPosition;
    }

    private float GetRotation()
    {
        Debug.Log(Input.touchCount);
        if (Input.touchCount > 1)
        {

            Touch touch1 = Input.touches[0];
            Touch touch2 = Input.touches[1];

            if (touch1.deltaTime < 0.00001f)
                return 0f;
            Vector2 t1Start = touch1.position - touch1.deltaPosition;
            Vector2 t1End = touch1.position;
            Vector2 t2Start = touch2.position - touch2.deltaPosition;
            Vector2 t2End = touch2.position;
            float sign = ((t2Start - t1Start).y / (t2Start - t1Start).x > (t2End - t1End).y / (t2End - t1End).x) ? -1f : 1f;
            return sign * Vector2.Angle(t2Start - t1Start, t2End - t1End) * Time.deltaTime / touch1.deltaTime;
        }
        return 0;
    }


    private void SetLocation()
    {
        Longitude = Input.location.lastData.longitude;
        Latitude = Input.location.lastData.latitude;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            Debug.Log("Pause App.");
            Input.location.Stop();
            Input.compass.enabled = false;
        }
        else
        {
            Debug.Log("Resume App.");
            Input.location.Start();
            Input.compass.enabled = true;
        }

    }

}
