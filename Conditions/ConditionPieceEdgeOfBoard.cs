using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// This condition checks if the piece is at the edge of the board. It can check for flanks, front/back rank, ally edge, or opponent edge.
    /// </summary>
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionPieceEdgeOfBoard", order = 10)]
    public class ConditionPieceEdgeOfBoard : ConditionData
    {
        public bool isAtEdge = true;
        public EdgeType edgeType;
        
        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            Card cardTarget = data.GetSlotCard(args.SlotTarget);
            int x = args.SlotTarget.x;
            int y = args.SlotTarget.y;
            int width = GameplayData.Get().boardSizeX;
            int height = GameplayData.Get().boardSizeY;

            switch (edgeType)
            {
                case EdgeType.AllyEdge:
                    if (cardTarget == null) return false;
                    return isAtEdge == IsAllyEdge(cardTarget.playerID, y, height);
                case EdgeType.OpponentEdge:
                    if (cardTarget == null) return false;
                    return isAtEdge == IsOpponentEdge(cardTarget.playerID, y, height);
                case EdgeType.Flanks:
                    return isAtEdge == (x == 0 || x == width - 1);
                case EdgeType.FrontBackRank:
                    return isAtEdge == (y == 0 || y == height - 1);
                default:
                    return false;
            }
        }
        
        private bool IsAllyEdge(int playerID, int y, int height)
        {
            return playerID == 1 ? y == 0 : y == height - 1;
        }

        private bool IsOpponentEdge(int playerID, int y, int height)
        {
            return playerID == 1 ? y == height - 1 : y == 0;
        }

        public enum EdgeType
        {
            Flanks, FrontBackRank, AllyEdge, OpponentEdge
        }
        
    }
}