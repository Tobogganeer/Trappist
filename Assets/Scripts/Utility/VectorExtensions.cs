using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
	// http://unity3d.college/2016/11/22/unity-extension-methods/

	public static Vector3 Flattened(this Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

	public static Vector3 Flat(this Vector3 vector)
	{
		return vector.Flattened();
	}

	public static float DistanceFlat(this Vector3 origin, Vector3 destination)
	{
		return Vector3.Distance(origin.Flattened(), destination.Flattened());
	}

	public static float DistFlat(this Vector3 origin, Vector3 destination)
	{
		return origin.DistanceFlat(destination);
	}

	public static Vector2 xy(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector3 XYToXZ(this Vector2 v)
	{
		return new Vector3(v.x, 0, v.y);
	}

	public static Vector3 WithX(this Vector3 v, float x)
	{
		return new Vector3(x, v.y, v.z);
	}

	public static Vector3 X(this Vector3 v, float x)
	{
		return new Vector3(x, v.y, v.z);
	}

	public static Vector3 WithY(this Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	public static Vector3 Y(this Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	public static Vector3 WithZ(this Vector3 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector3 Z(this Vector3 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector2 WithX(this Vector2 v, float x)
	{
		return new Vector2(x, v.y);
	}

	public static Vector2 WithY(this Vector2 v, float y)
	{
		return new Vector2(v.x, y);
	}

	public static Vector3 WithZ(this Vector2 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point, bool isNormalized = false)
	{
		if (!isNormalized) axisDirection.Normalize();
		var d = Vector3.Dot(point, axisDirection);
		return axisDirection * d;
	}

	public static Vector3 NearestPointOnLine(this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine, bool isNormalized = false)
	{
		if (!isNormalized) lineDirection.Normalize();
		var d = Vector3.Dot(point - pointOnLine, lineDirection);
		return pointOnLine + (lineDirection * d);
	}


	public static Vector3 DirectionTo(this Vector3 origin, Vector3 target)
    {
		return Vector3.Normalize(target - origin);
    }

	public static Vector3 Dir(this Vector3 origin, Vector3 target)
	{
		return Vector3.Normalize(target - origin);
	}

	public static Vector3 DirectionTo_NoNormalize(this Vector3 origin, Vector3 target)
	{
		return target - origin;
	}

	/// <summary>
	/// Direction, not normalized
	/// </summary>
	public static Vector3 DirNN(this Vector3 origin, Vector3 target)
	{
		return target - origin;
	}

	public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
		return new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
    }

	public static float SqrDistance(this Vector3 from, Vector3 to)
    {
		float to_x = to.x - from.x;
		float to_y = to.y - from.y;
		float to_z = to.z - from.z;

		return to_x * to_x + to_y * to_y + to_z * to_z;
	}

	public static float DistSqr(this Vector3 from, Vector3 to)
	{
		float to_x = to.x - from.x;
		float to_y = to.y - from.y;
		float to_z = to.z - from.z;

		return to_x * to_x + to_y * to_y + to_z * to_z;
	}

	public static float Distance(this Vector3 from, Vector3 to)
    {
		return Vector3.Distance(from, to);
    }

	public static float Dist(this Vector3 from, Vector3 to)
	{
		return Vector3.Distance(from, to);
	}

	public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
    {
		return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
    }

	public static bool IsWithinBounds(this Vector3 v, Vector3 min, Vector3 max)
    {
		if (v.x < min.x) return false;
		if (v.y < min.y) return false;
		if (v.z < min.z) return false;

		if (v.x > max.x) return false;
		if (v.y > max.y) return false;
		if (v.z > max.z) return false;

		return true;
    }

	public static Vector3 Abs(this Vector3 v)
    {
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

	public static Vector3 InverseLerp(this Vector3 v, float a, float b)
    {
		return new Vector3(
			Mathf.InverseLerp(a, b, v.x),
			Mathf.InverseLerp(a, b, v.y),
			Mathf.InverseLerp(a, b, v.z)
			);
    }

	public static Vector3 Add(this Vector3 v, Vector3 add)
    {
		return new Vector3(v.x + add.x, v.y + add.y, v.z + add.z);
    }

	public static Vector3 Add(this Vector3 v, float? x = null, float? y = null, float? z = null)
	{
		return new Vector3(v.x + x ?? 0, v.y + y ?? 0, v.z + z ?? 0);
	}

	public static Vector3 Mult(this Vector3 v, Vector3 mult)
	{
		return new Vector3(v.x * mult.x, v.y * mult.y, v.z * mult.z);
	}

	public static Vector3 Mult(this Vector3 v, float factor)
	{
		return new Vector3(v.x * factor, v.y * factor, v.z * factor);
	}

	public static Vector3 Mult(this Vector3 v, float? x = null, float? y = null, float? z = null)
	{
		return new Vector3(v.x * x ?? 1, v.y * y ?? 1, v.z * z ?? 1);
	}

	public static float Average(this Vector3 v)
    {
		return (v.x + v.y + v.z) / 3f;
    }
}


public static class TransformExtensions
{
	public static MutPosition PositionMut(this Transform t)
    {
		return new MutPosition(t, false);
    }

	public static MutPosition LocalPositionMut(this Transform t)
	{
		return new MutPosition(t, true);
	}
}

public class MutPosition
{
	Transform t;
	bool local;

	public MutPosition(Transform t, bool local)
    {
		this.t = t;
		this.local = local;
    }

	public float x
	{
		get
		{
			return local ? t.localPosition.x : t.position.x;
		}
		set
		{
			if (local) t.localPosition = t.localPosition.WithX(value);
			else t.position = t.position.WithX(value);
		}
	}

	public float y
	{
		get
		{
			return local ? t.localPosition.y : t.position.y;
		}
		set
		{
			if (local) t.localPosition = t.localPosition.WithY(value);
			else t.position = t.position.WithY(value);
		}
	}

	public float z
	{
		get
		{
			return local ? t.localPosition.z : t.position.z;
		}
		set
		{
			if (local) t.localPosition = t.localPosition.WithZ(value);
			else t.position = t.position.WithZ(value);
		}
	}

	// Use these to chain calls together
	public MutPosition X(float x)
	{
		this.x = x;
		return this;
	}

	public MutPosition Y(float y)
	{
		this.y = y;
		return this;
	}

	public MutPosition Z(float z)
	{
		this.z = z;
		return this;
	}

	// Use this to set multiple in one call, i.e transform.PositionMut().XYZ(y: 0, z: 15);
	public MutPosition XYZ(float? x = null, float? y = null, float? z = null)
	{
		this.x = x ?? this.x;
		this.y = y ?? this.y;
		this.z = z ?? this.z;
		return this;
	}
}
