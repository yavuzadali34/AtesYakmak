using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Entropy.Systems
{
    /// <summary>
    /// Singleton Inventory System: Oyuncunun envanterini yönetir
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;

        [Header("Inventory")]
        private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();

        [Header("Events")]
        public UnityEvent<ItemData, int> onItemAdded;
        public UnityEvent<ItemData, int> onItemRemoved;
        public UnityEvent onInventoryChanged;

        [Header("Debug")]
        public bool showDebugLogs = true;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Envantere item ekle
        /// </summary>
        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null) return false;

            if (_inventory.ContainsKey(item))
            {
                // Stack limit kontrolü
                if (item.isStackable)
                {
                    int currentAmount = _inventory[item];
                    int totalAmount = currentAmount + amount;

                    if (totalAmount > item.maxStackSize)
                    {
                        if (showDebugLogs)
                            Debug.LogWarning($"Stack limit aşıldı: {item.itemName}");
                        return false;
                    }

                    _inventory[item] = totalAmount;
                }
                else
                {
                    if (showDebugLogs)
                        Debug.LogWarning($"Bu item stacklenemez: {item.itemName}");
                    return false;
                }
            }
            else
            {
                _inventory.Add(item, amount);
            }

            onItemAdded?.Invoke(item, amount);
            onInventoryChanged?.Invoke();

            if (showDebugLogs)
                Debug.Log($"<color=green>+{amount} {item.itemName} eklendi.</color>");

            return true;
        }

        /// <summary>
        /// Envanterden item çıkar
        /// </summary>
        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (item == null || !_inventory.ContainsKey(item)) return false;

            int currentAmount = _inventory[item];
            if (currentAmount < amount)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"Yetersiz {item.itemName}. Mevcut: {currentAmount}, İstenen: {amount}");
                return false;
            }

            _inventory[item] -= amount;

            if (_inventory[item] <= 0)
            {
                _inventory.Remove(item);
            }

            onItemRemoved?.Invoke(item, amount);
            onInventoryChanged?.Invoke();

            if (showDebugLogs)
                Debug.Log($"<color=red>-{amount} {item.itemName} kullanıldı.</color>");

            return true;
        }

        /// <summary>
        /// Belirli bir item'dan yeterli miktarda var mı kontrol et
        /// </summary>
        public bool HasItem(ItemData item, int amount = 1)
        {
            if (item == null || !_inventory.ContainsKey(item)) return false;
            return _inventory[item] >= amount;
        }

        /// <summary>
        /// Belirli bir item'dan kaç tane var?
        /// </summary>
        public int GetItemCount(ItemData item)
        {
            if (item == null || !_inventory.ContainsKey(item)) return 0;
            return _inventory[item];
        }

        /// <summary>
        /// Tüm envanteri döndür (UI için)
        /// </summary>
        public Dictionary<ItemData, int> GetAllItems()
        {
            return new Dictionary<ItemData, int>(_inventory);
        }

        /// <summary>
        /// Envanteri temizle
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            onInventoryChanged?.Invoke();
            if (showDebugLogs)
                Debug.Log("<color=grey>Envanter temizlendi.</color>");
        }
    }
}
