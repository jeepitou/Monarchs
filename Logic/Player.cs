using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Client;
using TcgEngine;
using UnityEngine;
using UnityEngine.Serialization;
using ChessTCG.Logic;

namespace Monarchs.Logic
{
    //Represent the current state of a player during the game (data only)

    [System.Serializable]
    public class Player: ITargetable
    {
        public int playerID;
        public string username;
        public string avatar;
        public string cardback;
        public string deck;
        public bool is_ai = false;
        public bool submittedMulligan = false;
        public int ai_level;

        public bool connected = false; //Connected to server and game
        public bool ready = false;     //Sent all player data, ready to play
        
        public PlayerMana playerMana;
        public int kill_count = 0;

        public Dictionary<string, Card> cards_all = new Dictionary<string, Card>();
        public Card king = null;

        public List<Card> cards_deck = new List<Card>();
        public List<Card> cards_hand = new List<Card>();
        public List<Card> cards_board = new List<Card>();
        public List<Card> cards_discard = new List<Card>();
        public List<Card> cards_trap = new List<Card>();
        public List<Card> cards_temp = new List<Card>();

        public List<CardTrait> traits = new List<CardTrait>();
        public List<CardTrait> ongoing_traits = new List<CardTrait>();

        public List<CardStatus> ongoing_status = new List<CardStatus>();
        public List<CardStatus> status_effects = new List<CardStatus>();

        public Player(int id)
        {
            this.playerID = id;
            playerMana = new PlayerMana();
        }

        public bool IsReady() { return ready && cards_all.Count > 0; }
        public bool IsConnected() { return connected || is_ai; }

        public virtual void ClearOngoing() { ongoing_status.Clear(); ongoing_traits.Clear(); }

        //---- Cards ---------

        public void AddCard(List<Card> card_list, Card card)
        {
            card_list.Add(card);
        }

        public Slot GetSlot()
        {
            return Slot.None;
        }

        public bool CanBeTargeted()
        {
            return true;
        }

        public int GetPlayerId()
        {
            return playerID;
        }

        public void RemoveCard(List<Card> card_list, Card card)
        {
            card_list.Remove(card);
        }

        public virtual void RemoveCardFromAllGroups(Card card)
        {
            cards_deck.Remove(card);
            cards_hand.Remove(card);
            cards_board.Remove(card);
            cards_deck.Remove(card);
            cards_discard.Remove(card);
            cards_trap.Remove(card);
            cards_temp.Remove(card);
        }

        public int GetTotalSquaresControlled(Game game)
        {
            int square = 0;
            foreach (var piece in cards_board)
            {
                square += piece.GetLegalMoves(game).Length;
            }

            return square;
        }
        
        public int GetTotalSquaresAttackable(Game game)
        {
            int square = 0;
            foreach (var piece in cards_board)
            {
                Vector2S[] legalMoves = piece.GetLegalMoves(game);
                foreach (var move in legalMoves)
                {
                    if (GameClient.GetGameData().GetSlotCard(Slot.Get(move.x, move.y)) != null)
                    {
                        square += 1;
                    }
                }
            }

            return square;
        }
        
        public virtual Card GetRandomCard(List<Card> card_list, System.Random rand)
        {
            if (card_list.Count > 0)
                return card_list[rand.Next(0, card_list.Count)];
            return null;
        }

        public virtual int GetQuantityOfDestroyedCard(Card destroyedCard, bool sameCohort = false)
        {
            int quantity = 0;

            foreach (var card in cards_discard)
            {
                if (card.cardID == destroyedCard.cardID && card.wasOnBoard)
                {
                    if (sameCohort && card.CohortUid != destroyedCard.CohortUid)
                        continue;
                    quantity += 1 + card.numberOfCohortUnitDied;
                }
            }

            return quantity;
        }

        public Card UnitOfCohortInDiscard(Card cohortCard)
        {
            if (cohortCard.CohortUid == "")
            {
                return null;
            }
            
            foreach (var card in cards_discard)
            {
                if (card.CohortUid == cohortCard.CohortUid)
                {
                    return card;
                }
            }

            return null;
        }

        public bool HasCard(List<Card> card_list, Card card)
        {
            return card_list.Contains(card);
        }

        public Card GetHandCard(string uid)
        {
            foreach (Card card in cards_hand)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetBoardCard(string uid)
        {
            foreach (Card card in cards_board)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetDeckCard(string uid)
        {
            foreach (Card card in cards_deck)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetDiscardCard(string uid)
        {
            foreach (Card card in cards_discard)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetSlotCard(Slot slot)
        {
            foreach (Card card in cards_board)
            {
                if (card != null && card.slot == slot)
                    return card;
            }
            return null;
        }

        public Card GetCard(string uid)
        {
            if (uid != null)
            {
                bool valid = cards_all.TryGetValue(uid, out Card card);
                if (valid)
                    return card;
            }
            return null;
        }

        public bool IsOnBoard(Card card)
        {
            return card != null && GetBoardCard(card.uid) != null;
        }

        //------ Custom Traits/Stats ---------

        public void SetTrait(string id, int value)
        {
            traits.SetTrait(id, value);
        }

        public void AddTrait(string id, int value)
        {
            traits.AddTrait(id, value);
        }

        public void AddOngoingTrait(string id, int value)
        {
            ongoing_traits.AddTrait(id, value);
        }

        public void RemoveTrait(string id)
        {
            traits.RemoveTrait(id);
        }

        public CardTrait GetTrait(string id)
        {
            foreach (CardTrait trait in traits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        public CardTrait GetOngoingTrait(string id)
        {
            foreach (CardTrait trait in ongoing_traits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        public List<CardTrait> GetAllTraits()
        {
            List<CardTrait> all_traits = new List<CardTrait>();
            all_traits.AddRange(traits);
            all_traits.AddRange(ongoing_traits);
            return all_traits;
        }

        public int GetTraitValue(TraitData trait)
        {
            if (trait != null)
                return GetTraitValue(trait.id);
            return 0;
        }

        public virtual int GetTraitValue(string id)
        {
            int val = 0;
            CardTrait stat1 = GetTrait(id);
            CardTrait stat2 = GetOngoingTrait(id);
            if (stat1 != null)
                val += stat1.value;
            if (stat2 != null)
                val += stat2.value;
            return val;
        }

        public bool HasTrait(TraitData trait)
        {
            if (trait != null)
                return HasTrait(trait.id);
            return false;
        }

        public bool HasTrait(string id)
        {
            foreach (CardTrait trait in traits)
            {
                if (trait.id == id)
                    return true;
            }
            return false;
        }

        //---- Status ---------

        public void AddStatus(StatusData status, int value, int duration)
        {
            if (status != null)
                AddStatus(status.effect, value, duration);
        }

        public void AddOngoingStatus(StatusData status, int value)
        {
            if (status != null)
                AddOngoingStatus(status.effect, value);
        }

        public void AddStatus(StatusType effect, int value, int duration)
        {
            status_effects.AddStatus(new CardStatus(effect, value, duration));
        }

        public void AddOngoingStatus(StatusType effect, int value)
        {
            ongoing_status.AddStatus(new CardStatus(effect, value, 0));
        }

        public void RemoveStatus(StatusType effect)
        {
            status_effects.RemoveStatus(effect);
        }

        public CardStatus GetStatus(StatusType effect)
        {
            return status_effects.GetStatus(effect);
        }

        public CardStatus GetOngoingStatus(StatusType effect)
        {
            return ongoing_status.GetStatus(effect);
        }

        public bool HasStatusEffect(StatusType effect)
        {
            return status_effects.HasStatus(effect) || ongoing_status.HasStatus(effect);
        }

        public virtual int GetStatusEffectValue(StatusType effect)
        {
            CardStatus status1 = GetStatus(effect);
            CardStatus status2 = GetOngoingStatus(effect);
            return status1.value + status2.value;
        }
        
        //---- Action Check ---------

        public virtual bool CanPayMana(Card card, bool isMonarchTurn)
        {
            return playerMana.HasManaForCard(card, isMonarchTurn);
        }

        public virtual void PayMana(Card card, PlayerMana.ManaType selectorManaType)
        {
            playerMana.SpendMana(card);
            playerMana.SpendMana(selectorManaType);
        }

        public virtual bool CanPayAbility(Card card, AbilityData ability)
        {
            return playerMana.HasMana(ability.mana_cost);
        }

        public virtual bool IsDead()
        {
            if (!IsOnBoard(king))
                return true;
            return false;
        }

        //--------------------

        //Clone all player variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Player source, Player dest)
        {
            dest.playerID = source.playerID;
            dest.is_ai = source.is_ai;
            dest.ai_level = source.ai_level;
            //dest.username = source.username;
            //dest.avatar = source.avatar;
            //dest.deck = source.deck;
            //dest.connected = source.connected;
            //dest.ready = source.ready;

            dest.playerMana = source.playerMana.Clone();
            dest.kill_count = source.kill_count;

            Card.CloneNull(source.king, ref dest.king);
            Card.CloneDict(source.cards_all, dest.cards_all);
            Card.CloneListRef(dest.cards_all, source.cards_board, dest.cards_board);
            Card.CloneListRef(dest.cards_all, source.cards_hand, dest.cards_hand);
            Card.CloneListRef(dest.cards_all, source.cards_deck, dest.cards_deck);
            Card.CloneListRef(dest.cards_all, source.cards_discard, dest.cards_discard);
            Card.CloneListRef(dest.cards_all, source.cards_trap, dest.cards_trap);
            Card.CloneListRef(dest.cards_all, source.cards_temp, dest.cards_temp);

            // Card lists (CloneListRef) remain unchanged for now
            source.status_effects.CloneList(dest.status_effects);
            source.ongoing_status.CloneList(dest.ongoing_status);
        }
    }
}
