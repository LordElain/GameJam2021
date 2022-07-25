using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float m_CameraFollowSpeed = 1.0f;
    public Transform m_PlayerPosition;
    private Transform m_CameraPosition;

    // Start is called before the first frame update
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(new Vector3((m_PlayerPosition.position.x - this.transform.position.x) * m_CameraFollowSpeed * Time.deltaTime, (m_PlayerPosition.position.y - this.transform.position.y) * m_CameraFollowSpeed * Time.deltaTime, 0));
    }
}
