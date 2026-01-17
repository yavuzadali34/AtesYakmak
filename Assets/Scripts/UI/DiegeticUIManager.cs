using UnityEngine;
using UnityEngine.UI;
using Entropy.Systems;

namespace Entropy.UI
{
    /// <summary>
    /// Sayısal veri yerine görsel efektlerle (Buzlanma, Kararma) oyuncuya durumunu bildirir.
    /// Jack London'ın naturalizmine uygun bir "Diegetic UI" yönetir.
    /// </summary>
    public class DiegeticUIManager : MonoBehaviour
    {
        [Header("References")]
        public ThermodynamicsManager thermoManager;
        public Image frostVignette; // Ekranın kenarlarındaki buz efekti
        public Image coldTint; // Ekranın maviye/griye dönmesi
        public Image wetnessVignette; // Islaklık göstergesi
        public Image windIndicator; // Rüzgar göstergesi
        public Image matchWarningIcon; // Kibrit uyarısı

        [Header("Settings")]
        public float maxFrostAlpha = 0.8f;
        public Color healthyColor = new Color(1, 1, 1, 0);
        public Color frozenColor = new Color(0.5f, 0.7f, 1f, 0.3f);
        public Color wetnessColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        public ItemData matchItemData; // Match reference

        void Update()
        {
            if (thermoManager == null) return;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Vücut ısısına göre buzlanma miktarını hesapla
            // 37°C -> 0, 28°C -> maxFrostAlpha
            float t = Mathf.InverseLerp(thermoManager.hypothermiaThreshold + 2f, thermoManager.minViableTemp, thermoManager.bodyTemp);
            
            if (frostVignette != null)
            {
                Color c = frostVignette.color;
                c.a = t * maxFrostAlpha;
                frostVignette.color = c;
            }

            if (coldTint != null)
            {
                coldTint.color = Color.Lerp(healthyColor, frozenColor, t);
            }

            // Islaklık göstergesi
            if (wetnessVignette != null)
            {
                Color wetColor = wetnessColor;
                wetColor.a = thermoManager.wetnessFactor * 0.6f;
                wetnessVignette.color = wetColor;
            }

            // Rüzgar göstergesi
            if (windIndicator != null && GameManager.Instance != null)
            {
                float windIntensity = GameManager.Instance.ruzgarAralığı;
                windIndicator.fillAmount = windIntensity;
                windIndicator.color = Color.Lerp(Color.green, Color.red, windIntensity);
            }

            // Kibrit uyarısı
            if (matchWarningIcon != null && matchItemData != null)
            {
                int matchCount = Systems.InventoryManager.Instance != null 
                    ? Systems.InventoryManager.Instance.GetItemCount(matchItemData) 
                    : 0;
                matchWarningIcon.gameObject.SetActive(matchCount <= 3 && matchCount > 0);
            }

            // Eğer sırılsıklamsa ekranın sallanması (Titreme efekti) eklenebilir
            if (thermoManager.wetnessFactor > 0.5f)
            {
                ApplyShaker();
            }
        }

        private void ApplyShaker()
        {
            // Kamera veya UI üzerinde hafif bir titreme (Shake) simülasyonu
            // transform.position += Random.insideUnitSphere * 0.01f;
        }
    }
}
