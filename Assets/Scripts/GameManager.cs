using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public CameraMovement MainCam;
    public Transform[] hellPos;
    public Transform[] normalPos;
    public GameObject CooldownTimer;

    private CharacterController2D PlayerCharacterController;

    private int currentPos = 0;
    private float cooldown = 2;

    private bool inHell = false;
    WaitForSeconds camSpeedDelay = new WaitForSeconds(0.7f);
    void Start()
    {
        PlayerCharacterController = player.GetComponent<CharacterController2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (PlayerCharacterController.m_BookCollected)
            {
                if (hellPos[currentPos] && normalPos[currentPos])                              //Check if both positions exist
                {
                    if (inHell && cooldown <= 0)                                                                //Check if player is in Hell or not, swap to the other place respectively
                    {
                        hellPos[currentPos].position = player.transform.position;
                        player.transform.position = normalPos[currentPos].position;
                        inHell = !inHell;
                        StartCoroutine("CamSpeed");
                        cooldown = 2;
                        player.GetComponent<CharacterController2D>().TransitionToEarth();
                    }
                    else if (!inHell && cooldown <= 0)
                    {
                        normalPos[currentPos].position = player.transform.position;
                        player.transform.position = hellPos[currentPos].position;
                        inHell = !inHell;
                        StartCoroutine("CamSpeed");
                        cooldown = 2;
                        player.GetComponent<CharacterController2D>().TransitionToHell();
                    }
                }
            }
        }

        if(cooldown > 0)
            cooldown -= Time.deltaTime;

        CooldownTimer.GetComponent<Image>().fillAmount = 1 -( cooldown / 2);

    }

    IEnumerator CamSpeed()
    {
        MainCam.m_CameraFollowSpeed = 10.0f;
        yield return camSpeedDelay;
        MainCam.m_CameraFollowSpeed = 1.0f;
    }
}
