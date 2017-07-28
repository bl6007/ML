using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
    Renderer[] m_AllRenderer;
    Light[] m_Light;
    Camera m_Camera;
    public bool m_bTraning = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))  {
            ToggleTraning();
        }
    }

    void SetTrainig(bool bTraining)
    {
        m_bTraning = bTraining;
        if (m_bTraning)
        {
            Time.timeScale = 6;
            for (int i = 0; i < m_AllRenderer.Length; i++)
                m_AllRenderer[i].enabled = false;
            for (int i = 0; i < m_Light.Length; i++)
                m_Light[i].enabled = false;
            m_Camera.enabled = false;
        }
        else
        {
            Time.timeScale = 6;
            for (int i = 0; i < m_AllRenderer.Length; i++)
                m_AllRenderer[i].enabled = true;
            for (int i = 0; i < m_Light.Length; i++)
                m_Light[i].enabled = true;
            m_Camera.enabled = true;
        }
    }

    void ToggleTraning()
    {
        SetTrainig(!m_bTraning);
    }

    void Awake()
    {
        m_AllRenderer = gameObject.GetComponentsInChildren<Renderer>(true);
        m_Light = gameObject.GetComponentsInChildren<Light>(true);
        m_Camera = Camera.main;
    }
}
