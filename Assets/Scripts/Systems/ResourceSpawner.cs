using System.Collections.Generic;
using UnityEngine;
using Entropy.Systems;

namespace Entropy.Systems
{
    /// <summary>
    /// Procedural resource spawning ve object pooling
    /// </summary>
    public class ResourceSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnArea
        {
            public Vector2 center;
            public Vector2 size;
            public LayerMask avoidLayer; // Buz üzerine spawn etme gibi
        }

        [Header("Spawn Settings")]
        public SpawnArea[] spawnAreas;
        public GameObject collectableItemPrefab; // CollectableItem prefab

        [Header("Item Distribution")]
        public ItemData[] itemsToSpawn;
        public int[] spawnWeights; // Her item'ın spawn şansı (örn: [50, 30, 20])

        [Header("Spawn Constraints")]
        public int totalItemsToSpawn = 20;
        public float minDistanceBetweenItems = 2.0f;
        public int maxSpawnAttempts = 50;

        [Header("Object Pooling")]
        public bool useObjectPooling = true;
        public int poolSize = 30;

        private Queue<GameObject> _itemPool = new Queue<GameObject>();
        private List<Vector3> _spawnedPositions = new List<Vector3>();

        void Start()
        {
            if (useObjectPooling)
            {
                InitializePool();
            }

            SpawnResources();
        }

        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(collectableItemPrefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                _itemPool.Enqueue(obj);
            }
        }

        private GameObject GetFromPool()
        {
            if (_itemPool.Count > 0)
            {
                GameObject obj = _itemPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Pool boşsa yeni oluştur
                return Instantiate(collectableItemPrefab);
            }
        }

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            _itemPool.Enqueue(obj);
        }

        private void SpawnResources()
        {
            _spawnedPositions.Clear();
            int totalWeight = 0;

            // Toplam weight hesapla
            foreach (int weight in spawnWeights)
            {
                totalWeight += weight;
            }

            for (int i = 0; i < totalItemsToSpawn; i++)
            {
                Vector3? spawnPos = FindValidSpawnPosition();
                if (!spawnPos.HasValue) continue;

                // Weighted random item seçimi
                ItemData selectedItem = GetRandomWeightedItem(totalWeight);
                if (selectedItem == null) continue;

                // Item spawn
                GameObject itemObj;
                if (useObjectPooling)
                    itemObj = GetFromPool();
                else
                    itemObj = Instantiate(collectableItemPrefab);

                itemObj.transform.position = spawnPos.Value;
                itemObj.transform.SetParent(transform);

                // CollectableItem component'ine item data ata
                var collectableItem = itemObj.GetComponent<Environment.CollectableItem>();
                if (collectableItem != null)
                {
                    collectableItem.itemData = selectedItem;
                    collectableItem.amount = 1;
                }

                _spawnedPositions.Add(spawnPos.Value);
            }

            Debug.Log($"<color=cyan>{_spawnedPositions.Count} resource spawn edildi.</color>");
        }

        private Vector3? FindValidSpawnPosition()
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                // Random spawn area seç
                SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Length)];

                // Area içinde random pozisyon
                float x = Random.Range(area.center.x - area.size.x / 2, area.center.x + area.size.x / 2);
                float z = Random.Range(area.center.y - area.size.y / 2, area.center.y + area.size.y / 2);
                Vector3 pos = new Vector3(x, 0.5f, z); // Y offset (zemin üstü)

                // Raycast ile zemin yüksekliğini bul
                RaycastHit hit;
                if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out hit, 20f))
                {
                    pos.y = hit.point.y + 0.5f;

                    // Kaçınılması gereken layer kontrolü
                    if (area.avoidLayer != 0 && ((1 << hit.collider.gameObject.layer) & area.avoidLayer) != 0)
                    {
                        continue; // Bu pozisyon uygun değil
                    }
                }

                // Minimum mesafe kontrolü
                bool tooClose = false;
                foreach (Vector3 existingPos in _spawnedPositions)
                {
                    if (Vector3.Distance(pos, existingPos) < minDistanceBetweenItems)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    return pos;
                }
            }

            Debug.LogWarning("Geçerli spawn pozisyonu bulunamadı!");
            return null;
        }

        private ItemData GetRandomWeightedItem(int totalWeight)
        {
            if (itemsToSpawn.Length == 0) return null;

            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            for (int i = 0; i < itemsToSpawn.Length; i++)
            {
                cumulativeWeight += spawnWeights[i];
                if (randomValue < cumulativeWeight)
                {
                    return itemsToSpawn[i];
                }
            }

            return itemsToSpawn[0]; // Fallback
        }

        // Gizmos ile spawn area görselleştirme
        void OnDrawGizmosSelected()
        {
            if (spawnAreas == null) return;

            Gizmos.color = Color.cyan;
            foreach (var area in spawnAreas)
            {
                Vector3 center = new Vector3(area.center.x, 0, area.center.y);
                Vector3 size = new Vector3(area.size.x, 0.1f, area.size.y);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
