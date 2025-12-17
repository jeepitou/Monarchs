using TcgEngine;

namespace Monarchs.Api
{
    [System.Serializable]
    public class UserDeckData
    {
        public string tid;
        public string title;
        public string monarch;
        public string champion;
        public string[] cards;

        public int GetQuantity()
        {
            return cards.Length;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(tid) && !string.IsNullOrWhiteSpace(title) && cards.Length >= GameplayData.Get().deck_size;
        }
    }
}