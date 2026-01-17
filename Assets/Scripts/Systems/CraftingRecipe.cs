using UnityEngine;
using System.Collections.Generic;

namespace Entropy.Systems
{
    /// <summary>
    /// ScriptableObject: Crafting tarifleri (Ateş yakmak için gereken malzemeler)
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Entropy/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [System.Serializable]
        public class Ingredient
        {
            public ItemData item;
            public int amount = 1;
        }

        [Header("Recipe Info")]
        public string recipeName = "Build Fire";
        [TextArea(2, 4)]
        public string description;

        [Header("Requirements")]
        public List<Ingredient> requiredItems = new List<Ingredient>();

        [Header("Wind Sensitivity")]
        [Range(0f, 1f)]
        public float windResistance = 0.5f; // 0 = çok hassas, 1 = rüzgar etkilemez

        [Header("Output (Optional)")]
        public bool consumesItems = true; // Malzemeler tüketilsin mi?
        public float craftingTime = 2.0f; // Crafting süresi (saniye)

        /// <summary>
        /// Bu tarifin başarı şansını hesapla (rüzgar faktörüne göre)
        /// </summary>
        public float CalculateSuccessChance(float windIntensity)
        {
            // Rüzgar ne kadar yüksekse, başarı o kadar düşük
            float windPenalty = windIntensity * (1f - windResistance);
            float baseChance = 0.9f; // %90 temel şans
            float finalChance = baseChance - (windPenalty * 0.7f); // Max %70 düşüş
            return Mathf.Clamp01(finalChance);
        }
    }
}
