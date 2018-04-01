using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCenterDemo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DatabaseHelper.GetInstance().InitDatabase();
        DatabaseHelper.GetInstance().CloseConnection();
	}
}
