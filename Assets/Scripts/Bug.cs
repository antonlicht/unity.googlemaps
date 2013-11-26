using UnityEngine;

public class Bug : MonoBehaviour {


	void Start ()
	{
	    var x = Input.location.lastData;
        Debug.Log(this);
	}

}
