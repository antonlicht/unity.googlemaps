using System;
using UnityEngine;

public class S2Test : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
        Vector2 v = new Vector2(13.39699f, 52.50451f);
		ulong cID = S2.PosToCellID(v);
		int lvl = S2.CellIDLevel(cID);
		ulong cID2 = S2.CellIDParent(cID, 16);


		Debug.Log(string.Format("CID: {0} lvl: {1} CID2: {2}", Convert.ToString((long) cID, 16),lvl, Convert.ToString((long)cID2, 16)));

        Debug.Log(S2.PosToE6(S2.CellIDToPos(cID)));
        Debug.Log(S2.PosToE6(S2.CellIDToPos(cID2)));

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
