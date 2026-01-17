using UnityEngine;
using UnityEngine.Events;
using Entropy.Survival;

namespace Entropy.Systems
{
    /// <summary>
    /// Crafting işlemlerini yönetir (Ateş yakma, rüzgar etkisi, kibrit tüketimi)
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance;

        [Header("References")]
        public InventoryManager inventoryManager;
        public FireSystem targetFireSystem; // Hangi ateş sistemine crafting yapılacak

        [Header("Recipes")]
        public CraftingRecipe[] availableRecipes;

        [Header("Events")]
        public UnityEvent<CraftingRecipe> onCraftStart;
        public UnityEvent<CraftingRecipe, bool> onCraftComplete; // (recipe, success)

        [Header("Debug")]
        public bool showDebugLogs = true;

        private bool _isCrafting = false;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (inventoryManager == null)
                inventoryManager = InventoryManager.Instance;
        }

        /// <summary>
        /// Bir tarifi craft etmeye çalış
        /// </summary>
        public void AttemptCraft(CraftingRecipe recipe)
        {
            if (_isCrafting)
            {
                if (showDebugLogs)
                    Debug.LogWarning("Zaten bir crafting işlemi devam ediyor!");
                return;
            }

            if (recipe == null)
            {
                if (showDebugLogs)
                    Debug.LogError("Tarif null!");
                return;
            }

            // Malzeme kontrolü
            if (!CanCraft(recipe))
            {
                if (showDebugLogs)
                    Debug.Log("<color=red>Yetersiz malzeme!</color>");
                return;
            }

            // Crafting başlat
            StartCoroutine(CraftingProcess(recipe));
        }

        /// <summary>
        /// Tarif için yeterli malzeme var mı?
        /// </summary>
        public bool CanCraft(CraftingRecipe recipe)
        {
            if (inventoryManager == null) return false;

            foreach (var ingredient in recipe.requiredItems)
            {
                if (!inventoryManager.HasItem(ingredient.item, ingredient.amount))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Crafting süreci (Coroutine)
        /// </summary>
        private System.Collections.IEnumerator CraftingProcess(CraftingRecipe recipe)
        {
            _isCrafting = true;
            onCraftStart?.Invoke(recipe);

            if (showDebugLogs)
                Debug.Log($"<color=yellow>{recipe.recipeName} yapılıyor...</color>");

            // Crafting süresi bekle
            yield return new WaitForSeconds(recipe.craftingTime);

            // Başarı şansını hesapla (rüzgar etkisi)
            float windIntensity = GameManager.Instance != null ? GameManager.Instance.ruzgarAralığı : 0f;
            float successChance = recipe.CalculateSuccessChance(windIntensity);

            if (showDebugLogs)
                Debug.Log($"Başarı şansı: %{successChance * 100f:F0} (Rüzgar: {windIntensity:F2})");

            // Zar at
            bool success = Random.value <= successChance;

            // Malzemeleri tüket (başarısız olsa bile kibrit harcanır!)
            if (recipe.consumesItems)
            {
                foreach (var ingredient in recipe.requiredItems)
                {
                    inventoryManager.RemoveItem(ingredient.item, ingredient.amount);
                }
            }

            // Sonuç
            if (success)
            {
                if (showDebugLogs)
                    Debug.Log($"<color=green>✓ {recipe.recipeName} BAŞARILI!</color>");

                // Ateş sistemini aktive et
                if (targetFireSystem != null)
                {
                    targetFireSystem.LightFire();
                }
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"<color=red>✗ {recipe.recipeName} BAŞARISIZ! Rüzgar ateşi söndürdü.</color>");
            }

            onCraftComplete?.Invoke(recipe, success);
            _isCrafting = false;
        }

        /// <summary>
        /// Belirli bir recipe'nin başarı şansını döndür (UI için)
        /// </summary>
        public float GetSuccessChance(CraftingRecipe recipe)
        {
            if (recipe == null) return 0f;
            float windIntensity = GameManager.Instance != null ? GameManager.Instance.ruzgarAralığı : 0f;
            return recipe.CalculateSuccessChance(windIntensity);
        }
    }
}
