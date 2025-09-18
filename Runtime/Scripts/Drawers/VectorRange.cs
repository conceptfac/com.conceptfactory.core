using UnityEngine;

public class VectorRangeAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public string[] names;

    public VectorRangeAttribute(float min, float max, params string[] names)
    {
        this.min = min;
        this.max = max;
        this.names = names;
    }
}


public class Vector2RangeAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public string[] names;

    public Vector2RangeAttribute(float min, float max, string minName = "Min", string maxName = "Max")
    {
        this.min = min;
        this.max = max;
        this.names = new string[] { minName, maxName };
    }
}
