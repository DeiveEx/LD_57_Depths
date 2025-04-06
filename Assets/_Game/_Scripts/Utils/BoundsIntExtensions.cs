using UnityEngine;

public static class BoundsIntExtensions
{
    public static BoundsInt Encapsulate(this BoundsInt bounds, BoundsInt other)
    {
        Bounds a = new Bounds(bounds.center, bounds.size);
        Bounds b = new(other.center, other.size);
        
        a.Encapsulate(b);

        return new BoundsInt(bounds.position, new Vector3Int((int)a.size.x, (int)a.size.y, (int)a.size.z));
    }
    
    public static bool Intersects(this BoundsInt bounds, BoundsInt other)
    {
        Bounds a = new(bounds.center, bounds.size);
        Bounds b = new(other.center, other.size);

        return a.Intersects(b);
    }
    
    /// <summary>
    /// Check if this bounds is completely inside other
    /// </summary>
    public static bool IsContainedBy(this BoundsInt bounds, BoundsInt other)
    {
        if(bounds.min.x < other.min.x || bounds.max.x > other.max.x ||
           bounds.min.y < other.min.y || bounds.max.y > other.max.y ||
           bounds.min.z < other.min.z || bounds.max.z > other.max.z)
            return false;

        return true;
    }
}
