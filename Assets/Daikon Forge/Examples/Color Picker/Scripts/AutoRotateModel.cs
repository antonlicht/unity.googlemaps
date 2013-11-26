using UnityEngine;
using System.Collections;

public class AutoRotateModel : MonoBehaviour
{

	void Update()
	{
		transform.Rotate( Vector3.up * Time.deltaTime * 45 );
	}

}
