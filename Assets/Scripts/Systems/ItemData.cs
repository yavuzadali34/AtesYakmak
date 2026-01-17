using UnityEngine;

namespace Entropy.Systems
{
    /// <summary>
    /// ScriptableObject: Oyundaki tüm eşyaların verilerini tutar (Kibrit, Odun, vs.)
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Entropy/Item Data")]
    public class ItemData : ScriptableObject
    {
        public enum ItemType
        {
            Wood,       // Odun (Ateş için yakıt)
            Match,      // Kibrit (Sınırlı kaynak)
            Kindling,   // Çalı/Çırpı (Tutuşturma malzemesi)
            Tinder      // Kuru ot (İlk kıvılcım için)
        }

        [Header("Item Info")]
        public ItemType type;
        public string itemName = "New Item";
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Stack Settings")]
        public bool isStackable = true;
        public int maxStackSize = 99;

        [Header("Gameplay")]
        public float weight = 1.0f;
        public int fireValue = 0; // Ateşe ne kadar süre ekler (saniye)
    }
}
