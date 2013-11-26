using UnityEngine;

public class CompassTest : MonoBehaviour
{

    public float Latitude;
    public float Longitude;
    public MapGrid Grid;

    public const float moveSpeed = 0.003f;

    void Start()
    {
        Latitude = 52.50451f;
        Longitude = 13.39699f;
        //Input.location.Start();
        //Input.compass.enabled = true;

    }

    void Update()
    {
        //if (Input.location.status != LocationServiceStatus.Running)
        //    return;
        //SetLocation();
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            Longitude -= touch.deltaPosition.x * moveSpeed * moveSpeed;
            Latitude -= touch.deltaPosition.y * moveSpeed * moveSpeed;
        }
        Grid.CurrentPosition = MapUtils.GeographicToProjection(new Vector2(Longitude, Latitude), 18);
    }


    private void SetLocation()
    {
        Longitude = Input.location.lastData.longitude;
        Latitude = Input.location.lastData.latitude;
    }

}
