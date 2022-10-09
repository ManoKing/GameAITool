using UnityEngine;

namespace Unity.LEGO.Minifig
{
    public class BlinkAndDestroy : MonoBehaviour
    {
        public float timeLeft = 4.0f;

        private float blinkPeriod = 0.8f;
        private float blinkFrequency = 0.1f;

        private Renderer theRenderer;
        private float timeBlink;

        void Start()
        {
            theRenderer = GetComponent<Renderer>();
            timeLeft += Random.Range(-0.3f, 0.3f);
        }

        void Update()
        {
            timeLeft -= Time.deltaTime;

            if (timeLeft <= blinkPeriod)
            {
                if (timeBlink <= 0.0f)
                {
                    timeBlink += blinkFrequency;
                    theRenderer.enabled = !theRenderer.enabled;
                }

                timeBlink -= Time.deltaTime;
            }

            if (timeLeft <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
