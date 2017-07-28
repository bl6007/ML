using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetworkDLL;
using System;

public class AI_3DBall : AI {

    protected override float[] UpdateStateBuffer() {
        m_faStateBuffer[0] = m_scriptBall.transform.localPosition.x;
        m_faStateBuffer[1] = m_scriptBall.transform.localPosition.y;
        m_faStateBuffer[2] = m_scriptBall.transform.localPosition.z;
        m_faStateBuffer[3] = m_scriptBall.m_rigid.velocity.x;
        m_faStateBuffer[4] = m_scriptBall.m_rigid.velocity.y;
        m_faStateBuffer[5] = m_scriptBall.m_rigid.velocity.z;
        m_faStateBuffer[6] = m_scriptPaddle.transform.localPosition.x;
        m_faStateBuffer[7] = m_scriptPaddle.transform.localPosition.z;
        return m_faStateBuffer;
    }
}

