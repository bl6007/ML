using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetworkDLL;
using System;

public class AI : MonoBehaviour {

    Ball m_scriptBall;
    Paddle m_scriptPaddle;

    public bool m_bTraning = false;
    NeuralNetwork m_nn = new NeuralNetwork();    
    int m_nStateCount = 0;
    public int m_nHistoryCount = 2;
    public int m_nHiddenLayerCount = 3;
    public float m_fBias = 1 ;
    public float m_fEta = 1e-4f;
    public float m_fAlpha= 0.9f;
    public float m_fGamma = 0.95f;
    public float m_fTraningRandomValue = 0.4f;
    const int m_nInputSize = 3;
    float[] m_faStateBuffer = new float[m_nInputSize];
    Renderer[] m_AllRenderer;
    Light[] m_Light;
    Camera m_Camera;

    void Awake() {
        m_scriptBall = gameObject.GetComponentInChildren<Ball>();
        m_scriptPaddle = gameObject.GetComponentInChildren<Paddle>();
        m_AllRenderer = gameObject.GetComponentsInChildren<Renderer>(true);
        m_Light = gameObject.GetComponentsInChildren<Light>(true);
        m_Camera = Camera.main;
    }

    void Start() {
        m_nStateCount = m_scriptPaddle.GetControllSize(); 
        int nInputSIze = m_nInputSize * m_nHistoryCount;
        int nOutputSIze = m_nStateCount + 1; // +1 is do nothing
        m_nn.Init(nInputSIze, nOutputSIze, m_nHiddenLayerCount, 1, m_fEta, m_fAlpha);
        SetTrainig(m_bTraning);
        StartCoroutine(PlayAI());
    }

    void Update () {
	   if(Input.GetKeyDown(KeyCode.Space)) {
            ToggleTraning();
        }
	}
    
    void SetTrainig(bool bTraining)
    {
        m_bTraning = bTraining;
        if (m_bTraning)
        {
            Time.timeScale = 7;
            for (int i = 0; i < m_AllRenderer.Length; i++)
                m_AllRenderer[i].enabled = false;
            for (int i = 0; i < m_Light.Length; i++)
                m_Light[i].enabled = false;
            m_Camera.enabled = false;
        }
        else
        {
            Time.timeScale = 1;
            for (int i = 0; i < m_AllRenderer.Length; i++)
                m_AllRenderer[i].enabled = true;
            for (int i = 0; i < m_Light.Length; i++)
                m_Light[i].enabled = true;
            m_Camera.enabled = true;
        }
    }

    void ToggleTraning() {        
        SetTrainig(!m_bTraning);
    }

    int counter = 0;
    int counter_all = 0;
    IEnumerator PlayAI()
    {
        CircularQueue<float[]> History = new CircularQueue<float[]>();
        History.Init(m_nHistoryCount, new float[m_nInputSize]);
        UpdateStateBuffer();
        for(int i=0; i< m_nHistoryCount; i++)
            History.pushBack(m_faStateBuffer);
        double[] daInput = new double[m_nn.m_nInputSize];
        double[] daNext_input = new double[m_nn.m_nInputSize];
        double[] daReward = new double[m_nn.m_nOutputSize];
        
        float[] HistoryData;
        WaitForEndOfFrame endofFrame = new WaitForEndOfFrame();
        while (true) {
            for (int i = 0; i < m_nHistoryCount; i++) {
                History.getValue(i, out HistoryData);
                Array.Copy(HistoryData, 0, daInput, i * HistoryData.Length, HistoryData.Length);
            }
            m_nn.SetInput(daInput);
            m_nn.FeedForward();

            int nSelected_dir = m_bTraning ? m_nn.GetOutputIXEpsilonGreedy(m_fTraningRandomValue) : m_nn.GetOutputIXEpsilonGreedy(0.0);
            m_scriptPaddle.Move((Paddle.MoveDir)nSelected_dir);

            yield return endofFrame;

            float fReward = m_scriptBall.m_fReward;

            if (fReward > 0.5f)
                counter++;
            if (fReward != 0.0f)
                counter_all++;

            //Debug.Log(counter_all);

            if (counter_all == 1000)
            {
                //static double time = timeGetTime();

                //if (m_bTraning == true)
                //{
                    Debug.Log((float)counter / (float)counter_all * 100.0f);
                //}
                    //std::cout << (float)counter / (float)counter_all * 100.0f << " % " << (timeGetTime() - time) / 1000.0f << " sec" << std::endl;

                //time = timeGetTime();

                counter = 0;
                counter_all = 0;
            }
            if (fReward < 0.0f) fReward = 0.0f;

            m_scriptBall.m_fReward = 0;

            m_nn.copyOutputVectorTo(ref daReward);
            UpdateStateBuffer();
            History.pushBack(m_faStateBuffer.Clone() as float[]);

            for (int i = 0; i < m_nHistoryCount; i++)
            {
                History.getValue(i, out HistoryData);
                Array.Copy(HistoryData, 0, daNext_input, i * HistoryData.Length, HistoryData.Length);
            }
            m_nn.SetInput(daNext_input);
            m_nn.FeedForward();

            double? dNext_Q = m_nn.GetMaxValueFromOutput();
            if(dNext_Q.HasValue)
            {
//                  for(int i=0; i< daReward.Length; i++)
//                  {
//                     if (i == nSelected_dir)
                        daReward[nSelected_dir] = fReward + m_fGamma * dNext_Q.Value;
//                  }
            }
            if(m_bTraning)
            {
                if (fReward > 0)
                {
                    m_nn.SetInput(daInput);
                    for (int i = 0; i < 20; i++)
                    {   
                        m_nn.FeedForward();
                        m_nn.PropBackward(daReward);
                    }
                }
            }            
            
//            m_nn.SetInput(daInput);
            m_nn.FeedForward();
            m_nn.PropBackward(daReward);
        }
    }

    void UpdateStateBuffer()
    {
//         m_faStateBuffer[0] = m_scriptBall.transform.position.x;
//         m_faStateBuffer[1] = m_scriptBall.transform.position.y;
//         m_faStateBuffer[2] = m_scriptBall.transform.position.z;
//         m_faStateBuffer[3] = m_scriptBall.m_rigid.velocity.x;
//         m_faStateBuffer[4] = m_scriptBall.m_rigid.velocity.y;
//         m_faStateBuffer[5] = m_scriptBall.m_rigid.velocity.z;
//         m_faStateBuffer[6] = m_scriptPaddle.transform.position.x;
//         m_faStateBuffer[7] = m_scriptPaddle.transform.position.y;
//         m_faStateBuffer[8] = m_scriptPaddle.transform.position.z;
//         Vector3 v = m_scriptPaddle.transform.position - m_scriptBall.transform.position;
//         m_faStateBuffer[9] = v.x;
//         m_faStateBuffer[10] = v.y;
//         m_faStateBuffer[11] = v.z;
        m_faStateBuffer[0] = m_scriptBall.transform.position.x;
        m_faStateBuffer[1] = m_scriptBall.transform.position.y;     
//         m_faStateBuffer[2] = m_scriptBall.m_rigid.velocity.x;
//         m_faStateBuffer[3] = m_scriptBall.m_rigid.velocity.y;        
        m_faStateBuffer[2] = m_scriptPaddle.transform.position.x;
//         Vector3 v = m_scriptPaddle.transform.position - m_scriptBall.transform.position;
//         m_faStateBuffer[9] = v.x;
//         m_faStateBuffer[10] = v.y;
//         m_faStateBuffer[11] = v.z;
    }
}

