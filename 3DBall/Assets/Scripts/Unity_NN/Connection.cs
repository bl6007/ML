using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using MathDLL;

namespace NeuralNetworkDLL
{
    abstract class ConnectionBase
    {
        public abstract void forward(double[] pre_layer_acts_, ref  double[] next_layer_acts);
        public abstract void backward(double[] next_layer_grads_, ref double[] pre_layer_grads_);
        public abstract void updateWeights(double eta, double alpha, double[] next_layer_grad, double[] prev_layer_act);
        //public abstract void writeTXT(std::ofstream& of);
        public abstract void check();
    };

    class FullConnection : ConnectionBase
    {
        Matrix weights_;
        Matrix delta_weights_;

        public FullConnection(int num_next, int num_prev, double rand_scale, double rand_min) {
            weights_ = new Matrix(num_next, num_prev);
            delta_weights_ = new Matrix(num_next, num_prev);
            weights_.assignAllRandom(rand_scale, rand_min);
        }

        public override void forward(double[] pre_layer_acts_, ref double[] next_layer_acts)
        {
            weights_.multiply(pre_layer_acts_, next_layer_acts);
        }
        public override void backward(double[] next_layer_grads_, ref double[] pre_layer_grads_)
        {
            weights_.multiplyTransposed(next_layer_grads_, pre_layer_grads_);
        }
        public override void updateWeights(double eta, double alpha, double[] next_layer_grad, double[] prev_layer_act)
        {
            int num_rows = weights_.num_rows_;
            int num_cols = weights_.num_cols_;

            for (int row = 0; row < num_rows; row++)
            {
                for (int col = 0; col < num_cols; col++)
                {
                    double old_delta_w = delta_weights_.getValue(row, col);
                    double delta_w = eta * next_layer_grad[row] * prev_layer_act[col] + alpha * old_delta_w;
                    weights_.SetValue(row, col, weights_.getValue(row, col) + delta_w);
                    delta_weights_.SetValue(row, col, delta_w);
                }
            }
        }
        //public abstract void writeTXT(std::ofstream& of);
        public override void check()
        {
            weights_.check();
            delta_weights_.check();
        }
    }
}
