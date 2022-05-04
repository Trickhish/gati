using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    private int maxspeed;
    private int acceleration;
    private int agility;
    private int resistance;
    private float speed;

    private int mFacingDirection = 1;
    private float mDelayToIdle = 0.0f;
    private bool isGrounded = false;
    private bool isSliding = false;
    private bool isMoving = false;
    private bool isJumping = false;

    private float originalsize;
    private float originaloffset;

    private Sensor groundSensor;
    private Sensor slideSensor;
    
    private Rigidbody2D playerRigidbody;

    private BoxCollider2D collider;
    private Player pl;

    public int Maxspeed
    {
        get => maxspeed;
        set => maxspeed = value;
    }

    public int Acceleration
    {
        get => acceleration;
        set => acceleration = value;
    }
    public int Agility
    {
        get => agility;
        set => agility = value;
    }
    public int Resistance
    {
        get => resistance;
        set => resistance = value;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        pl = GetComponent<Player>();
        speed = 6;
        playerRigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        originalsize = collider.size.y;
        originaloffset = collider.offset.y;

        groundSensor = transform.Find("GroundSensor").GetComponent<Sensor>();
        slideSensor = transform.Find("SlideSensor").GetComponent<Sensor>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            GetComponent<Animator>().SetTrigger("Capacity");
        }

        if ((!isSliding && isGrounded) || isJumping)
            MovePlayer();
        if (!isGrounded && groundSensor.State())
        {
            isGrounded = true;
            isJumping = false;
            collider.size = new Vector2(collider.size.x, originalsize);
            collider.offset = new Vector2(collider.offset.x, originaloffset);
            GetComponent<Animator>().SetBool("Idle", isGrounded);
        }

        //Check if character just started falling
        if (isGrounded && !groundSensor.State())
        {
            isGrounded = false;
            GetComponent<Animator>().SetBool("Idle", isGrounded);
        }
        GetComponent<Animator>().SetFloat("AirSpeedY", playerRigidbody.velocity.y);
        if (((Input.GetKey("left shift")|| Input.GetKey("down")) && isGrounded) || slideSensor.State())
        {
            isSliding = true;
            GetComponent<Animator>().SetTrigger("Slide");
            Slide();
        } else
        {
            isSliding = false;
            collider.size = new Vector2(collider.size.x, originalsize);
            collider.offset = new Vector2(collider.offset.x, originaloffset);
        }
        if (Input.GetButtonDown("Jump") && groundSensor.State())
        {
            if (groundSensor.touchTag == "Obstacle")
                Roll();
            else Jump();
        }

        if (isMoving)
        {
            pl.updatepos();

            float t = Vector3.Distance(this.transform.position, GameLogic.Singleton.endpos);

            UIManager.Singleton.pgr_slider.GetComponent<Slider>().value = 100 - (t / GameLogic.Singleton.maxpos) * 100f;
        }
    }

    private void Jump()
    {
        GetComponent<Animator>().SetTrigger("Jump");
        isGrounded = false;
        isJumping = true;
        GetComponent<Animator>().SetBool("Idle", isGrounded);
        playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 16);
        groundSensor.Disable(0.2f);
        
    }
    
    private void Roll()
    {
        GetComponent<Animator>().SetTrigger("Roll");
        isGrounded = false;
        playerRigidbody.velocity = new Vector2(mFacingDirection * speed + (agility * 2), 11);
        groundSensor.Disable(0.7f);
        collider.size = new Vector2(collider.size.x, 0.6f);
        collider.offset = new Vector2(collider.offset.x, 0.1f);
    }

    private void Slide()
    {
        if (speed > 0)
            speed -= 10 * Time.deltaTime;
        else
            speed = 0;
        playerRigidbody.velocity = new Vector2(mFacingDirection * (speed + 1), playerRigidbody.velocity.y);
        collider.size = new Vector2(collider.size.x, 0.6f);
        collider.offset = new Vector2(collider.offset.x, -0.3f);
    }
    
    private void MovePlayer()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            mFacingDirection = 1;
            isMoving = true;
        }else if (horizontalInput < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            mFacingDirection = -1;
            isMoving = true;
        } else
        {
            isMoving = false;
        }
        if (horizontalInput == 0 && speed > 6) speed = 6;
        else
        {
            if (speed < maxspeed)
                speed += acceleration * Time.deltaTime;
            else
                speed = maxspeed;
            playerRigidbody.velocity = new Vector2(horizontalInput * speed, playerRigidbody.velocity.y);
        }
        if (Mathf.Abs(horizontalInput) > Mathf.Epsilon)
        {
            // Reset timer
            mDelayToIdle = 0.05f;
            GetComponent<Animator>().SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            mDelayToIdle -= Time.deltaTime;
            if(mDelayToIdle < 0)
                GetComponent<Animator>().SetInteger("AnimState", 0);
        }
    }
}
