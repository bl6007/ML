using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour {

    Vector3 m_vPos;
    float m_fSpeed = 10;
    float m_fMinPos = -4;
    float m_fMaxPos = 4;
    public enum MoveDir { Forward, Back, Left, Right, Count }
    bool[] m_bInput;
    

    void Start() {
        m_bInput = new bool[(int)MoveDir.Count];
        for (int i = 0; i < m_bInput.Length; i++)
            m_bInput[i] = false;
    }

    void Update () {
        float fSpeed = m_fSpeed * Time.deltaTime;
        
        if (m_bInput[(int)MoveDir.Forward] || Input.GetKey(KeyCode.W)) {
            transform.Translate(Vector3.forward * fSpeed);
            //m_vPos.z += 0.5f;
        }
        if (m_bInput[(int)MoveDir.Back] || Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * fSpeed);
            //m_vPos.z -= 0.5f;
        }        
        if (m_bInput[(int)MoveDir.Left] || Input.GetKey(KeyCode.A)) {
            transform.Translate(Vector3.left * fSpeed);
            //m_vPos.x -= 0.5f;
        }
        if (m_bInput[(int)MoveDir.Right] || Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * fSpeed);
            //m_vPos.x += 0.5f;
        }

        for (int i = 0; i < m_bInput.Length; i++)
            m_bInput[i] = false;
        m_vPos = transform.position;
        m_vPos.x = Mathf.Clamp(m_vPos.x, m_fMinPos, m_fMaxPos);
        m_vPos.z = Mathf.Clamp(m_vPos.z, m_fMinPos, m_fMaxPos);
        transform.position = m_vPos;
    }
    
    public int GetControllSize() {
        return (int)MoveDir.Count;
    }

    //public void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("Ball"))
    //        m_fReward = 1;
    //}

    public void Move(MoveDir Dir){
        if (Dir >= MoveDir.Count || Dir < 0)
            return;
        m_bInput[(int)Dir] = true;
    }
}
