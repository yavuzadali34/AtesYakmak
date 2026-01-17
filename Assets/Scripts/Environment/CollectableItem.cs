using UnityEngine;
using Entropy.Systems;

namespace Entropy.Environment
{
    /// <summary>
    /// Sahada toplanabilir itemlar (Odun, Kibrit, vs.)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectableItem : MonoBehaviour
    {
        [Header("Item Data")]
        public ItemData itemData;
        public int amount = 1;

        [Header("VFX")]
        public GameObject collectVFX;
        public AudioClip collectSound;

        [Header("Settings")]
        public float bobSpeed = 1.0f;
        public float bobHeight = 0.2f;
        public bool rotateItem = true;

        private Vector2 _startPos;
        private AudioSource _audioSource;

        void Start()
        {
            _startPos = transform.position;
            _audioSource = GetComponent<AudioSource>();

            // Collider'ı trigger yap
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void Update()
        {
            // Yukarı-aşağı hareket (Bob effect)
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.y);

            // Dönme efekti
            if (rotateItem)
            {
                transform.Rotate(Vector2.up, 50f * Time.deltaTime);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Oyuncu tag kontrolü
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            // Envantere ekle
            if (InventoryManager.Instance != null)
            {
                bool success = InventoryManager.Instance.AddItem(itemData, amount);
                if (!success)
                {
                    Debug.Log("<color=yellow>Envanter dolu veya stack limit!</color>");
                    return;
                }
            }

            // VFX spawn
            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, Quaternion.identity);
            }

            // Ses çal
            if (collectSound != null && _audioSource != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            // Objeyi yok et (veya pool'a geri dön)
            Destroy(gameObject);
        }

        // Gizmos ile editörde görselleştirme
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
