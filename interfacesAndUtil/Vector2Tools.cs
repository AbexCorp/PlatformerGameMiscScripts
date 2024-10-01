using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Vector2Tools
{
    /// <summary>
    /// Used to rotate the point around a given pivot by an angle.
    /// </summary>
    /// <example>
    /// Point = 5,6
    /// Pivot = 4,6
    /// Angle = -90
    /// Result = 4,7
    /// </example>
    /// <param name="point"></param>
    /// <param name="pivot"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float angle)
    {
        Vector2 direction = point - pivot;
        float distance = direction.magnitude;
        float initialAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = initialAngle + angle;
        float angleRadians = Mathf.Deg2Rad * angle;
        Vector2 rotated = new Vector2(distance * Mathf.Cos(angleRadians), distance * Mathf.Sin(angleRadians));
        return rotated + pivot;
    }
    public static Vector2 ReadMousePosition() //Temporary function to read mouse position
    {
        return (Vector2)(Camera.main.ScreenToWorldPoint( Mouse.current.position.value));
    }
}
