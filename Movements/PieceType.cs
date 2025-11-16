using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PieceType
{
    Monarch = 1,
    Champion = 2,
    Rook = 4,
    Knight = 8,
    Bishop = 16,
    Pawn = 32,
}


