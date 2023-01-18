using System;
using UnityEngine;

/// <summary>
///     Unity-related extension methods for ValueRandom by David F Dev.
/// </summary>
public static class ValueRandomUnityExtensions
{
	#region Static fields and constants

	private const float PI_2 = Mathf.PI * 2f;

	#endregion

	#region Static methods

	/// <summary>
	///     Generates a random float between 0.0 [inclusive] and 2Pi [exclusive].
	/// </summary>
	public static float NextAngleRadians(this ValueRandom rng, out ValueRandom next)
	{
		return rng.NextSingle(PI_2, out next);
	}

	/// <summary>
	///     Generates a random float between 0.0 [inclusive] and 360.0 [exclusive].
	/// </summary>
	public static float NextAngleDegrees(this ValueRandom rng, out ValueRandom next)
	{
		return rng.NextSingle(360f, out next);
	}

	/// <summary>
	///     Generates a colour with random components (RGB) between 0.0 [inclusive] and 1.0 [exclusive].
	/// </summary>
	public static Color NextColour(this ValueRandom rng, out ValueRandom next)
	{
		return new Color(rng.NextSingle(out next), next.NextSingle(out next), next.NextSingle(out next));
	}

	/// <summary>
	///     Generates a colour with random components (RGB) between 0 [inclusive] and 255 [inclusive].
	/// </summary>
	public static Color32 NextColour32(this ValueRandom rng, out ValueRandom next)
	{
		return new Color32(rng.NextByte(out next), next.NextByte(out next), next.NextByte(out next), byte.MaxValue);
	}

	/// <summary>
	///     Generates a random unit vector.
	/// </summary>
	public static Vector2 NextVector2(this ValueRandom rng, out ValueRandom next, float magnitude = 1f)
	{
		float angle = rng.NextAngleRadians(out next);
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * magnitude;
	}

	/// <summary>
	///     Generates a random unit vector.
	/// </summary>
	public static Vector3 NextVector3(this ValueRandom rng, out ValueRandom next, float magnitude = 1f)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///     Generates a random unit vector.
	/// </summary>
	public static Vector4 NextVector4(this ValueRandom rng, out ValueRandom next, float magnitude = 1f)
	{
		throw new NotImplementedException();
	}

	#endregion
}
