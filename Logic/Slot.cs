using System;
using System.Collections.Generic;
using Monarchs.Ability.Target;
using TcgEngine;
using TcgEngine.Client;
using Unity.Netcode;

namespace Monarchs.Logic
{
    /// <summary>
    /// Represent a slot in gameplay (data only)
    /// </summary>

    [System.Serializable]
    public struct Slot : INetworkSerializable, ITargetable
    {
        public int x; //From 1 to 5
        public int y; //Not in use, could be used to add more rows or different locations on the board

        public static readonly List<Slot> all_slots = new List<Slot>();

        public static int xMin => 0; //Minimum x coordinate
        public static int xMax => GameplayData.Get().boardSizeX - 1; //Maximum x coordinate
        public static int yMin => 0; //Minimum y coordinate
        public static int yMax => GameplayData.Get().boardSizeY - 1; //Maximum y coordinate
        
        public static SlotOperations SlotOperations
        {
            get {
                if (_slotOperations != null)
                {
                    return _slotOperations;
                }
                else
                {
                    return new SlotOperations();
                }
                ;  }
        }
        private static SlotOperations _slotOperations;

        public Slot(int x, int y, SlotOperations slotOperations)
        {
            this.x = x;
            this.y = y;
            _slotOperations = slotOperations;
        }
        
        public Slot(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Slot(Slot slot)
        {
            this.x = slot.x;
            this.y = slot.y;
        }

        public Slot GetSlot()
        {
            return this;
        }

        public bool CanBeTargeted()
        {
            return true;
        }

        public int GetPlayerId()
        {
            return -1;
        }

        public Vector2S GetCoordinate()
        {
            return SlotOperations.GetCoordinate(this);
        }
        
        public string GetCoordinateString()
        {
            return SlotOperations.GetCoordinateString(this);
        }

        public bool IsInRangeX(Slot slot, int range)
        {
            return SlotOperations.IsInRangeX(this, slot, range);
        }

        public bool IsInRangeY(Slot slot, int range)
        {
            return SlotOperations.IsInRangeY(this, slot, range);
        }

        //No Diagonal, Diagonal = 2 dist
        public bool IsInDistanceStraight(Slot slot, int dist)
        {
            return SlotOperations.IsInDistanceStraight(this, slot, dist);
        }

        public int GetDistanceTo(Slot slot)
        {
            return SlotOperations.GetDistanceTo(this, slot);
        }

        //Diagonal = 1 dist
        public bool IsInDistance(Slot slot, int dist)
        {
            return SlotOperations.IsInDistance(this, slot, dist);
        }

        public List<Slot> GetSlotsInRange(int range)
        {
            return SlotOperations.GetSlotsInRange(this, range);
        }

        //Check if the slot is valid one (or if out of board)
        public bool IsValid()
        {
            return SlotOperations.IsValid(this);
        }

        public static bool IsInStraightLine(Slot slot1, Slot slot2)
        {
            return SlotOperations.IsInStraightLine(slot1, slot2);
        }

        public static bool IsInDiagonal(Slot slot1, Slot slot2)
        {
            return SlotOperations.IsInDiagonal(slot1, slot2);
        }

        public static bool IsBetween(Slot slot1, Slot slot2, Slot slotToCheck)
        {
            return SlotOperations.IsBetween(slot1, slot2, slotToCheck);
        }

        public static Slot GetClosest(Slot slot, List<Slot> slotList)
        {
            return SlotOperations.GetClosest(slot, slotList);
        }
        
        
        //Get a random slot amongts all valid ones
        public static Slot GetRandom(System.Random rand)
        {
            return SlotOperations.GetRandom(rand);
        }

        public static Slot Get(int x, int y)
        {
            return SlotOperations.Get(x, y);
        }
        
        public static Slot Get(Vector2S vector)
        {
            return SlotOperations.Get(vector.x, vector.y);
        }
        
        public static List<Slot> GetSlotsOfTargets(List<ITargetable> targets)
        {
            List<Slot> slots = new List<Slot>();
            
            foreach (var target in targets)
            {
                if (target != null)
                {
                    slots.Add(target.GetSlot());
                }
            }

            return slots;
        }

        /// <summary>
        /// This returns a list of all the slot between slot1 and slot2. It doesn't return slot1 or slot2 in the list.
        /// </summary>
        public static List<Slot> GetSlotInBetween(Slot slot1, Slot slot2)
        {
            return SlotOperations.GetSlotInBetween(slot1, slot2);
        }

        public static Slot KeepMovementOnBoard(Slot slot1, Slot slot2)
        {
            return SlotOperations.KeepMovementOnBoard(slot1, slot2);
        }
        
        //Get all valid slots
        public static List<Slot> GetAll()
        {
            return SlotOperations.GetAll(); //Faster access
        }

        public static List<Slot> GetSlotListFromCoordinates(List<Vector2S> coordinates)
        {
            List<Slot> slots = new List<Slot>();

            foreach (var coordinate in coordinates)
            {
                slots.Add(new Slot(coordinate.x, coordinate.y));
            }

            return slots;
        }
        
        public static List<Vector2S> GetCoordinateListFromSlots(List<Slot> slots)
        {
            List<Vector2S> coordinates = new List<Vector2S>();

            foreach (var slot in slots)
            {
                coordinates.Add(new Vector2S(slot.x, slot.y));
            }

            return coordinates;
        }

        public static void GenerateAllSlots()
        {
            for (int y = 0; y <= yMax; y++)
            {
                for (int x = 0; x <= xMax; x++)
                {
                    all_slots.Add(new Slot(x, y));
                }
            }
        }

        public static bool operator ==(Slot slot1, Slot slot2)
        {
            return slot1.x == slot2.x && slot1.y == slot2.y;
        }

        public static bool operator !=(Slot slot1, Slot slot2)
        {
            return slot1.x != slot2.x || slot1.y != slot2.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public bool Equals(Slot other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Slot other && Equals(other);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
        }

        public static Slot None
        {
            get { return new Slot(-1, -1); }
        }
    }
}
