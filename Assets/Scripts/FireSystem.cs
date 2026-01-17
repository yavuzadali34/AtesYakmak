using UnityEngine;
using Entropy.Systems;

namespace Entropy.Survival
{
    /// <summary>
    /// Ateş yakma mekaniği ve Jack London'ın "Ladin Ağacı Tuzağı"nı simüle eder.
    /// </summary>
    public class FireSystem : MonoBehaviour
    {
        [Header("Ateş Özellikleri")]
        public bool isLit = false;
        public float duration = 60.0f; // Ateşin yanma süresi
        public float warmthRadius = 5.0f;

        [Header("Ladin Ağacı Tuzağı (Spruce Tree Trap)")]
        public bool isUnderTree = false;
        public float treeCollapseChance = 0.1f; // Zamanla artabilir

        private ThermodynamicsManager _playerThermo;
        private ParticleSystem _fireParticles;

        void Start()
        {
            _fireParticles = GetComponentInChildren<ParticleSystem>();
            if (_fireParticles != null && !isLit) _fireParticles.Stop();
        }

        void Update()
        {
            if (isLit)
            {
                ManageFireLife();
                CheckWarmth();
                CheckTreeTrap();
            }
        }

        public void LightFire()
        {
            isLit = true;
            if (_fireParticles != null) _fireParticles.Play();
            Debug.Log("<color=orange>Ateş Yandı!</color>");
        }

        private void ManageFireLife()
        {
            duration -= Time.deltaTime;
            if (duration <= 0)
            {
                Extinguish();
            }
        }

        private void CheckWarmth()
        {
            if (_playerThermo == null) _playerThermo = FindObjectOfType<ThermodynamicsManager>();
            if (_playerThermo == null) return;

            float dist = Vector3.Distance(transform.position, _playerThermo.transform.position);
            _playerThermo.isNearFire = (dist <= warmthRadius);
        }

        private void CheckTreeTrap()
        {
            // Eğer ağaç altındaysa ve ateş belli bir süre yanmışsa, kar düşme riski artar.
            if (isUnderTree && isLit)
            {
                // Kitaptaki sahne: Ladin ağacının üzerindeki kar ateşin ısısıyla düşer.
                if (Random.value < 0.005f) // Her frame şans (Temsili)
                {
                    Debug.Log("<color=red>FELAKET: Ladin ağacından düşen kar ateşi söndürdü!</color>");
                    Extinguish();
                    // Ekstra: Oyuncuyu biraz daha dondurabiliriz
                }
            }
        }

        public void Extinguish()
        {
            isLit = false;
            if (_fireParticles != null) _fireParticles.Stop();
            if (_playerThermo != null) _playerThermo.isNearFire = false;
            Debug.Log("<color=grey>Ateş Söndü.</color>");
        }
    }
}
