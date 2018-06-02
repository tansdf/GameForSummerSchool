using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player2D : MonoBehaviour
{

    public float speed;             //Floating point variable to store the player's movement speed.

    public Text countText;

    public float smoothTime = 0.3F;

    private Rigidbody2D rb2d;       //Store a reference to the Rigidbody2D component required to use 2D Physics.

    private Vector3 velocity = Vector3.zero;

    private int count;
    // Use this for initialization
    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rb2d = GetComponent<Rigidbody2D> ();
        rb2d.freezeRotation = true;
        count = 0;
        SetCountText();
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {
        //Store the current horizontal input in the float moveHorizontal.
      /*  float moveHorizontal = Input.GetAxis ("Horizontal");

        //Store the current vertical input in the float moveVertical.
        float moveVertical = Input.GetAxis ("Vertical");
        
        //Use the two store floats to create a new Vector2 variable movement.
        Vector2 movement = new Vector2 (moveHorizontal, moveVertical).normalized;
        
        rb2d.MovePosition(rb2d.position + movement*Time.fixedDeltaTime*speed);
        */
        Vector2 touchDeltaPosition = Vector2.zero;

     //touch movement
     if (Input.touchCount > 0)
     {
         // Get movement of the finger since last frame
         touchDeltaPosition = Input.GetTouch(0).deltaPosition*speed;
     }
        
     Vector3 actualPosition = GetComponent<Rigidbody2D>().position;
     Vector3 target = new Vector3(actualPosition.x + touchDeltaPosition.x, actualPosition.y + touchDeltaPosition.y, 0.0f);
     GetComponent<Rigidbody2D>().position = Vector3.SmoothDamp(actualPosition, target, ref velocity, smoothTime, speed*3);
        
    }

    public void SetZeroCount()
    {
        count = 0;
        SetCountText();
    }

    void OnTriggerEnter2D(Collider2D other)
    {       
        if (other.gameObject.CompareTag("Collectable"))
        {
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
            count+=50;
            SetCountText();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
    }

}
