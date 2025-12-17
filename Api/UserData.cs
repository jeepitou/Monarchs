using System.Collections.Generic;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Api
{
    /// <summary>
    /// Contain UserData retrieved from the web api database
    /// </summary>
    
    [System.Serializable]
    public class UserData
    {
        public string id;
        public string username;

        public string email;
        public string avatar;
        public string cardback;
        public int permissionLevel = 1;
        public int validationLevel = 1;

        public int coins;
        public int xp;
        public int elo;

        public int matches;
        public int victories;
        public int defeats;

        public UserCardData[] cards;
        public UserCardData[] packs;
        public UserDeckData[] decks;
        public string[] rewards;
        public string[] avatars;
        public string[] cardbacks;
        public string[] friends;

        public UserData()
        {
            cards = new UserCardData[0];
            packs = new UserCardData[0];
            decks = new UserDeckData[0];
            rewards = new string[0];
            avatars = new string[0];
            cardbacks = new string[0];
            friends = new string[0];
        }

        public int GetLevel()
        {
            return Mathf.FloorToInt((float)xp/1000) + 1;
        }

        public string GetAvatar()
        {
            if (avatar != null)
                return avatar;
            return "";
        }

        public string GetCardback()
        {
            if (cardback != null)
                return cardback;
            return "";
        }

        public void SetDeck(UserDeckData deck)
        {
            for(int i=0; i<decks.Length; i++)
            {
                if (decks[i].tid == deck.tid)
                {
                    decks[i] = deck;
                    return;
                }
            }

            List<UserDeckData> ldecks = new List<UserDeckData>(decks);
            ldecks.Add(deck);
            decks = ldecks.ToArray();
        }

        public UserDeckData GetDeck(string tid)
        {
            foreach (UserDeckData deck in decks)
            {
                if (deck.tid == tid)
                    return deck;
            }
            return null;
        }

        public UserCardData GetCard(string tid)
        {
            foreach (UserCardData card in cards)
            {
                if (card.tid == tid)
                    return card;
            }
            return null;
        }

        public int GetCardQuantity(string tid)
        {
            if (cards == null)
                return 0;

            foreach (UserCardData card in cards)
            {
                if (card.tid == tid)
                    return card.quantity;
            }
            return 0;
        }

        public UserCardData GetPack(string tid)
        {
            foreach (UserCardData pack in packs)
            {
                if (pack.tid == tid)
                    return pack;
            }
            return null;
        }

        public int GetPackQuantity(string tid)
        {
            if (packs == null)
                return 0;

            foreach (UserCardData pack in packs)
            {
                if (pack.tid == tid)
                    return pack.quantity;
            }
            return 0;
        }

        public int CountUniqueCards()
        {
            if (cards == null)
                return 0;

            HashSet<string> uniqueCards = new HashSet<string>();
            foreach (UserCardData card in cards)
            {
                string cardID = UserCardData.GetCardId(card.tid);
                if (!uniqueCards.Contains(cardID))
                    uniqueCards.Add(cardID);
            }
            return uniqueCards.Count;
        }

        public int CountCardType(VariantData variant)
        {
            int value = 0;
            foreach (UserCardData card in cards)
            {
                if (UserCardData.GetCardVariant(card.tid) == variant)
                    value += 1;
            }
            return value;
        }

        public bool HasDeckCards(UserDeckData deck)
        {
            return true; // COLLECTION NOT YET IMPLEMENTED
            Dictionary<string, int> deckCardQuantity = new Dictionary<string, int>();
            foreach (string cardID in deck.cards)
            {
                if (!deckCardQuantity.ContainsKey(cardID))
                    deckCardQuantity[cardID] = 1;
                else
                    deckCardQuantity[cardID] += 1;
            }

            foreach (KeyValuePair<string, int> pair in deckCardQuantity)
            {
                if (GetCardQuantity(pair.Key) < pair.Value)
                    return false;
            }

            return true;
        }

        public bool IsDeckValid(UserDeckData deck)
        {
            if (Authenticator.Get().IsApi())
                return HasDeckCards(deck) && deck.IsValid();
            return deck.IsValid();
        }

        public void AddPack(string tid, int quantity)
        {
            bool found = false;
            foreach (UserCardData pack in packs)
            {
                if (pack.tid == tid)
                {
                    found = true;
                    pack.quantity += quantity;
                }
            }
            if (!found)
            {
                UserCardData npack = new UserCardData();
                npack.tid = tid;
                npack.quantity = quantity;
                List<UserCardData> apacks = new List<UserCardData>(packs);
                apacks.Add(npack);
                packs = apacks.ToArray();
            }
        }

        public void AddCard(string tid, int quantity)
        {
            bool found = false;
            foreach (UserCardData card in cards)
            {
                if (card.tid == tid)
                {
                    found = true;
                    card.quantity += quantity;
                }
            }
            if (!found)
            {
                UserCardData npack = new UserCardData();
                npack.tid = tid;
                npack.quantity = quantity;
                List<UserCardData> acards = new List<UserCardData>(cards);
                acards.Add(npack);
                cards = acards.ToArray();
            }
        }

        public void AddReward(string tid)
        {
            if (!HasReward(tid))
            {
                List<string> arewards = new List<string>(rewards);
                arewards.Add(tid);
                rewards = arewards.ToArray();
            }
        }

        public bool HasCard(string cardTid, int quantity = 1)
        {
            foreach (UserCardData card in cards)
            {
                if (card.tid == cardTid && card.quantity >= quantity)
                    return true;
            }
            return false;
        }

        public bool HasPack(string packTid, int quantity=1)
        {
            foreach (UserCardData pack in packs)
            {
                if (pack.tid == packTid && pack.quantity >= quantity)
                    return true;
            }
            return false;
        }

        public bool HasReward(string rewardID)
        {
            foreach (string reward in rewards)
            {
                if (reward == rewardID)
                    return true;
            }
            return false;
        }

        public string GetCoinsString()
        {
            return coins.ToString();
        }

        public bool HasFriend(string friendUserName)
        {
            List<string> flist = new List<string>(friends);
            return flist.Contains(friendUserName);
        }

        public void AddFriend(string friendUserName)
        {
            List<string> flist = new List<string>(friends);
            if (!flist.Contains(friendUserName))
                flist.Add(friendUserName);
            friends = flist.ToArray();
        }

        public void RemoveFriend(string friendUserName)
        {
            List<string> flist = new List<string>(friends);
            if (flist.Contains(friendUserName))
                flist.Remove(friendUserName);
            friends = flist.ToArray();
        }
    }
}

