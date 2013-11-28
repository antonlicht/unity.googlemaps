using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour 
{

	void Update ()
	{
	    transform.rotation = Camera.main.transform.rotation;
	}
}
