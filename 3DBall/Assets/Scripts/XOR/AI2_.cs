using NeuralNetworkDLL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI2_ : MonoBehaviour
{
    NeuralNetwork m_NN;
	// Use this for initialization
	void Start () {
        m_NN = new NeuralNetwork(1, 1, 4, 1, 0.0001, 0.1);
        StartCoroutine(PlayAI());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator PlayAI()
    {
        int nDataSize = 100;
        double[][] x = new double[nDataSize][];
        for (int i = 0; i < nDataSize; i++)
        {
            x[i] = new double[1] { i + 1 };
        }
        double[][] y_target = new double[nDataSize][];
        for (int i = nDataSize - 1; i >= 0; i--)
        {
            y_target[i] = new double[1] { 100 - i };
        }
        double[] y_temp = new double[1];
        

        for (int i = 0; i < nDataSize; i++)
        {
            m_NN.SetInput(x[i]);
            m_NN.FeedForward();
            m_NN.copyOutputVectorTo(ref y_temp);            

            Debug.Log("Pre Input: " + x[i][0].ToString());
            Debug.Log("\t\tPre Target: " + y_target[i][0].ToString());
            Debug.Log("\t\tResult: " + y_temp[0]);
        }

//        float fTime = 0;
        for (int j = 0; j < 10000; j++)
        {
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < nDataSize; i++)
            {
                m_NN.SetInput(x[i]);
                m_NN.FeedForward();
                m_NN.copyOutputVectorTo(ref y_temp);
                m_NN.PropBackward(y_target[i]);
            }

//             stopWatch.Stop();
//             fTime += stopWatch.ElapsedMilliseconds;
        }

        for (int i = 0; i < nDataSize; i++)
        {
            //                 double[] d = new double[1] {i*2};                
            //                 nn_.SetInput(d);
            m_NN.SetInput(x[i]);
            m_NN.FeedForward();
            m_NN.copyOutputVectorTo(ref y_temp);

            //Debug.Log("Input: " + d[0].ToString());
            Debug.Log("Input: " + x[i][0].ToString());
            Debug.Log(" Target: " + y_target[i][0].ToString());
            Debug.Log(" Result: " + y_temp[0]);
        }

        //MessageBox.Show(fTime / 10 + "ms");            
    }
}
