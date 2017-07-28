using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle_2D : Paddle
{
    public enum MoveDir { /*Forward, Back,*/ Left, Right, Count }
    
    void Awake() {
        m_bInput = new bool[(int)MoveDir.Count];
        for (int i = 0; i < m_bInput.Length; i++)
            m_bInput[i] = false;
    }

    void Update () {
        float fSpeed = m_fSpeed * Time.deltaTime;
        
//         if (m_bInput[(int)MoveDir.Forward] || Input.GetKey(KeyCode.W)) {
//             transform.Translate(Vector3.forward * fSpeed);
//             //m_vPos.z += 0.5f;
//         }
//         if (m_bInput[(int)MoveDir.Back] || Input.GetKey(KeyCode.S))
//         {
//             transform.Translate(Vector3.back * fSpeed);
//             //m_vPos.z -= 0.5f;
//         }        
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
        m_vPos = transform.localPosition;
        m_vPos.x = Mathf.Clamp(m_vPos.x, m_fMinPos, m_fMaxPos);
        m_vPos.z = Mathf.Clamp(m_vPos.z, m_fMinPos, m_fMaxPos);
        transform.localPosition = m_vPos;
    }
    
    public override int GetControllSize() {
        return (int)MoveDir.Count;
    }

    public override void Move(int nDir)
    {
        if (nDir >= (int)MoveDir.Count || nDir < 0)
            return;
        m_bInput[nDir] = true;
    }
}
