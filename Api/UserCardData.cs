using TcgEngine;

namespace Monarchs.Api
{
    [System.Serializable]
    public class UserCardData
    {
        public string tid;
        public int quantity;

        public static string GetTid(string cardID, VariantData variant)
        {
            if (!variant.is_default)
                return cardID + variant.GetSuffix();
            return cardID;
        }

        public static CardData GetCardData(string tid)
        {
            return CardData.Get(GetCardId(tid));
        }

        public static string GetCardId(string tid)
        {
            foreach (VariantData variant in VariantData.GetAll())
            {
                string suffix = variant.GetSuffix();
                if (tid != null && tid.EndsWith(suffix))
                    return tid.Replace(suffix, "");
            }
            return tid;
        }

        public static VariantData GetCardVariant(string tid)
        {
            foreach (VariantData variant in VariantData.GetAll())
            {
                string suffix = variant.GetSuffix();
                if (tid != null && tid.EndsWith(suffix))
                    return variant;
            }
            return VariantData.GetDefault();
        }
    }
}