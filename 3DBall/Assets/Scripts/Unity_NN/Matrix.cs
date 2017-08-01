using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace MathDLL
{
    public class Matrix
    {
        public int num_rows_
        {
            get;
            private set;
        }
        public int num_cols_
        {
            get;
            private set;
        }
        double[] values_;

        public Matrix(int _m, int _n)
        {
            num_rows_ = _m;
            num_cols_ = _n;
            int num_all = num_rows_ * num_cols_;
            values_ = new double[num_all];
            for (int i = 0; i < num_all; i++) values_[i] = 0;
        }

        public int GetSize()
        {
            return num_rows_ * num_cols_;
        }

        public void Initialize(int _m, int _n, bool init = true)
        {        
            int num_all_old = num_rows_ * num_cols_;

            num_rows_ = _m;
            num_cols_ = _n;

            int num_all = num_rows_ * num_cols_;
            if (num_all_old != num_all) {
                // check if the matrix is too large
                //Debug.Assert((double)num_rows_ * (double)num_cols_ <= (double)double.MaxValue);
                values_ = new double[num_all];
            }

            if (init) {
                for (int i = 0; i < num_all; i++)  values_[i] = 0;
            }
        }

        public void assignAllRandom(double scale, double min) {
            System.Random r = new System.Random();
		    int num_all = num_rows_ * num_cols_;
		    for (int i = 0; i < num_all; i++)
                values_[i] = r.NextDouble() * scale + min;            
	    }

        public void assignAllXavier(int nInputSize, int nOutputSize)
        {
            System.Random r = new System.Random();
            int num_all = num_rows_ * num_cols_;

            double d = Math.Sqrt(nInputSize);
            if (nInputSize > nOutputSize)
            {
                for (int i = 0; i < num_all; i++)
                    values_[i] = r.Next(nOutputSize, nInputSize) / d;
            }
            else
            {
                for (int i = 0; i < num_all; i++)
                    values_[i] = r.Next(nInputSize, nOutputSize) / d;
            }
            
        }

        public void assignAll(double v) {
		    int num_all = num_rows_ * num_cols_;
		    for (int i = 0; i < num_all; i++)
			    values_[i] = v;
	    }

        public void multiply(double[] vector, double[] result)
        {
            Debug.Assert(num_rows_ <= result.Length, "MathDLL::Matrix::multiply");
            Debug.Assert(num_cols_ <= vector.Length, "MathDLL::Matrix::multiply");

            for (int i = 0; i < num_rows_; i++) {
                result[i] = 0;
                int ix = i * num_cols_;
                double temp;
                for (int j = 0; j < num_cols_; j++, ix++) {
                    temp = values_[ix];
                    temp *= vector[j];
                    result[i] += temp;
                }
            }
        }

        public void multiplyTransposed(double[] vector, double[] result)
        {
            Debug.Assert(num_rows_ <= vector.Length, "MathDLL::Matrix::multiplyTransposed");
            Debug.Assert(num_cols_ <= result.Length, "MathDLL::Matrix::multiplyTransposed");            

            for (int col = 0; col < num_cols_; col++)
            {
                result[col] = 0;
                for (int row = 0, ix = col; row < num_rows_; row++, ix += num_cols_)
                {
                    result[col] += values_[ix] * vector[row];
                }
            }
        }

        public int get1DIndex(int row, int col)
        {
            Debug.Assert(row >= 0, "MathDLL::Matrix::GetIndex");
            Debug.Assert(col >= 0, "MathDLL::Matrix::GetIndex");
            Debug.Assert(row < num_rows_, "MathDLL::Matrix::GetIndex");
            Debug.Assert(row < num_cols_, "MathDLL::Matrix::GetIndex");
            return row * num_cols_ + col;
        }

        public void SetValue(int i, double d)
        {
            Debug.Assert(i < num_rows_ * num_cols_ && i >= 0, "MathDLL::Matrix::SetValue(int i, double d)");
            values_[i] = d;
        }

        public void SetValue(int row, int col, double d)
        {
            Debug.Assert(row < num_rows_ && row >= 0, "MathDLL::Matrix::SetValue(int row, int col, double d)");
            Debug.Assert(col < num_cols_ && col >= 0, "MathDLL::Matrix::SetValue(int row, int col, double d)");
            values_[get1DIndex(row, col)] = d;
        }
        
        public double getValue(int row, int column)
        {
            return values_[get1DIndex(row, column)];
        }

        public void getTransposed(Matrix result)
        {
            result.Initialize(num_cols_, num_rows_);

            for (int row = 0; row < num_rows_; row++)
            {
                int ix_from = row * num_cols_;
                int ix_to = row;

                for (int col = 0; col < num_cols_; col++, ix_from++, ix_to += num_rows_)
                {
                      result.values_[ix_to] = values_[ix_from];
                }
            }
        }

        //여기까지 함
	    void setDiagonal() {
            int num = Math.Min(num_cols_, num_rows_);
		    for (int i = 0; i < num_cols_ * num_rows_; i++) values_[i] = 0.0;
		    for (int i = 0; i < num; i++) values_[get1DIndex(i, i)] = 1.0;
	    }

// #if _Unity
//         void cout(StreamWriter sw)
//         {
//             sw.WriteLine(num_rows_);
//             sw.WriteLine(num_cols_);
//             for (int row = 0; row < num_rows_; row++)
//             {
//                 for (int col = 0; col < num_cols_; col++)
//                 {
//                     sw.Write(values_[get1DIndex(row, col)]);
//                     sw.Write("_");
//                 }
//                 sw.Write(sw.NewLine);
//             }
//         }
// #else   
//         void cout()
//         {
//             Console.Write(num_rows_);
//             Console.Write("_");
//             Console.WriteLine(num_cols_);            
//             for (int row = 0; row < num_rows_; row++)
//             {
//                 for (int col = 0; col < num_cols_; col++)
//                 {
//                     Console.Write(values_[get1DIndex(row, col)]);
//                     Console.Write("_");                    
//                 }
//                 Console.WriteLine("");
//             }
//         }
// #endif

        void normalizeAllRows(double row_sum_min)
	    {
		    for (int row = 0; row < num_rows_; row++)
			    normalizeRow(row, row_sum_min);
	    }

	    void normalizeRow(int row, double row_sum_min)
	    {
		    double row_sum = 0;
            int nSize = num_cols_ - 1;
            for (int col = 0; col < nSize; col++) // TODO normalize bias option
		    {
                row_sum += values_[get1DIndex(row, col)];
		    }

		    if (row_sum > row_sum_min)
		    {
			    for (int col = 0; col < num_cols_-1; col++)// TODO normalize bias option
			    {
				    values_[get1DIndex(row, col)] /= row_sum;
			    }
		    }
	    }

	    void writeTXT(StreamWriter sw) {
            sw.WriteLine("Matrix");
            sw.Write(num_rows_);
            sw.Write("_");
            sw.WriteLine(num_cols_);
            int nSize = num_rows_ * num_cols_;
            for (int i = 0; i < nSize; i++){
                if (i != 0 && i % num_cols_ == 0) sw.Write(sw.NewLine);
                sw.Write(values_[i]);
                sw.Write("_");
            }
        }

        public void check()
        {
            int nSize = num_rows_ * num_cols_;

            for (int i = 0; i < nSize; i++)
            {
                if (Double.IsNaN(values_[i]))
                {
                    Debug.Assert(false);
                    break;
                }
                else if (Double.IsInfinity(values_[i]))
                {
                    Debug.Assert(false);
                    break;
                }
            }

        }
    }
}
