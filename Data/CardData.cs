using System;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using TcgEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Monarchs
{
    public enum CardType
    {
        None = 0,
        Hero = 5,
        Character = 10,
        Spell = 20,
        Artifact = 30,
        Trap = 40,
    }

    public interface ICard
    {
        PieceType GetPieceType();
        int GetArmor();
        int GetHP();
        int GetCohortSize();
        int GetInitiative();
        int GetMoveRange();
        int GetAttack();
        SubtypeData[] GetSubtypes();
        PlayerMana.ManaType GetManaCost();
        bool HasArmorPenetration();
        int GetMaxAttackRange();
        int GetMinAttackRange();
        CardData GetCardData();
        public AbilityData[] GetAllCurrentAbilities();
    }

    /// <summary>
    /// Defines all card data
    /// </summary>

    [CreateAssetMenu(fileName = "card", menuName = "TcgEngine/CardData", order = 5)]
    public class CardData : ScriptableObject, ICard
    {
        public string id;

        [Header("ChessTCG")] 
        public PieceModel[] pieceModels;
        [SerializeField,SingleEnumFlagSelect(EnumType = typeof(PieceType))]
        [ShowIf("IsCharacter")] public PieceType type;

        public bool overrideMovementScheme;
        [ShowIf("overrideMovementScheme")] public MovementScheme newMovementScheme;
        public bool IsKing => type == PieceType.Monarch;
        [ShowIf("IsKing")] public PlayerMana.ManaType manaTypeThatCanBeGenerated;
        [EnumToggleButtons]
        public PieceType possibleCasters;
        [EnumToggleButtons]
        public PlayerMana.ManaType manaCost;
        [ShowIf("IsCharacter")] public int cohortSize;
        [ShowIf("IsCharacter")] public DeployChoices deployType = DeployChoices.Default;
        [ShowIf("IsCharacter")] public CardData[] boundSpells;

        public MovementScheme MovementScheme
        {
            get
            {
                if (overrideMovementScheme)
                {
                    return newMovementScheme;
                }
                MovementPieceTypeLink pieceTypeLink = GameplayData.Get().MovementPieceTypeLink;
                return pieceTypeLink.GetMovementScheme(this);
            }
        }

        public bool IsPawn => type == PieceType.Pawn;

        [ShowIf("IsPawn")]
        public MovementScheme promoteMovementScheme;
        [ShowIf("IsPawn")] 
        public int promoteMoveRange;

        [ShowIf("IsCharacter")] public int moveRange;
        [SerializeField]private bool jumping;

        [Header("Display")]
        public string title;
        public Sprite artFull;
        public Sprite artBoard;
        public Sprite[] aoePatterns;

        [FormerlySerializedAs("type")] [Header("Stats")]
        public CardType cardType;
        public bool IsSpell => cardType == CardType.Spell;
        [SerializeField,SingleEnumFlagSelect(EnumType = typeof(InterventionType))]
        [ShowIf("IsSpell")] 
        public InterventionType interventionType;
        public GuildData guild;
        public SubtypeData[] subtypes;
        public RarityData rarity;
        [ShowIf("IsCharacter")] public int initiative;
        [ShowIf("IsCharacter")] public int musterTime;

        [ShowIf("IsCharacter")] public int attack;
        public EffectSplashDamage meleeSplashDamageForHighlights;
        
        [ShowIf("HasRangedAttack")] public int minAttackRange;
        public int maxAttackRange;
        public EffectSplashDamage rangedSplashDamageForHighlights;
        public bool HasRangedAttack => maxAttackRange > 0;

        [ShowIf("IsCharacter")] public int hp;

        [Header("Traits")]
        [ShowIf("IsCharacter")] public TraitData[] traits;
        [ShowIf("IsCharacter")] public TraitStat[] stats;

        [Header("Abilities")]
        [SerializeField]protected AbilityData[] abilities;

        private bool PawnCanCast => differentSpellPerMovement && possibleCasters.HasFlag(PieceType.Pawn);
        private bool RookCanCast => differentSpellPerMovement && possibleCasters.HasFlag(PieceType.Rook);
        private bool KnightCanCast => differentSpellPerMovement && possibleCasters.HasFlag(PieceType.Knight);
        private bool BishopCanCast => differentSpellPerMovement && possibleCasters.HasFlag(PieceType.Bishop);
        private bool ChampionCanCast => differentSpellPerMovement && possibleCasters.HasFlag(PieceType.Champion);
        private bool MonarchCanCast => differentSpellPerMovement && possibleCasters.HasFlag(PieceType.Monarch);

        public bool differentSpellPerMovement;
        [ShowIf("PawnCanCast")] public CardData pawnSpell;
        [ShowIf("RookCanCast")] public CardData rookSpell;
        [ShowIf("KnightCanCast")] public CardData knightSpell;
        [ShowIf("BishopCanCast")] public CardData bishopSpell;
        [ShowIf("ChampionCanCast")] public CardData championSpell;
        [ShowIf("MonarchCanCast")] public CardData monarchSpell;

        [Header("Card Text")]
        [TextArea(3, 5)]
        public string text;

        [Header("Description")]
        [TextArea(5, 10)]
        public string desc;

        [Header("FX")]
        [SerializeField] private GameObject spawnFX;
        [SerializeField] private GameObject deathFX;
        [SerializeField] private GameObject attackFX;
        [SerializeField] private GameObject rangeAttackFX;
        [SerializeField] private GameObject damageFX;
        [SerializeField] private GameObject idleFX;
        [SerializeField] private GameObject trapOnGroundFX;
        [SerializeField] private GameObject trapTriggeredFX;
        [SerializeField] private AudioClip spawnAudio;
        [SerializeField] private AudioClip deathAudio;
        [SerializeField] private AudioClip attackAudio;
        [SerializeField] private AudioClip damageAudio;
        

        [Header("Availability")]
        public bool deckBuilding;
        public int cost = 100;
        public PackData[] packs;

        public static List<CardData> CardList = new ();
        private static List<CardData> _interventionCardList = new ();

        public static void Load(string folder = "")
        {
            if (CardList.Count == 0)
                CardList.AddRange(Resources.LoadAll<CardData>(folder));
        }

        public Sprite GetBoardArt(VariantData variant)
        {
            return artBoard;
        }

        public Slot GetSlot()
        {
            return Slot.None;
        }
        
        //Used mainly for tests
        public void SetAbilities(AbilityData[] newAbilities)
        {
            abilities = newAbilities;
        }

        public Sprite GetFullArt(VariantData variant)
        {
            return artFull;
        }

        public string GetTitle()
        {
            return title;
        }

        public string GetText()
        {
            return text;
        }

        public SubtypeData[] GetSubtypes()
        {
            return subtypes;
        }

        public PieceType GetPieceType()
        {
            return type;
        }

        public virtual int GetArmor()
        {
            int permanentArmor = 0;
            if (HasStat("armor"))
            {
                permanentArmor = GetStat("armor");
            }

            return permanentArmor;
        }

        public int GetHP()
        {
            return hp;
        }

        public int GetCohortSize()
        {
            return cohortSize;
        }

        public int GetInitiative()
        {
            return initiative;
        }

        public int GetMoveRange()
        {
            return moveRange;
        }

        public int GetAttack()
        {
            return attack;
        }

        public PlayerMana.ManaType GetManaCost()
        {
            return manaCost;
        }

        public bool HasArmorPenetration()
        {
            return HasStat("armor_penetration");
        }

        public int GetMaxAttackRange()
        {
            return maxAttackRange;
        }

        public int GetMinAttackRange()
        {
            return minAttackRange;
        }

        public GuildData GetGuild()
        {
            return guild;
        }

        public ICard GetCardCorrespondingWithPieceType(PieceType typeToGet)
        {
            if (!differentSpellPerMovement)
            {
                return this;
            }
            
            ICard returnValue = null;
            switch (typeToGet)
            {
                case PieceType.Bishop:
                    returnValue = bishopSpell;break;
                case PieceType.Monarch:
                    returnValue =  monarchSpell;break;
                case PieceType.Knight:
                    returnValue =  knightSpell;break;
                case PieceType.Pawn:
                    returnValue =  pawnSpell;break;
                case PieceType.Champion:
                    returnValue =  championSpell;break;
                case PieceType.Rook:
                    returnValue =  rookSpell;break;
            }

            if (returnValue == null)
                return this;

            return returnValue;
        }

        public CardData GetAnyChildCardData()
        {
            if (bishopSpell != null)
                return bishopSpell;
            if (monarchSpell != null)
                return monarchSpell;
            if (knightSpell != null)
                return knightSpell;
            if (pawnSpell != null)
                return pawnSpell;
            if (championSpell != null)
                return championSpell;
            if (rookSpell != null)
                return rookSpell;
            return null;
        }

        public CardData GetCardData()
        {
            return this;
        }

        public AbilityData[] GetAllCurrentAbilities()
        {
            return abilities;
        }

        public string GetDesc()
        {
            return desc;
        }

        public string GetTypeId()
        {
            if (cardType == CardType.Character)
                return "character";
            if (cardType == CardType.Artifact)
                return "artifact";
            if (cardType == CardType.Spell)
                return "spell";
            if (cardType == CardType.Trap)
                return "trap";
            return "";
        }

        public string GetAbilitiesDesc()
        {
            string txt = "";
            foreach (AbilityData ability in abilities)
            {
                if (!string.IsNullOrWhiteSpace(ability.desc) && !ability.DontShowOnPreview)
                    txt += "<b>" + ability.GetTitle() + ":</b> " + ability.GetDesc(this) + "\n";
            }
            return txt;
        }

        public bool IsCharacter()
        {
            return cardType == CardType.Character;
        }

        public bool IsTrap()
        {
            return cardType == CardType.Trap;
        }

        public bool IsBoardCard()
        {
            return cardType == CardType.Character || cardType == CardType.Artifact;
        }

        public bool IsRequireTarget()
        {
            if (differentSpellPerMovement)
            {
                CardData cardData = GetAnyChildCardData();
                return cardData == null ? false : cardData.IsRequireTarget();
            }

            bool isTrap = (cardType == CardType.Trap);
            bool isTargetSpell = (cardType == CardType.Spell &&
                                  (HasAbility(AbilityTrigger.OnPlay, AbilityTargetType.PlayTarget) || HasAbility(AbilityTrigger.OnPlay, AbilityTargetType.PlayTargetAndSelectMultiplesTarget)));
            return isTrap || isTargetSpell;
        }

        public virtual bool HasTrait(string trait)
        {
            foreach (TraitData t in traits)
            {
                if (t.id == trait)
                    return true;
            }
            return false;
        }

        public virtual bool HasTrait(TraitData trait)
        {
            if(trait != null)
                return HasTrait(trait.id);
            return false;
        }

        public bool CanBeTargeted()
        {
            return false;
        }

        public int GetPlayerId()
        {
            return -1;
        }

        public virtual bool IsTeamTraitAndType(GuildData guildToCheck, TraitData trait, SubtypeData subtype,  CardType typeToCheck)
        {
            bool isType = this.cardType == typeToCheck || typeToCheck == CardType.None;
            bool isTeam = this.guild == guildToCheck || guildToCheck == null;
            bool isTrait = HasTrait(trait) || trait == null;
            bool isSubtype = subtype == null || GetSubtypes().Contains(subtype);
            return (isType && isTeam && isTrait && isSubtype);
        }

        public virtual bool HasStat(string trait)
        {
            if (stats == null)
                return false;

            foreach (TraitStat stat in stats)
            {
                if (stat.trait.id == trait)
                    return true;
            }
            return false;
        }

        public virtual bool HasStat(TraitData trait)
        {
            if(trait != null)
                return HasStat(trait.id);
            return false;
        }

        public virtual int GetStat(string traitID)
        {
            if (stats == null)
                return 0;

            foreach (TraitStat stat in stats)
            {
                if (stat.trait.id == traitID)
                    return stat.value;
            }
            return 0;
        }

        public virtual int GetStat(TraitData trait)
        {
            if(trait != null)
                return GetStat(trait.id);
            return 0;
        }

        public virtual bool HasAbility(AbilityTrigger trigger)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == trigger)
                    return true;
            }
            return false;
        }

        public virtual bool HasAbility(AbilityTrigger trigger, AbilityTargetType targetType)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == trigger && ability.targetType == targetType)
                    return true;
            }
            return false;
        }

        public AbilityData GetAbility(AbilityTrigger trigger)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == trigger)
                    return ability;
            }
            return null;
        }

        public virtual bool AreTrapConditionsMet(AbilityTrigger trapTrigger, Game data, Card caster, Card trigger)
        {
            foreach (AbilityData ability in abilities)
            {
                AbilityArgs args = new AbilityArgs() { ability = ability, caster = caster, triggerer = trigger, target = trigger};
                if (ability && ability.trigger == trapTrigger && ability.AreTriggerConditionsMet(data, args))
                    return true;
            }
            return false;
        }

        public int GetAbilitiesAiValue(AbilityTrigger trigger)
        {
            int total = 0;
            foreach (AbilityData ability in abilities)
            {
                if(ability.trigger == trigger)
                    total += ability.GetAiValue();
            }
            return total;
        }

        public int GetAbilitiesAiValue()
        {
            int total = 0;
            foreach (AbilityData ability in abilities)
                total += ability.GetAiValue();
            return total;
        }

        public bool HasPack(PackData packInput)
        {
            foreach (PackData pack in packs)
            {
                if (pack == packInput)
                    return true;
            }
            return false;
        }

        public static CardData Get(string id)
        {
            foreach (CardData card in GetAll())
            {
                if (card.id == id)
                    return card;
            }
            return null;
        }

        public static List<CardData> GetList(List<string> ids)
        {
            List<CardData> cards = new List<CardData>();
            foreach (string id in ids)
            {
                CardData card = Get(id);
                if (card != null)
                    cards.Add(card);
            }
            return cards;
        }

        public static List<CardData> GetAllDeckBuilding()
        {
            List<CardData> multiList = new List<CardData>();
            foreach (CardData acard in GetAll())
            {
                if (acard.deckBuilding)
                    multiList.Add(acard);
            }
            return multiList;
        }

        public static List<CardData> GetAll(PackData pack)
        {
            List<CardData> multiList = new List<CardData>();
            foreach (CardData acard in GetAll())
            {
                if (acard.HasPack(pack))
                    multiList.Add(acard);
            }
            return multiList;
        }
        
        public static List<CardData> GetAllInterventions()
        {
            if (_interventionCardList.Count == 0)
            {
                _interventionCardList.AddRange(
                    GetAll().Where(acard =>
                        (acard.cardType == CardType.Spell || acard.cardType == CardType.Trap) && acard.deckBuilding
                    )
                );
            }

            return _interventionCardList;
        }

        public static List<CardData> GetAll()
        {
            return CardList;
        }
        
        public GameObject SpawnFX => spawnFX ? spawnFX : AssetData.Get().card_spawn_fx; 
        public GameObject DeathFX => deathFX ? deathFX : AssetData.Get().card_destroy_fx;
        public GameObject AttackFX => attackFX ? attackFX : AssetData.Get().card_attack_fx;
        public GameObject RangeAttackFX => rangeAttackFX;
        public GameObject DamageFX => damageFX ? damageFX : AssetData.Get().card_damage_fx;
        public GameObject IdleFX => idleFX;
        public GameObject TrapOnGroundFX => trapOnGroundFX;
        public GameObject TrapTriggeredFX => trapTriggeredFX ? trapTriggeredFX : AssetData.Get().trap_triggered_fx;
        public AudioClip SpawnAudio => spawnAudio ? spawnAudio : AssetData.Get().card_spawn_audio;
        public AudioClip DeathAudio => deathAudio ? deathAudio : AssetData.Get().card_destroy_audio;
        public AudioClip AttackAudio => attackAudio ? attackAudio : AssetData.Get().card_attack_audio;
        public AudioClip DamageAudio => damageAudio ? damageAudio : AssetData.Get().card_damage_audio;
        
        

        public PieceModel GetSkin(string skinName)
        {
            foreach (var model in pieceModels)
            {
                if (model.skinName == skinName)
                {
                    return model;
                }
            }

            throw new Exception($"Skin {skinName} doesn't exist on piece {id}");
        }

        [Serializable]
        public struct PieceModel
        {
            public string skinName;
            public GameObject whitePrefab;
            public GameObject blackPrefab;
        }
    }
}