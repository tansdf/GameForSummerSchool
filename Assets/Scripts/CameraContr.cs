using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraContr : MonoBehaviour {

    public GameObject player;

    private Vector3 offset;
	
	void Start () {
        transform.position = player.transform.position + new Vector3(0,0,-10);    
        
        offset = transform.position - player.transform.position;
	}
	

	void LateUpdate () {
        transform.position = player.transform.position + offset;
	}
}
