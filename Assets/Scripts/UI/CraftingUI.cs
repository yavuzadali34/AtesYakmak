using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entropy.Systems;

namespace Entropy.UI
{
    /// <summary>
    /// Crafting panelini yönetir (Ateş yakma UI'ı)
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject craftingPanel;
        public Button craftButton;
        public TextMeshProUGUI recipeNameText;
        public TextMeshProUGUI successChanceText;
        public Image windIndicator;

        [Header("Ingredient Display")]
        public Transform ingredientContainer;
        public GameObject ingredientSlotPrefab;

        [Header("Current Recipe")]
        public CraftingRecipe currentRecipe;

        [Header("Settings")]
        public KeyCode openKey = KeyCode.F; // F tuşu ile crafting aç
        public Color highChanceColor = Color.green;
        public Color mediumChanceColor = Color.yellow;
        public Color lowChanceColor = Color.red;

        void Start()
        {
            if (craftingPanel != null)
                craftingPanel.SetActive(false);

            if (craftButton != null)
                craftButton.onClick.AddListener(OnCraftButtonClicked);
        }

        void Update()
        {
            // F tuşu ile crafting paneli aç/kapa
            if (Input.GetKeyDown(openKey))
            {
                ToggleCraftingPanel();
            }

            // Panel açıksa sürekli güncelle (rüzgar değişebilir)
            if (craftingPanel != null && craftingPanel.activeSelf)
            {
                UpdateSuccessChance();
            }
        }

        public void ToggleCraftingPanel()
        {
            if (craftingPanel != null)
            {
                craftingPanel.SetActive(!craftingPanel.activeSelf);
                if (craftingPanel.activeSelf)
                {
                    RefreshUI();
                }
            }
        }

        private void RefreshUI()
        {
            if (currentRecipe == null) return;

            // Recipe adı
            if (recipeNameText != null)
                recipeNameText.text = currentRecipe.recipeName;

            // Malzeme listesi
            RefreshIngredients();

            // Başarı şansı
            UpdateSuccessChance();

            // Craft butonu aktif mi?
            UpdateCraftButton();
        }

        private void RefreshIngredients()
        {
            // Eski ingredient slot'ları temizle
            foreach (Transform child in ingredientContainer)
            {
                Destroy(child.gameObject);
            }

            if (currentRecipe == null || InventoryManager.Instance == null) return;

            // Her ingredient için slot oluştur
            foreach (var ingredient in currentRecipe.requiredItems)
            {
                GameObject slot = Instantiate(ingredientSlotPrefab, ingredientContainer);

                // Icon
                Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && ingredient.item.icon != null)
                    icon.sprite = ingredient.item.icon;

                // Amount text
                int currentAmount = InventoryManager.Instance.GetItemCount(ingredient.item);
                TextMeshProUGUI amountText = slot.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
                if (amountText != null)
                {
                    amountText.text = $"{currentAmount}/{ingredient.amount}";

                    // Yetersiz malzeme kırmızı
                    if (currentAmount < ingredient.amount)
                        amountText.color = Color.red;
                    else
                        amountText.color = Color.white;
                }

                // Name
                TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = ingredient.item.itemName;
            }
        }

        private void UpdateSuccessChance()
        {
            if (currentRecipe == null || CraftingManager.Instance == null) return;

            float chance = CraftingManager.Instance.GetSuccessChance(currentRecipe);

            if (successChanceText != null)
            {
                successChanceText.text = $"Başarı Şansı: %{chance * 100f:F0}";

                // Renk kodlama
                if (chance >= 0.7f)
                    successChanceText.color = highChanceColor;
                else if (chance >= 0.4f)
                    successChanceText.color = mediumChanceColor;
                else
                    successChanceText.color = lowChanceColor;
            }

            // Rüzgar göstergesi
            if (windIndicator != null && GameManager.Instance != null)
            {
                float windIntensity = GameManager.Instance.ruzgarAralığı;
                windIndicator.fillAmount = windIntensity;

                // Rüzgar yüksekse kırmızı
                Color windColor = Color.Lerp(Color.green, Color.red, windIntensity);
                windIndicator.color = windColor;
            }
        }

        private void UpdateCraftButton()
        {
            if (craftButton == null || CraftingManager.Instance == null) return;

            // Malzeme yeterli mi kontrol et
            bool canCraft = CraftingManager.Instance.CanCraft(currentRecipe);
            craftButton.interactable = canCraft;
        }

        private void OnCraftButtonClicked()
        {
            if (CraftingManager.Instance != null && currentRecipe != null)
            {
                CraftingManager.Instance.AttemptCraft(currentRecipe);

                // Crafting sonrası UI'ı güncelle
                Invoke(nameof(RefreshUI), 0.1f);
            }
        }

        /// <summary>
        /// Dışarıdan recipe değiştirmek için
        /// </summary>
        public void SetRecipe(CraftingRecipe recipe)
        {
            currentRecipe = recipe;
            RefreshUI();
        }
    }
}
