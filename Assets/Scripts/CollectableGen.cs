using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableGen : MonoBehaviour {

    public Transform collectable;
    public Transform parent;
    public GameObject collectableHolder;
    public int count = 1 ;
    // Use this for initialization
    void Start () {
		
	}
	
    public void Awake()
    {
        for (int i = collectableHolder.transform.childCount-1; i >=0 ; i--)
        {
            var objectToRemove = collectableHolder.transform.GetChild(i).gameObject;
            Destroy(objectToRemove);
        }
        int width = 800;
        int height = 400;        
        int placeX = 0, placeY = 0;
        System.Random pseudoRandom = new System.Random();
        for (int i = 0; i < count; i++)
        {
            placeX = pseudoRandom.Next(0, width);
            placeY = pseudoRandom.Next(0, height);
            Instantiate(collectable, new Vector3(-width / 2 + .5f + placeX, -height / 2 + .5f + placeY, 1), Quaternion.identity, parent);            
        }       

    }

	// Update is called once per frame
	void Update () {
		
	}
}
