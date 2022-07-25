using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D PlayerController;
    public Animator Animator;

    public float RunSpeed = 40f;

    float HorizontalMovement = 0f;

    bool JumpPressed = false;
    bool CrouchPressed = false;
    bool AttackPressed = false;

    // Update is called once per frame
    void Update()
    {
        HorizontalMovement = Input.GetAxisRaw("Horizontal") * RunSpeed;
        Animator.SetFloat("PlayerSpeed", Mathf.Abs(HorizontalMovement));

        if (Input.GetButtonDown("Jump"))
        {
            JumpPressed = true;
            Animator.SetBool("IsJumping?", true);
        }

        else
        {
            Animator.SetBool("IsJumping?", false);
        }
            

        if (Input.GetButtonDown("Slide"))
        {
            CrouchPressed = true;
            
        }
        else if (Input.GetButtonUp("Slide"))
        {
            CrouchPressed = false;
            PlayerController.CrouchReset();
            Animator.SetBool("IsSliding?", false);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (PlayerController.m_BowActive)
                Animator.SetBool("IsShooting?", true);
            else if (PlayerController.m_DaggerActive)
                Animator.SetTrigger("IsAttacking?");
                AkSoundEngine.PostEvent("Player_Attack_SFX", gameObject);
            AttackPressed = true;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            if (PlayerController.m_BowActive)
                Animator.SetBool("IsShooting?", false);
            else if (PlayerController.m_DaggerActive)
                Animator.ResetTrigger("IsAttacking?");
            AttackPressed = false;
        }
    }

    private void FixedUpdate()
    {
        PlayerController.Move(HorizontalMovement * Time.fixedDeltaTime, CrouchPressed, JumpPressed, AttackPressed);
        if (CrouchPressed && PlayerController.m_DaggerActive)
        {
            PlayerController.CrouchSlide();
            Animator.SetBool("IsSliding?", true);
        }

        else
        {
            Animator.SetBool("IsSliding?",false);
        }
           
        JumpPressed = false;
        //CrouchPressed = false;
    }

}
