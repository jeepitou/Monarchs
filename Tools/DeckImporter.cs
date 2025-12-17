using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Monarchs.Api;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Tools
{
    public static class DeckImporter
    {
        [Serializable]
        private class DeckJson
        {
            public string monarch;
            public string champion;
            public string[] cards;
        }
        
        public static UserDeckData ImportDeck(string importString, string title)
        {
            if (string.IsNullOrEmpty(importString))
            {
                Debug.LogError("Deck import failed: empty string.");
                return null;
            }
            
            DeckJson deckJson = ImportDeckToJson(importString);
            if (deckJson == null)
            {
                return null;
            }
            
            UserDeckData userDeck = new UserDeckData();
            userDeck.tid = Guid.NewGuid().ToString();
            userDeck.title = title;
            userDeck.monarch = deckJson.monarch;
            userDeck.champion = deckJson.champion;
            userDeck.cards = new List<string>(deckJson.cards).ToArray();
            return userDeck;
        }

        /// <summary>
        /// Takes the Base64+Gzip encoded importString and returns a generated DeckData ScriptableObject.
        /// </summary>
        public static DeckData ImportDeck(string importString)
        {
            if (string.IsNullOrEmpty(importString))
            {
                Debug.LogError("Deck import failed: empty string.");
                return null;
            }

            try
            {
                DeckJson deckJson = ImportDeckToJson(importString);

                // STEP 4 — Create ScriptableObject
                DeckData deck = ScriptableObject.CreateInstance<DeckData>();
                deck.monarch = CardData.Get(deckJson.monarch);
                deck.champion = CardData.Get(deckJson.champion);
                deck.cards = CardData.GetList(new List<string>(deckJson.cards)).ToArray();

                return deck;
            }
            catch (Exception ex)
            {
                Debug.LogError("Deck import failed: " + ex);
                return null;
            }
        }
        
        private static DeckJson ImportDeckToJson(string importString)
        {
            if (string.IsNullOrEmpty(importString))
            {
                Debug.LogError("Deck import failed: empty string.");
                return null;
            }

            try
            {
                byte[] compressedBytes = Convert.FromBase64String(importString);

                string json = DecompressGzip(compressedBytes);

                return JsonUtility.FromJson<DeckJson>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError("Deck import failed: " + ex);
                return null;
            }
        }

        private static string DecompressGzip(byte[] gzipData)
        {
            
            using (var compressedStream = new MemoryStream(gzipData))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}