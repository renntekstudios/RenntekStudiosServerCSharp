using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RennTekNetworking;
using System;

public class RTNetworkManager : MonoBehaviour {

	// Use this for initialization
	void Start () {	
		RTNetwork.Connect ("127.0.0.1", 1024);
	}

    void OnApplicationQuit()
    {
        RTNetwork.Disconnect();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
