using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monarchs.Logic
{
    public class SlotOperations
    {
        public virtual Vector2S GetCoordinate(Slot slot)
        {
            return new Vector2S(slot.x, slot.y);
        }
        
        public virtual string GetCoordinateString(Slot slot)
        {
            return GetRowLetterFromXCoordinate(slot.x) + (slot.y+1).ToString();
        }
        
        public string GetRowLetterFromXCoordinate(int x)
        {
            return ((char)('A' + x)).ToString();
        }

        public virtual bool IsInRangeX(Slot slot, Slot slotInRange, int range)
        {
            return Mathf.Abs(slot.x - slotInRange.x) <= range;
        }

        public virtual bool IsInRangeY(Slot slot, Slot slotInRange, int range)
        {
            return Mathf.Abs(slot.y - slotInRange.y) <= range;
        }

        //No Diagonal, Diagonal = 2 dist
        public virtual bool IsInDistanceStraight(Slot slot, Slot slotInRange, int dist)
        {
            int r = Mathf.Abs(slot.x - slotInRange.x) + Mathf.Abs(slot.y - slotInRange.y);
            return r <= dist;
        }

        public virtual int GetDistanceTo(Slot slot, Slot slotToGo)
        {
            int dx = Mathf.Abs(slot.x - slotToGo.x);
            int dy = Mathf.Abs(slot.y - slotToGo.y);

            return Math.Max(dx, dy);
        }

        //Diagonal = 1 dist
        public virtual bool IsInDistance(Slot slot, Slot slotInDistance, int dist)
        {
            int dx = Mathf.Abs(slot.x - slotInDistance.x);
            int dy = Mathf.Abs(slot.y - slotInDistance.y);
            return dx <= dist && dy <= dist;
        }

        public virtual List<Slot> GetSlotsInRange(Slot slot, int range)
        {
            List<Slot> slotList = new List<Slot>();
            int minX = Math.Max(slot.x - range, Slot.xMin);
            int maxX = Math.Min(slot.x + range, Slot.xMax);
            int minY = Math.Max(slot.y - range, Slot.yMin);
            int maxY = Math.Min(slot.y + range, Slot.yMax);
        
            for (int x = minX; x<= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Slot _slot = Slot.Get(x, y);

                    slotList.Add(_slot);
                
                }
            }

            return slotList;
        }

        //Check if the slot is valid one (or if out of board)
        public virtual bool IsValid(Slot slot)
        {
            //bool isValid = slot.x >= Slot.x_min && slot.x <= Slot.x_max && slot.y >= Slot.y_min && slot.y <= Slot.y_max;
            return slot.x >= Slot.xMin && slot.x <= Slot.xMax && slot.y >= Slot.yMin && slot.y <= Slot.yMax;
        }

        public virtual bool IsInStraightLine(Slot slot1, Slot slot2)
        {
            return (slot1.x == slot2.x || slot1.y == slot2.y);
        }

        public virtual bool IsInDiagonal(Slot slot1, Slot slot2)
        {
            return (Math.Abs(slot1.y - slot2.y) == Math.Abs(slot1.x - slot2.x));
        }

        public virtual bool IsBetween(Slot slot1, Slot slot2, Slot slotToCheck)
        {
            Vector3 position1 = new Vector3(slot1.x, slot1.y);
            Vector3 position2 = new Vector3(slot2.x, slot2.y);
            Vector3 position3 = new Vector3(slotToCheck.x, slotToCheck.y);

            return Vector2S.IsCBetweenAAndB(position1, position2, position3);
        }

        public virtual Slot GetClosest(Slot slot, List<Slot> slotList)
        {
            float minDistance = Mathf.Infinity;
            Slot closestSlot = Slot.None;

            foreach (var s in slotList)
            {
                float distance = (slot.GetCoordinate() - s.GetCoordinate()).Magnitude();
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSlot = s;
                }
            }

            return closestSlot;
        }
    
    
        //Get a random slot amongts all valid ones
        public virtual Slot GetRandom(System.Random rand)
        {
            if (Slot.yMax > Slot.yMin)
                return new Slot(rand.Next(Slot.xMin, Slot.xMax + 1), rand.Next(Slot.yMin, Slot.yMax + 1));
            return new Slot(rand.Next(Slot.xMin, Slot.xMax + 1), Slot.yMin);
        }

        public virtual Slot Get(int x, int y)
        {
            List<Slot> slots = GetAll();
            foreach (Slot slot in slots)
            {
                if (slot.x == x && slot.y == y)
                    return slot;
            }
            return new Slot(x, y);
        }

        /// <summary>
        /// This returns a list of all the slot between slot1 and slot2. It doesn't return slot1 or slot2 in the list.
        /// </summary>
        public virtual List<Slot> GetSlotInBetween(Slot slot1, Slot slot2)
        {
            List<Slot> returnList = new List<Slot>();
            int deltaX = slot2.x - slot1.x;
            int deltaY = slot2.y - slot1.y;
            int signX = Math.Sign(deltaX);
            int signY = Math.Sign(deltaY);
        
            // Horizontal
            if (deltaY == 0)
            {
                for (int x = slot1.x + signX; x != slot2.x; x+= signX)
                {
                    returnList.Add(Get(x, slot1.y));
                }
            }
        
            // Vertical
            if (deltaX == 0)
            {
                for (int y = slot1.y+signY; y != slot2.y; y+=signY)
                {
                    returnList.Add(Get(slot1.x, y));
                }
            }
        
            //Diagonal
            if (Math.Abs(deltaX) == Math.Abs(deltaY))
            {
                for (int x = slot1.x + signX; x != slot2.x; x+= signX)
                {
                    for (int y = slot1.y + signY; y != slot2.y; y += signY)
                    {
                        returnList.Add(Get(x, y));
                    }
                }
            }

            return returnList;
        }

        public virtual Slot KeepMovementOnBoard(Slot slot1, Slot slot2)
        {
            if (slot2.IsValid())
            {
                return slot2;
            }
        
            int deltaX = slot2.x - slot1.x;
            int deltaY = slot2.y - slot1.y;
            int signX = Math.Sign(deltaX);
            int signY = Math.Sign(deltaY);

            return KeepMovementOnBoard(slot1, Slot.Get(slot2.x - signX, slot2.y - signY));
        }
    
        //Get all valid slots
        public virtual List<Slot> GetAll()
        {
            if (Slot.all_slots.Count > 0)
                return Slot.all_slots; //Faster access

            Slot.GenerateAllSlots();
        
            return Slot.all_slots;
        }
    }
}
