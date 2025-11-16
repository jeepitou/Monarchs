using System;
using UnityEngine;

[Serializable]
public struct Vector2S
{
    public int x;
    public int y;
    
    public static Vector2S zero = new Vector2S(0, 0);
 
    public Vector2S(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
 
    public override bool Equals(object obj)
    {
        if (!(obj is Vector2S))
        {
            return false;
        }
 
        var s = (Vector2S)obj;
        return x == s.x &&
               y == s.y;
    }
 
 
    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public float Magnitude()
    {
        return (float)Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2));
    }
    
    
    public static bool IsCBetweenAAndB(Vector3 A, Vector3 B, Vector3 C)
    {
        return ((B - A).magnitude > (B - C).magnitude && (B - A).normalized == (B - C).normalized);
    }
    
 
    public static bool operator ==(Vector2S a, Vector2S b)
    {
        return a.x == b.x && a.y == b.y;
    }
 
    public static bool operator !=(Vector2S a, Vector2S b)
    {
        return a.x != b.x || a.y != b.y;
    }
    
    public static Vector2S operator +(Vector2S a, Vector2S b)
    {
        return new Vector2S(a.x + b.x, a.y + b.y);
    }
    
    public static Vector2S operator -(Vector2S a, Vector2S b)
    {
        return new Vector2S(a.x - b.x, a.y - b.y);
    }
 
    public static implicit operator Vector2(Vector2S x)
    {
        return new Vector2(x.x, x.y);
    }
 
    public static implicit operator Vector2S(Vector2Int x)
    {
        return new Vector2S(x.x, x.y);
    }


    public Vector2S ToUnit()
    {
        x = x > 1 ? 1 : x;
        x = x < -1 ? -1 : x;
        y = y > 1 ? 1 : y;
        y = y < -1 ? -1 : y;

        return this;
    }
    
    public Vector2S GetPerpendicular()
    {
        Vector2S perpendicular = this;

        if (x == 0)
        {
            perpendicular.x = 1;
            perpendicular.y = 0;
        }
        else if (y == 0)
        {
            perpendicular.y = 1;
            perpendicular.x = 0;
        }
        else
        {
            perpendicular.x = -perpendicular.x;
        }
        return perpendicular;
    }
    
    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode();
    }
}