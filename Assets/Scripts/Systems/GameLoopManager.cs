using UnityEngine;
using UnityEngine.SceneManagement;
using Entropy.Systems;

namespace Entropy.Systems
{
    /// <summary>
    /// Oyunun ana döngüsünü, ölüm ve yeniden başlama durumlarını yönetir.
    /// </summary>
    public class GameLoopManager : MonoBehaviour
    {
        public ThermodynamicsManager thermoManager;
        public GameObject gameOverUI;

        void Start()
        {
            if (thermoManager != null)
            {
                thermoManager.onGameOver.AddListener(OnPlayerDeath);
            }
            if (gameOverUI != null) gameOverUI.SetActive(false);
        }

        private void OnPlayerDeath()
        {
            Debug.Log("<color=black>OYUN BİTTİ: Doğa kazandı.</color>");
            if (gameOverUI != null) gameOverUI.SetActive(true);
            
            // Zamanı yavaşlat veya durdur
            Time.timeScale = 0.5f; 
        }

        public void RestartGame()
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Test amaçlı: Suya düşme tetikleyici
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) // 'K' harfi ile suya düşme testi
            {
                thermoManager.GetDrenched();
            }
        }
    }
}
