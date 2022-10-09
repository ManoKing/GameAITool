using UnityEngine;

namespace Unity.LEGO.Behaviours
{
    public class BlinkAndDisable : MonoBehaviour
    {
        const float k_BlinkPeriod = 0.8f;
        const float k_BlinkFrequency = 0.1f;

        float m_TimeLeft = 4.0f;
        float m_TimeBlink;

        Renderer[] m_Renderers;

        void Start()
        {
            m_Renderers = GetComponentsInChildren<Renderer>();
            m_TimeLeft += Random.Range(-0.3f, 0.3f);
        }

        void Update()
        {
            m_TimeLeft -= Time.deltaTime;

            if (m_TimeLeft <= k_BlinkPeriod)
            {
                if (m_TimeBlink <= 0.0f)
                {
                    m_TimeBlink += k_BlinkFrequency;

                    foreach (var renderer in m_Renderers)
                    {
                        renderer.enabled = !renderer.enabled;
                    }
                }

                m_TimeBlink -= Time.deltaTime;
            }

            if (m_TimeLeft <= 0.0f)
            {
                var rigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();
                foreach (var rigidBody in rigidBodies)
                {
                    Destroy(rigidBody);
                }
                gameObject.SetActive(false);
            }
        }
    }
}
