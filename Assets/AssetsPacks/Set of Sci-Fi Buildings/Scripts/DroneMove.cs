using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMove : MonoBehaviour {
    public Transform[] targetPointsPos;                 //(enemy AI) Points for positions 
    private byte sel_ltargetPointPos;                   //(enemy AI) selected targetPointPos in array
    private float m_MovementValue;         // The current value of the movement .
    private float m_MovementValueY;         // The current value of the movement Z.
    private float m_Speed = 8.0f;                 // How fast the tank moves forward and back.
    private bool m_MoveUp;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (targetPointsPos.Length > 0)
        {
            var heading = transform.position - targetPointsPos[sel_ltargetPointPos].position;


            //move forward
             heading.y = 0;  // This is the overground heading.
            if (heading.sqrMagnitude > 20)
            { //if the target is far move otherwise stand
                if (m_MovementValue < 1)
                    m_MovementValue += 0.01f;
                //turn towards  
                Vector3 targetDir = targetPointsPos[sel_ltargetPointPos].position - transform.position;
                float step = 5.5f * Time.deltaTime;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
                newDir.y = 0;
                transform.rotation = Quaternion.LookRotation(newDir);

            }
            else if (m_MovementValue > 0.3f)
                m_MovementValue -= 0.01f;
            else
            {
                //The tank got to the target, choose another target position for movement
                m_MovementValue = 0.3f;
                if (targetPointsPos.Length > 1)
                    if (sel_ltargetPointPos < targetPointsPos.Length - 1)
                        sel_ltargetPointPos++;
                    else
                        sel_ltargetPointPos = 0;


            }
            if (m_MoveUp)
                if (m_MovementValueY < 0.05f) m_MovementValueY += 0.001f;
                else m_MoveUp = false;
            if (!m_MoveUp)
                if (m_MovementValueY > -0.05f) m_MovementValueY -= 0.001f;
                else m_MoveUp = true;

        }
    }
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = transform.forward * m_MovementValue * m_Speed * Time.deltaTime;

        // Apply this movement to the object.
        transform.position = new Vector3(transform.position.x+ movement.x, transform.position.y + m_MovementValueY, transform.position.z + movement.z);
  


    }
}
