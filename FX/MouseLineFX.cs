using System;
using TcgEngine.Client;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Board;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
namespace TcgEngine.FX
{
    /// <summary>
    /// Line FX that appear when dragin a board card to attack
    /// </summary>

    public class MouseLineFX : MonoBehaviour
    {
        public GameObject dot_template;
        public float[] dot_spacing = new float[] { 0.2f };

        private float[] default_dot_spacing = new float[] { 0.05f };
        private float[] dashed_dot_spacing = new float[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.2f };
        private List<GameObject> dot_list = new List<GameObject>();
        private List<Vector3> points = new List<Vector3>();

        public bool useParabola = false; // Toggle for projectile trajectory
        public float parabolaHeight = 1.5f; // Height of the parabola
        
        public Color rangeAttackColor = Color.red;
        public Color meleeAttackColor = Color.yellow;
        public Color canMoveColor = Color.green;
        public Color cantMoveColor = Color.grey;
        

        void Start()
        {
            dot_template.SetActive(false);
        }

        void Update()
        {
            RefreshLine();
            RefreshRender();
        }

        private void RefreshLine()
        {
            points.Clear();

            if (!GameClient.Get().IsReady())
            {
                return;
            }
            
            PlayerControls controls = PlayerControls.Get();
            BoardElement bcard = controls.GetDragged();
            bool visible = false;
            Vector3 source = Vector3.zero;
            
            if (!GameClient.GetGameData().GetCurrentCardTurn().Contains(bcard?.GetCard()) || !GameClient.Get().IsYourTurn() || bcard.GetCard().exhausted)
            {
                return;
            }

            bool canRangeAttack = false;
            bool canMeleeAttack = false;
            bool canMove = false;
            
            if (bcard != null)
            {
                source = bcard.transform.position;
                visible = true;
                
                BoardSlot hoveredBoardSlot = BoardInputManager.Instance.GetLastHoveredSlot();
                Slot hoveredSlot = hoveredBoardSlot == null ? Slot.None : hoveredBoardSlot.GetSlot();
                var game = GameClient.GetGameData();
                canRangeAttack = game.CanRangeAttackTarget(bcard?.GetCard(), hoveredSlot);
                canMeleeAttack = game.CanAttackTarget(bcard?.GetCard(), game.GetSlotCard(hoveredSlot));
                canMove = game.CanMoveCard(bcard?.GetCard(), hoveredSlot);

                if (canRangeAttack)
                {
                    dot_spacing = dashed_dot_spacing;
                    useParabola = true;
                    SetDotColor(rangeAttackColor);
                }
                else if (canMeleeAttack)
                {
                    dot_spacing = default_dot_spacing;
                    useParabola = false;
                    SetDotColor(meleeAttackColor);
                }
                else if (canMove)
                {
                    dot_spacing = default_dot_spacing;
                    useParabola = false;
                    SetDotColor(canMoveColor);
                }
                else
                {
                    dot_spacing = default_dot_spacing;
                    useParabola = false;
                    SetDotColor(cantMoveColor);
                }
            }

            HandCard drag = HandCard.GetDrag();
            if (drag != null)
            {
                source = drag.transform.position;
                visible = drag.GetCardData().IsRequireTarget();
            }
            
            if (BoardInputManager.Instance.GetLastHoveredSlot() == null)
            {
                visible = false;
            }

            if (visible)
            {

                if (bcard != null && bcard.GetCard().GetPieceType() == PieceType.Knight && canMove)
                {
                    Slot sourceSlot = bcard.GetCard().slot;
                    Slot dest2 = BoardInputManager.Instance.GetLastHoveredSlot().GetSlot();
                    Slot dest1;
                    if (Math.Abs(dest2.y - sourceSlot.y) < Math.Abs(dest2.x - sourceSlot.x))
                    {
                        dest1 = new Slot(sourceSlot.x, dest2.y);
                    }
                    else
                    {
                        dest1 = new Slot(dest2.x, sourceSlot.y);
                    }
                    AddLineSegment(source, BoardSlot.Get(dest1).transform.position);
                    AddLineSegment(BoardSlot.Get(dest1).transform.position, BoardSlot.Get(dest2).transform.position);
                }
                else
                {
                    Vector3 dest = BoardInputManager.Instance.GetMousePositionOnBoard();
                    AddLineSegment(source, dest);
                }
                
                // float dist = (dest - source).magnitude;
                //
                // Vector3 dir = (dest - source).normalized;
                // float value = 0f;
                // int spacingIndex = 0;
                // while (value < dist)
                // {
                //     float spacing = dot_spacing.Length > 0 ? dot_spacing[spacingIndex % dot_spacing.Length] : 0.2f;
                //     float t = dist > 0f ? value / dist : 0f;
                //     Vector3 pos;
                //     if (useParabola)
                //     {
                //         pos = Vector3.Lerp(source, dest, t);
                //         float height = 4 * parabolaHeight * t * (1 - t);
                //         pos += Camera.main.transform.up * height;
                //     }
                //     else
                //     {
                //         pos = source + dir * value;
                //     }
                //     points.Add(pos);
                //     value += spacing;
                //     spacingIndex++;
                // }
            
            }
        }
        
        private void AddLineSegment(Vector3 source, Vector3 dest)
        {
            float dist = (dest - source).magnitude;
                
            Vector3 dir = (dest - source).normalized;
            float value = 0f;
            int spacingIndex = 0;
            while (value < dist)
            {
                float spacing = dot_spacing.Length > 0 ? dot_spacing[spacingIndex % dot_spacing.Length] : 0.2f;
                float t = dist > 0f ? value / dist : 0f;
                Vector3 pos;
                if (useParabola)
                {
                    pos = Vector3.Lerp(source, dest, t);
                    float height = 4 * parabolaHeight * t * (1 - t);
                    pos += Camera.main.transform.up * height;
                }
                else
                {
                    pos = source + dir * value;
                }
                points.Add(pos);
                value += spacing;
                spacingIndex++;
            }
        }

        private void RefreshRender()
        {
            while (dot_list.Count < points.Count)
            {
                AddDot();
            }

            int index = 0;
            foreach (GameObject dot in dot_list)
            {
                bool active = false;
                if (index < points.Count)
                {
                    Vector3 pos = points[index];
                    dot.transform.position = pos;
                    active = true;
                }

                if (dot.activeSelf != active)
                    dot.SetActive(active);

                index++;
            }
        }

        public void AddDot()
        {
            GameObject dot = Instantiate(dot_template, transform);
            dot.SetActive(true);
            dot_list.Add(dot);
        }

        public void SetDotColor(Color color)
        {
            foreach (GameObject dot in dot_list)
            {
                ParticleSystem ps = dot.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = color;
                    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
                    int count = ps.GetParticles(particles);
                    for (int i = 0; i < count; i++)
                    {
                        particles[i].startColor = color;
                    }
                    ps.SetParticles(particles, count);
                }
            }
        }
    }
}
