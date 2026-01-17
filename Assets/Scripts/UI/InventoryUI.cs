using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entropy.Systems;
using System.Collections.Generic;

namespace Entropy.UI
{
    /// <summary>
    /// Envanter panelini yönetir ve günceller
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject itemSlotPrefab;
        public Transform itemContainer;
        public GameObject inventoryPanel;

        [Header("Match Counter (Important Resource)")]
        public TextMeshProUGUI matchCountText;
        public Image matchWarningIcon;
        public ItemData matchItemData; // Match item reference

        [Header("Settings")]
        public Color warningColor = Color.red;
        public int warningThreshold = 3; // Son 3 kibrit uyarısı

        private List<GameObject> _activeSlots = new List<GameObject>();

        void Start()
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            // Inventory event'lerine abone ol
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.onInventoryChanged.AddListener(RefreshUI);
            }

            RefreshUI();
        }

        void Update()
        {
            // Tab tuşu ile envanter aç/kapa
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }
        }

        public void ToggleInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
                if (inventoryPanel.activeSelf)
                {
                    RefreshUI();
                }
            }
        }

        private void RefreshUI()
        {
            // Eski slot'ları temizle
            foreach (var slot in _activeSlots)
            {
                Destroy(slot);
            }
            _activeSlots.Clear();

            if (InventoryManager.Instance == null) return;

            // Tüm item'ları al
            Dictionary<ItemData, int> items = InventoryManager.Instance.GetAllItems();

            // Her item için slot oluştur
            foreach (var kvp in items)
            {
                ItemData item = kvp.Key;
                int amount = kvp.Value;

                GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
                _activeSlots.Add(slot);

                // Slot UI'ını doldur
                Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && item.icon != null)
                    icon.sprite = item.icon;

                TextMeshProUGUI amountText = slot.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
                if (amountText != null)
                    amountText.text = amount.ToString();

                TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = item.itemName;
            }

            // Kibrit sayacını güncelle
            UpdateMatchCounter();
        }

        private void UpdateMatchCounter()
        {
            if (matchItemData == null || InventoryManager.Instance == null) return;

            int matchCount = InventoryManager.Instance.GetItemCount(matchItemData);

            if (matchCountText != null)
            {
                matchCountText.text = $"Kibrit: {matchCount}";

                // Uyarı rengi
                if (matchCount <= warningThreshold && matchCount > 0)
                {
                    matchCountText.color = warningColor;
                }
                else
                {
                    matchCountText.color = Color.white;
                }
            }

            // Uyarı ikonu
            if (matchWarningIcon != null)
            {
                matchWarningIcon.gameObject.SetActive(matchCount <= warningThreshold && matchCount > 0);
            }
        }
    }
}
