using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NeuralNetworkDLL
{
    public class LayerBase
    {
        public enum ActType { Sigmoid, ReLU, LReLU };
	    ActType act_type_;

	    public double[] m_daAct;
        public double[] m_daGrad;

        public LayerBase(int nSize, ActType _type, double bias = 1.0)
        {
            resize(nSize, bias);
            act_type_ = _type;
        }

#region _ActivationFunctions
        double getLRELU(double x)
        {
            return x > 0.0 ? x : 0.01 * x;
        }
        double getLRELUGradFromY(double x)
        {
            return x > 0.0 ? 1.0 : 0.01;
        }

        double getRELU(double x)
        {
            return Math.Max(0.0, x);
        }
        double getRELUGradFromY(double x)
        {
            return x > 0.0 ? 1.0 : 0.0;
        }

        double getSigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }
        double getSigmoidGradFromY(double y)
        {
            return (1.0 - y) * y;
        }
#endregion

	    public void initialize(int nSize, ActType _type, double bias = 1.0) {
            resize(nSize, bias);
            act_type_ = _type;
        }

        public void resize(int nSize, double bias = 1.0){
            m_daAct = new double[nSize];
            m_daGrad = new double[nSize];
            m_daAct[m_daAct.Length - 1] = bias;	// last element is bias
        }

 	    public void assignAllErrorToGrad(double[] target)
        {
            Debug.Assert(m_daAct.Length > target.Length, "NeuralNetworkDLL::LayerBase::assignErrorToGrad"); // target may include bias or not
            Debug.Assert(m_daGrad.Length > target.Length, "NeuralNetworkDLL::LayerBase::assignErrorToGrad");
            Debug.Assert(m_daAct.Length == m_daGrad.Length, "NeuralNetworkDLL::LayerBase::assignErrorToGrad");
            for (int d = 0; d < m_daAct.Length - 1; d++) {
                m_daGrad[d] = (target[d] - m_daAct[d]);
            }
        }
        public void activate()
        {
            switch (act_type_)
	        {
	        case ActType.Sigmoid:
		        applySigmoidToVector(1,ref m_daAct);		// 1 is not to activate bias at the end of act_ vector
		        break;
	        case ActType.ReLU:
		        applyRELUToVector(1,ref m_daAct);		// 1 is not to activate bias at the end of act_ vector
		        break;
	        default:
		        applyLRELUToVector(1,ref m_daAct);		// 1 is not to activate bias at the end of act_ vector
                break;
	        }
        }
        public void applySigmoidToVector(int nbiasCount, ref double[] vector)
	    {
		    int num = vector.Length - nbiasCount;
		    for (int i = 0; i < num; i++) // don't apply activation function to bias
			    vector[i] = getSigmoid(vector[i]);
	    }
        public void applyRELUToVector(int nbiasCount, ref double[] vector)
	    {
            int num = vector.Length - nbiasCount;
		    for (int i = 0; i < num; i++) // don't apply activation function to bias
			    vector[i] = getRELU(vector[i]);
	    }
        public void applyLRELUToVector(int nbiasCount, ref double[] vector)
	    {
            int num = vector.Length - nbiasCount;
		    for (int i = 0; i < num; i++) // don't apply activation function to bias
			    vector[i] = getLRELU(vector[i]);
	    }

        public void multiplyActGradToGrad()
        {
            switch (act_type_)
	        {
	        case ActType.Sigmoid:
		        for (int d = 0; d < m_daAct.Length - 1; d++)   // skip last component (bias)		        
			        m_daGrad[d] *= getSigmoidGradFromY(m_daAct[d]);
		        break;
	        case ActType.ReLU:
		        for (int d = 0; d < m_daAct.Length - 1; d++)   // skip last component (bias)		        
			        m_daGrad[d] *= getRELUGradFromY(m_daAct[d]);
		        break;
	        default:
		        for (int d = 0; d < m_daAct.Length - 1; d++)   // skip last component (bias)		        
			        m_daGrad[d] *= getLRELUGradFromY(m_daAct[d]);
                break;
	        }
        }
     	public void SetActType(ActType type) {
            act_type_ = type;
        }

     	public void check()
     	{
            int nSize = m_daAct.Length;

            for (int i = 0; i < nSize; i++)
            {
                if (Double.IsNaN(m_daAct[i]))
                {
                    Debug.Assert(false, "NeuralNetworkDLL::LayerBase::check act isNan");
                }
                else if (Double.IsInfinity(m_daAct[i]))
                {
                    Debug.Assert(false, "NeuralNetworkDLL::LayerBase::check act isInf");
                }
            }          

            nSize = m_daGrad.Length;

            for (int i = 0; i < nSize; i++)
            {
                if (Double.IsNaN(m_daGrad[i]))
                {
                    Debug.Assert(false, "NeuralNetworkDLL::LayerBase::check grad isNan");
                }
                else if (Double.IsInfinity(m_daGrad[i]))
                {
                    Debug.Assert(false, "NeuralNetworkDLL::LayerBase::check grad isInf");
                }
            }
        }
    };

    public class NeuralNetwork
    {
        public int m_nInputSize
        {
            get;
            private set;
        }
        public int m_nOutputSize
        {
            get;
            private set;
        }
        public int m_nLayerSize
        { // hidden_layers_ + 2        
            get;
            private set;
        }

        double m_dBias;
        double m_dEta;
        double m_dAlpha;

        LayerBase[] m_aLayers;
        ConnectionBase[] m_aConnections;

        public NeuralNetwork() { }
        public NeuralNetwork(int nInputSize, int nOutputSize, int nHiddenLayerSize,
            double dBias = 1.0f, double dEta = 0.15f, double dAlpha = 0.5f)
        {
            Init(nInputSize, nOutputSize, nHiddenLayerSize, dBias, dEta, dAlpha);
        }

        public void Init(int nInputSize, int nOutputSize, int nHiddenLayerSize,
            double dBias = 1.0f, double dEta = 0.15f, double dAlpha = 0.5f)
        {
            m_nInputSize = nInputSize;
            m_nOutputSize = nOutputSize;
            m_nLayerSize = nHiddenLayerSize + 2; // +2 = input layer + 1, output layer + 1

            m_dBias = dBias;
            m_dEta = dEta;
            m_dAlpha = dAlpha;
            // initialize all layers
            m_aLayers = new LayerBase[m_nLayerSize];
            int nSize = m_nLayerSize - 1;
            int nInputCount = m_nInputSize + 1;     // +1 is bias
            for (int i = 0; i < nSize; ++i)
            {
                m_aLayers[i] = new LayerBase(nInputCount, LayerBase.ActType.LReLU, m_dBias);
            }
            m_aLayers[m_nLayerSize - 1] = new LayerBase(m_nOutputSize + 1, LayerBase.ActType.LReLU, m_dBias);

            // initialize connections between layers
            m_aConnections = new ConnectionBase[m_nLayerSize - 1];
            Array.Clear(m_aConnections, 0, m_aConnections.Length);
            for (int i = 0; i < m_aConnections.Length; i++)
            {
                SetFullConnection(i, 0.01f, 0.01f);
            }
        }

        public void SetLayer(int nLayerIndex, int nSize, LayerBase.ActType type)
        {
            Debug.Assert(nLayerIndex >= 0, "NeuralNetworkDLL::NeuralNetwork::SetLayer");
            Debug.Assert(nLayerIndex < m_aLayers.Length, "NeuralNetworkDLL::NeuralNetwork::SetLayer");
            m_aLayers[nLayerIndex] = new LayerBase(nSize, type, m_dBias);
        }

        public void SetLayerType(int nLayerIndex, LayerBase.ActType type)
        {
            Debug.Assert(nLayerIndex >= 0, "NeuralNetworkDLL::NeuralNetwork::SetLayerType");
            Debug.Assert(nLayerIndex < m_aLayers.Length, "NeuralNetworkDLL::NeuralNetwork::SetLayerType");
            m_aLayers[nLayerIndex].SetActType(type);
        }

        public void SetFullConnection(int nConnection_index, double rand_scale, double rand_min)
        {
            Debug.Assert(nConnection_index >= 0, "NeuralNetworkDLL::NeuralNetwork::setFullConnection");
            Debug.Assert(nConnection_index < m_aConnections.Length, "NeuralNetworkDLL::NeuralNetwork::setFullConnection");

            int nNextCount = m_aLayers[nConnection_index + 1].m_daAct.Length - 1;
            int nPreCount = m_aLayers[nConnection_index].m_daAct.Length;
            m_aConnections[nConnection_index] = new FullConnection(nNextCount, nPreCount, rand_scale, rand_min);
        }

        //         private void Assert(bool p)
        //         {
        //             throw new NotImplementedException();
        //         }

        // forward propagation
        public void FeedForward()
        {
            for (int l = 0; l < m_aConnections.Length; l++)
            {
                m_aConnections[l].forward(m_aLayers[l].m_daAct, ref m_aLayers[l + 1].m_daAct);
                m_aLayers[l + 1].activate();
            }
        }
        public void SetInput(double[] daInput)
        {
            Debug.Assert(daInput.Length >= m_nInputSize);
            Array.Copy(daInput, m_aLayers[0].m_daAct, m_nInputSize);
        }

        public void PropBackward(double[] daTarget)    // backward propagation
        {
            m_aLayers[m_aLayers.Length - 1].assignAllErrorToGrad(daTarget);
            int nSize = m_aConnections.Length - 1;
            for (int l = nSize; l >= 0; l--)
            {
                m_aLayers[l + 1].multiplyActGradToGrad();
                m_aConnections[l].backward(m_aLayers[l + 1].m_daGrad, ref m_aLayers[l].m_daGrad);
            }
            UpdateConnectionWeights();
        }

        public void UpdateConnectionWeights()
        {
            int nSize = m_aConnections.Length - 1;
            for (int l = nSize; l >= 0; l--)
                m_aConnections[l].updateWeights(m_dEta, m_dAlpha, m_aLayers[l + 1].m_daGrad, m_aLayers[l].m_daAct);
        }

        public int GetOutputIXEpsilonGreedy(double epsilon)
        {
            if (epsilon > 0.0)
            {
                Random r = new Random();
                if (r.NextDouble() < epsilon)
                    return r.Next(m_nOutputSize);
            }
            return GetMaxIndexFromOutput();
        }
        public int GetMaxIndexFromOutput()
        {
            if (m_aLayers.Length <= 0)
                return -1;
            double[] output_layer_act = m_aLayers[m_aLayers.Length - 1].m_daAct;
            double max = output_layer_act[0];
            int ix = 0;
            for (int d = 1; d < m_nOutputSize; d++)
            {
                if (max < output_layer_act[d])
                {
                    max = output_layer_act[d];
                    ix = d;
                }
            }
            return ix;
        }
        public int GetProbabilityIndexFromOutput()
        {
            if (m_aLayers.Length <= 0)
                return -1;
            double[] output_layer_act = m_aLayers[m_aLayers.Length - 1].m_daAct;
            double[] possibility = new double[m_nOutputSize];
            double sum = 0;
            for (int d = 0; d < m_nOutputSize; d++)
            {
                sum += output_layer_act[d];
            }

            if (sum == 0.0) return 0;
            double accum = 0.0;
            for (int d = 0; d < m_nOutputSize; d++)
            {
                accum += output_layer_act[d] / sum;
                possibility[d] = accum;
            }

            Random rand = new Random();
            double r = rand.NextDouble();

            for (int d = 0; d < m_nOutputSize; d++)
            {
                if (r < possibility[d]) return d;
            }

            return m_nOutputSize - 1;
        }
        public double? GetMaxValueFromOutput()
        {
            if (m_aLayers.Length <= 0)
                return null;
            double[] output_layer_act = m_aLayers[m_aLayers.Length - 1].m_daAct;
            double max = output_layer_act[0];
            //            int ix = 0;
            for (int d = 1; d < m_nOutputSize; d++)
            {
                if (max < output_layer_act[d])
                {
                    max = output_layer_act[d];
                    //                    ix = d;
                }
            }
            return max;
        }
        public double? GetL2NormError(double[] desired)
        {
            if (m_aLayers.Length <= 0)
                return null;
            double[] output_layer_act = m_aLayers[m_aLayers.Length - 1].m_daAct;
            double sum = 0;
            for (int d = 0; d < m_nOutputSize; d++)
                sum += Math.Pow(desired[d] - output_layer_act[d], 2);
            return Math.Sqrt(sum);
        }
        public double? GetLinfNormError(double[] desired)
        {
            if (m_aLayers.Length <= 0)
                return null;
            double[] output_layer_act = m_aLayers[m_aLayers.Length - 1].m_daAct;
            double temp = 0;
            for (int d = 0; d < m_nOutputSize; d++)
                temp = Math.Max(temp, Math.Abs(desired[d] - output_layer_act[d]));
            return temp;
        }
        public double GetOutput(int ix)
        {
            return m_aLayers[m_aLayers.Length - 1].m_daAct[ix];
        }

        public void copyOutputVectorTo(ref double[] copy, bool copy_bias = false)
        {
            double[] output_layer_act = m_aLayers[m_aLayers.Length - 1].m_daAct;
            if (copy_bias == false)
            {
                if (copy.Length < m_nOutputSize)
                    copy = new double[m_nOutputSize];
                for (int d = 0; d < m_nOutputSize; d++)
                    copy[d] = output_layer_act[d];
            }
            else
            {
                int nSize = m_nOutputSize + 1;
                if (copy.Length < nSize)
                    copy = new double[nSize];
                for (int d = 0; d < nSize; d++)
                    copy[d] = output_layer_act[d];
            }
        }

        // 	    void readTXT(StreamReader sr)
        //         {
        //            
        //         }
        // 
        //         void writeTXT(string sFilePath)
        //         {
        //             StreamWriter sw = new StreamWriter(sFilePath);
        // 
        //             sw.WriteLine(m_aConnections.Length);
        // 
        //             int nSize = m_aConnections.Length;
        //             for (int i = 0; i < nSize; i++)
        //             {
        //             	//connections_[i].
        //             }
        // 
        //             nSize = m_aLayers.Length;
        //             for (int i = 0; i < nSize; i++)
        //             {
        //                 //layers_[i].act_.
        //             }
        //         }

        public void check()
        {
            for (int i = 0; i < m_aLayers.Length; i++)
                m_aLayers[i].check();

            for (int i = 0; i < m_aConnections.Length; i++)
                m_aConnections[i].check();
        }
    }
}
