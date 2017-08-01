using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Paddle : MonoBehaviour {

    protected Vector3 m_vPos;
    public float m_fSpeed = 20;
    protected float m_fMinPos = -4;
    protected float m_fMaxPos = 4;
    protected bool[] m_bInput;

    public abstract int GetControllSize();
    public abstract void Move(int nDir);    
}
