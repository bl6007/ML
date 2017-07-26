using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularQueue<TT> {

    int tail_;
    bool full_;
    List<TT> m_lData;

    public void Init(int num_max, TT tInit)
    {
        tail_ = 0;
        full_ = false;

        m_lData = new List<TT>(num_max);
        for(int i=0; i<num_max; i++)
            m_lData.Add(tInit);
    }

    public void pushBack(TT input)
    {
        m_lData[tail_] = input;
        tail_++;
        if (full_ == false && tail_ >= m_lData.Count) full_ = true;
        tail_ = tail_ % m_lData.Count;
    }

    public void getValue(int index, out TT t)
    {
        t = m_lData[(tail_ + index) % m_lData.Count];// avilable when queue is full
    }

    void getFirst(out TT t)
    {
        if (full_ == false) t = m_lData[0];
        else getValue(0, out t);
    }

    void getLast( out TT t)
    {
        getValue(-1, out t);
    }
}
