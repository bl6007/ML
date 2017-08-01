using NeuralNetworkDLL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    protected Ball m_scriptBall;
    protected Paddle m_scriptPaddle;
    protected Game m_Game;

    public bool m_bTraning = false;
    NeuralNetwork m_nn = new NeuralNetwork();
    int m_nStateCount = 0;
    public int m_nHistoryCount = 2;
    public int m_nHiddenLayerCount = 3;
    public float m_fBias = 1;
    public float m_fEta = 1e-4f;
    public float m_fAlpha = 0.9f;
    public float m_fGamma = 0.95f;
    public float m_fTraningRandomValue = 0.4f;
    public int m_nInputSize = 5;
    protected float[] m_faStateBuffer;
    public List<float> m_lSuccessRateHistory = new List<float>();
    public float m_fSuccessRateMax = 0;
    public float m_fSuccessRateAverage = 0;

    void Awake()
    {
        m_faStateBuffer = new float[m_nInputSize];
        m_Game = gameObject.GetComponentInParent<Game>();
        m_scriptBall = gameObject.GetComponentInChildren<Ball>();
        m_scriptPaddle = gameObject.GetComponentInChildren<Paddle>();
    }

    void Start()
    {
        m_nStateCount = m_scriptPaddle.GetControllSize();
        int nInputSIze = m_nInputSize * m_nHistoryCount;
        int nOutputSIze = m_nStateCount + 1; // +1 is do nothing
        m_nn.Init(nInputSIze, nOutputSIze, m_nHiddenLayerCount, m_fBias, m_fEta, m_fAlpha);
        for (int i = 0; i < m_nn.m_nLayerSize ; i++)
        {
            m_nn.SetLayerType(i, LayerBase.ActType.ReLU);
        }
        
        StartCoroutine(PlayAI());
    }

    int counter = 0;
    int counter_all = 0;
    IEnumerator PlayAI()
    {
        //int FrameCount = 0;
        CircularQueue<float[]> History = new CircularQueue<float[]>();
        History.Init(m_nHistoryCount, new float[m_nInputSize]);
        UpdateStateBuffer();
        for (int i = 0; i < m_nHistoryCount; i++)
            History.pushBack(m_faStateBuffer);
        int nInputSize = m_nn.m_nInputSize;
        double[] daInput = new double[nInputSize];
        double[] daNext_input = new double[nInputSize];
        double[] daReward = new double[m_nn.m_nOutputSize];

        float[] HistoryData;
        WaitForEndOfFrame endofFrame = new WaitForEndOfFrame();
        while (true)
        {
            for (int i = 0; i < m_nHistoryCount; i++)
            {
                History.getValue(i, out HistoryData);
                Array.Copy(HistoryData, 0, daInput, i * HistoryData.Length, HistoryData.Length);
            }
            m_nn.SetInput(daInput);
            m_nn.FeedForward();

            int nSelected_dir = m_Game.m_bTraning ? m_nn.GetOutputIXEpsilonGreedy(m_fTraningRandomValue) : m_nn.GetOutputIXEpsilonGreedy(0.0);
            m_scriptPaddle.Move(nSelected_dir);

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
                float f = (float)counter / (float)counter_all * 100.0f;
                m_lSuccessRateHistory.Add(f);
                if (m_lSuccessRateHistory.Count > 10)
                    m_lSuccessRateHistory.RemoveAt(0);
                if (m_fSuccessRateMax < f)
                    m_fSuccessRateMax = f;
                m_fSuccessRateAverage = GetAverage();
                Debug.Log(gameObject.name + m_fSuccessRateAverage);
                //}
                //std::cout << (float)counter / (float)counter_all * 100.0f << " % " << (timeGetTime() - time) / 1000.0f << " sec" << std::endl;

                //time = timeGetTime();

                counter = 0;
                counter_all = 0;
            }
            //Debug.Log("FrameCount: " + ++FrameCount + " Reward: " + fReward);
            //if (fReward < 0.0f)
            //{                
            //    fReward = 0.0f;
            //}
            m_scriptBall.m_fReward = 0;

            m_nn.copyOutputVectorTo(ref daReward);
            History.pushBack(UpdateStateBuffer().Clone() as float[]);

            for (int i = 0; i < m_nHistoryCount; i++)
            {
                History.getValue(i, out HistoryData);
                Array.Copy(HistoryData, 0, daNext_input, i * HistoryData.Length, HistoryData.Length);
            }
            m_nn.SetInput(daNext_input);
            m_nn.FeedForward();

            double? dNext_Q = m_nn.GetMaxValueFromOutput();
            if (dNext_Q.HasValue)
                daReward[nSelected_dir] = fReward + (m_fGamma * dNext_Q.Value);

            m_nn.SetInput(daInput);
            m_nn.FeedForward();
            m_nn.PropBackward(daReward);
            m_nn.check();
        }
    }

    float GetAverage()
    {
        float f = 0;
        int nSize = m_lSuccessRateHistory.Count;
        for (int i = 0; i < nSize; i++)
            f += m_lSuccessRateHistory[i];
        return f / (float)m_lSuccessRateHistory.Count;
    }

    protected virtual float[] UpdateStateBuffer()
    {
        return m_faStateBuffer;
    }
}
