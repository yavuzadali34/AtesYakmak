using UnityEngine;
using UnityEngine.Events;

namespace Entropy.Systems
{
    /// <summary>
    /// Jack London'ın "Ateş Yakmak" öyküsündeki termodinamik yasalarını simüle eder.
    /// Vücut ısısı (Entropy) yönetiminden sorumludur.
    /// </summary>
    public class ThermodynamicsManager : MonoBehaviour
    {
        [Header("Vücut Değerleri")]
        public float bodyTemp = 37.0f; // Başlangıç ısısı (Normal: 37°C)
        public float minViableTemp = 28.0f; // Donma sınırı
        public float hypothermiaThreshold = 32.0f; // Hipotermi sınırı

        [Header("Çevre Koşulları")]
        public float ambientTemp = -60.0f; // Ortam sıcaklığı (Jack London: -75°F ≈ -60°C)
        public float wetnessFactor = 0f; // 0 (Kuru) - 1 (Sırılsıklam)
        public float insulation = 1.0f; // Giysi koruması (1: Normal, >1: Daha iyi)

        [Header("Durum Değişkenleri")]
        public bool isSheltered = false;
        public bool isMoving = false;
        public bool isNearFire = false;
        public float fireWarmthEffect = 40.0f; // Ateşin sağladığı ortam ısısı artışı

        [Header("Events")]
        public UnityEvent onHypothermia;
        public UnityEvent onGameOver;

        private bool _hasTriggeredHypothermia = false;

        void Update()
        {
            CalculateEntropy();
        }

        private void CalculateEntropy()
        {
            // Efektif ortam ısısı (Ateş yanındaysa artar)
            float effectiveAmbient = ambientTemp + (isNearFire ? fireWarmthEffect : 0);

            // İletkenlik (Conduction): Islaklık ısı kaybı hızını devasa artırır.
            // 0.01f (Kuru) -> 0.51f (Tamamen ıslak)
            float conduction = 0.01f + (wetnessFactor * 0.5f);
            
            // Yalıtım etkisi (Giysiler kaybı böler)
            conduction /= insulation;

            // Rüzgar faktörü (Sığınak yoksa kayıp artar)
            if (!isSheltered) conduction += 0.04f;

            // Newton'un Soğuma Yasası: Isı farkı x İletkenlik
            float heatLoss = (bodyTemp - effectiveAmbient) * conduction * Time.deltaTime;
            
            // Metabolik Isı: Hareket ısı üretir ama kalori harcar (Enerji sistemi eklenebilir)
            float metabolicHeat = isMoving ? 0.05f * Time.deltaTime : 0f;

            // Vücut ısısını güncelle
            bodyTemp -= (heatLoss - metabolicHeat);

            // Durum Kontrolleri
            CheckThresholds();
        }

        private void CheckThresholds()
        {
            if (bodyTemp < hypothermiaThreshold && !_hasTriggeredHypothermia)
            {
                _hasTriggeredHypothermia = true;
                onHypothermia?.Invoke();
                Debug.Log("<color=orange>Kritik: Hipotermi Başlıyor!</color>");
            }

            if (bodyTemp <= minViableTemp)
            {
                onGameOver?.Invoke();
                Debug.Log("<color=red>Ölüm: Donarak hayatınızı kaybettiniz.</color>");
                this.enabled = false; // Sistemi durdur
            }

            // Eğer ateş başında ısınırsa hipotermi bayrağını sıfırla (Basit mantık)
            if (bodyTemp > hypothermiaThreshold + 1.0f)
            {
                _hasTriggeredHypothermia = false;
            }
        }

        // Wetness'ı zamanla azalt (Kuruma mekaniği)
        public void DryOut(float rate)
        {
            wetnessFactor = Mathf.Max(0, wetnessFactor - rate * Time.deltaTime);
        }

        // Suya düşme durumu
        public void GetDrenched()
        {
            wetnessFactor = 1.0f;
            Debug.Log("<color=blue>SUYA DÜŞTÜN! Hemen ateş yakmalısın!</color>");
        }
    }
}
