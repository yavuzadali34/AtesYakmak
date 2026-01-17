using UnityEngine;
using Entropy.Player;

namespace Entropy.Environment
{
    /// <summary>
    /// Zemindeki farklı bölgelerin (Derin kar, İnce buz) etkilerini yönetir.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        public enum TerrainType { Solid, DeepSnow, ThinIce }
        public TerrainType currentTerrain = TerrainType.Solid;

        [Header("Settings")]
        public float snowMovePenalty = 0.5f; // Derin karda hız %50 düşer.
        public LayerMask terrainLayer;

        private PlayerController _player;

        void Start()
        {
            _player = FindObjectOfType<PlayerController>();
        }

        void Update()
        {
            CheckTerrainUnderPlayer();
        }

        private void CheckTerrainUnderPlayer()
        {
            if (_player == null) return;

            RaycastHit hit;
            if (Physics.Raycast(_player.transform.position + Vector3.up, Vector3.down, out hit, 2f, terrainLayer))
            {
                if (hit.collider.CompareTag("DeepSnow"))
                {
                    HandleDeepSnow();
                }
                else if (hit.collider.CompareTag("ThinIce"))
                {
                    HandleThinIce();
                }
                else
                {
                    currentTerrain = TerrainType.Solid;
                    // Normal hızı PlayerController zaten self-regulate ediyor slowFactor ile
                }
            }
        }

        private void HandleDeepSnow()
        {
            currentTerrain = TerrainType.DeepSnow;
            // Derin kar efektleri (parçacıklar vs.) eklenebilir
        }

        private void HandleThinIce()
        {
            currentTerrain = TerrainType.ThinIce;
            
            // GridManager ile de kontrol et
            if (GridManager.Instance != null && _player != null)
            {
                bool isDangerous = GridManager.Instance.KonumTehlikeliMi(_player.transform.position);
                
                if (isDangerous)
                {
                    // Buza basma süresi kontrolü
                    _timeOnThinIce += Time.deltaTime;
                    
                    if (_timeOnThinIce > iceBreakDelay)
                    {
                        BreakIce();
                    }
                }
                else
                {
                    _timeOnThinIce = 0f;
                }
            }
        }

        private float _timeOnThinIce = 0f;
        [Header("Thin Ice Settings")]
        public float iceBreakDelay = 2.0f; // 2 saniye sonra kırılır

        private void BreakIce()
        {
            Debug.Log("<color=cyan>İNCE BUZ KIRILDI! Suya düştün!</color>");
            
            // Oyuncuyu ıslatıyor
            if (_player != null)
            {
                var thermoManager = _player.GetComponent<Entropy.Systems.ThermodynamicsManager>();
                if (thermoManager != null)
                {
                    thermoManager.GetDrenched();
                }
            }
            
            // TODO: Buz kırılma VFX ve ses efekti eklenebilir
            
            // Reset
            _timeOnThinIce = 0f;
            currentTerrain = TerrainType.Solid;
        }
    }
}
