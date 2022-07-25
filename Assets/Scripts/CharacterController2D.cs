using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 350f;                          // Amount of force added when the player jumps.
    [SerializeField] private float m_SlideForce = 350f;                         // Amount of force added when the player slides.
    [SerializeField] private float m_maxSlideSpeed = 10f;                       // Maximum speed while sliding.
    [SerializeField] private float m_ArrowForce = 10f;                          // Speed of arrows.
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
    [SerializeField] private GameObject Arrow;
    [SerializeField] private GameObject Bow;
    [SerializeField] private GameObject Dagger;
    [SerializeField] private Transform ShotPoint;
    [SerializeField] private float iFrameTime = 1.45f;

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private float temp_slideforce;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private bool m_DoubleJumpAvailable = true;
    public bool m_BookCollected = false;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;
    private bool canReset = true;
    private bool canMove = true;
    private bool canShoot = true;
    private bool canHit = true;
    private float CrouchTimer = 0;
    public float MaxSlideTime = 2f;
    private WaitForSeconds resetdelay = new WaitForSeconds(0.5f);
    private WaitForSeconds DaggerDelay = new WaitForSeconds(0.8f);

    [Header("Player Statistics")]
    [Space]

    public int m_Health = 10;
    public bool m_Alive = true;
    public bool m_BowActive = true;
    public bool m_DaggerActive = false;
    public bool m_IsInvincible = false;

    

    WaitForSeconds ShootDelay = new WaitForSeconds(0.7f);

    private void Start()
    {
        AkSoundEngine.SetState("Earth_or_Hell", "Earth");
    }

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        temp_slideforce = m_SlideForce;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        Dagger.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void Update()
    {
        Vector2 bowPos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - bowPos;
        Bow.transform.right = direction;
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }


    public void Move(float move, bool crouch, bool jump, bool attack)
    {
        //Debug.Log(m_Rigidbody2D.velocity);
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                canReset = false;
                crouch = true;
                StartCoroutine("ResetDelay");
            }
            else
                canReset = true;
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {

            // If crouching
            if (crouch && m_DaggerActive)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                if (canMove)
                    move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            if (canMove)
            {
                // Move the character by finding the target velocity
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
                // And then smoothing it out and applying it to the character
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
            }

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }

            if (attack)
            {
                if (m_DaggerActive)
                {
                    DaggerHit();
                    //Daggerangriff
                }
                else if (m_BowActive)
                {
                    Shoot();
                    //Bogenangriff
                }
            }
        }

        if (m_Grounded && !jump)              // Reset Double Jump when Grounded
        {
            m_DoubleJumpAvailable = true;
        }
        else if (m_Grounded && jump)         // Jump from Ground
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_DoubleJumpAvailable = true;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
        else if (!m_Grounded && jump)       // Jump Mid-Air
        {
            if (m_DoubleJumpAvailable && m_BowActive)
            {
                m_Grounded = false;
                m_Rigidbody2D.velocity = new Vector2(0f, 0f); // to prevent Physics Exploits
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                m_DoubleJumpAvailable = false;
            }
        }
    }


    private void Flip()
    {
        if (canMove)
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    public void CrouchSlide()
    {
        //CrouchTimer += Time.deltaTime;
        //if (canMove)
        //{
        //    m_Rigidbody2D.velocity = Vector2.zero;
        //    canMove = false;
        //}

        //if (CrouchTimer <= MaxSlideTime)
        //{
        //    if (m_FacingRight)
        //        m_Rigidbody2D.AddForce(new Vector2(m_SlideForce * 0.5f, 0f));
        //    else
        //        m_Rigidbody2D.AddForce(new Vector2(-m_SlideForce * 0.5f, 0f));
        //}

        //    CrouchReset();



        if (m_DaggerActive)
        {
            if (m_SlideForce >= 10 && m_Rigidbody2D.velocity != Vector2.zero)
            {
                if (canMove)
                    m_Rigidbody2D.velocity = Vector2.zero;
                canMove = false;
                if (m_FacingRight)                                                  //Let character slide in direction he is facing
                    m_Rigidbody2D.AddForce(new Vector2(m_SlideForce, 0f));
                else
                    m_Rigidbody2D.AddForce(new Vector2(-m_SlideForce, 0f));
                m_SlideForce -= m_SlideForce / 15;                                      //Reduce slideforce every tick
                if (m_Rigidbody2D.velocity.x >= m_maxSlideSpeed)
                    m_Rigidbody2D.velocity = new Vector2(m_maxSlideSpeed, m_Rigidbody2D.velocity.y);
                else if (m_Rigidbody2D.velocity.x <= -m_maxSlideSpeed)
                    m_Rigidbody2D.velocity = new Vector2(-m_maxSlideSpeed, m_Rigidbody2D.velocity.y);
            }
            else
                canMove = true;
        }
    }

    public void CrouchReset()
    {
        canMove = true;
        if (canReset)
            m_SlideForce = temp_slideforce;
    }

    IEnumerator ResetDelay()
    {
        bool canreset_temp = canReset;
        yield return resetdelay;
        if (canReset != canreset_temp)                                             //Reset slideforce after player comes up from beneath a wall
            m_SlideForce = temp_slideforce;
    }

    public void TakeDamage()
    {
        this.m_Health--;
    }

    public void TransitionToEarth()
    {
        m_BowActive = true;
        m_DaggerActive = false;
        AkSoundEngine.SetState("Earth_or_Hell", "Earth");
    }

    public void TransitionToHell()
    {
        m_BowActive = false;
        m_DaggerActive = true;

        AkSoundEngine.SetState("Earth_or_Hell", "Hell");
    }

    private void Shoot()
    {
        if (canShoot)
        {
            StartCoroutine("ShotDelay");
            GameObject newArrow = Instantiate(Arrow, ShotPoint.transform.position, ShotPoint.rotation);
            newArrow.GetComponent<Rigidbody2D>().velocity = Bow.transform.right * m_ArrowForce;
        }
    }

    private void DaggerHit()
    {
        if (canHit)
        {
            Dagger.GetComponent<Animator>().Play("Hit 0");
            StartCoroutine("HitDelay");
        }
    }

    IEnumerator ShotDelay()
    {
        if (canShoot)
            canShoot = false;
        yield return ShootDelay;
        canShoot = true;
    }

    IEnumerator HitDelay()
    {
        if (canHit)
            canHit = false;
        Dagger.GetComponent<BoxCollider2D>().enabled = true;
        yield return DaggerDelay;
        Dagger.GetComponent<BoxCollider2D>().enabled = false;
        canHit = true;
    }

    private IEnumerator IFrame()
    {
        Debug.Log("iFrame goes brrr");
        m_IsInvincible = true;
        yield return new WaitForSeconds(iFrameTime);
        m_IsInvincible = false;
        Debug.Log("iFrame stopped brrr");
    }

    public void LoseHealth(int amount)
    {
        if(m_IsInvincible == true)
        {
            return;
        }
        else
        {
            m_Health -= amount;
        
            if (m_Health <= 0)
            {
                //Dead Trigger
                Debug.Log("Tod");
                return;
            }
            else
            {
                StartCoroutine(IFrame());
            }
            
            }
        
      
    }
}