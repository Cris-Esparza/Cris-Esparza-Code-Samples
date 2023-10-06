using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField]
    private float jumpForce = 50f;

    Vector2 upDirection;
    Vector2 downDirection;
    // Start is called before the first frame update
    void Start()
    {
        // gets the direction for the jump from the orentation of the pad
        upDirection = transform.TransformDirection(Vector2.up * jumpForce);
        downDirection = transform.TransformDirection(Vector2.down * jumpForce);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // checks the tag of the collision object
        if(collision.gameObject.CompareTag("Penguin"))
        {
            if (collision.gameObject.GetComponent<Rigidbody2D>().velocity.y < 0)
            {
                // applies the force in the upward direction to the penguin
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(upDirection, ForceMode2D.Impulse);
            }
            else if(collision.gameObject.GetComponent<Rigidbody2D>().velocity.y > 0)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(downDirection, ForceMode2D.Impulse);
            }
            // play bouncepad sound
            AudioManager.Instance.Play(Sounds.PlayerBounce);
            AudioManager.Instance.Play(Sounds.SquawkBounce);
		}
    }
}
