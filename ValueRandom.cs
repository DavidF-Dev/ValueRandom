using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

/// <summary>
///     Random number generator implemented as a struct in C# by David F Dev.<br />
///     Based off of 'NetRandom - A fast random number generator for .NET' by Colin Green (January, 2005):
///     https://github.com/DV8FromTheWorld/C-Sharp-Programming/blob/master/LIBRARIES/Lidgren.Network/Lidgren.Network/NetRandom.cs
///     <br />
///     - No heap allocation or boxing.<br />
///     - Up to 8x faster than System.Random depending on which methods are called.<br />
///     - Light-weight; only 128 bytes.<br />
///     - Re-initialisation is as easy as creating a new instance with a different seed.<br />
///     <example>
///         <code>
///             ValueRandom rng = new ValueRandom(Environment.TickCount);
///             uint sample = rng.NextUInt32(out rng);
///             uint nextSample = rng.NextUInt32(out rng);
///         </code>
///         <br />
///         Each method provides the next instance. Be sure to use it otherwise you will keep getting the same results.
///     </example>
/// </summary>
public readonly struct ValueRandom : IEquatable<ValueRandom>
{
	#region Static fields and constants

	private const double REAL_UNIT_INT = 1.0 / (int.MaxValue + 1.0);
	private const double REAL_UNIT_UINT = 1.0 / (uint.MaxValue + 1.0);
	private const uint Y = 842502087;
	private const uint Z = 3579807591;
	private const uint W = 273326509;

	#endregion

	#region Static properties

	/// <summary>
	///     Retrieve a default instance with its seed set to Environment.TickCount.
	/// </summary>
	public static ValueRandom Default => new(Environment.TickCount);

	#endregion

	#region Static methods

	public static bool operator==(ValueRandom left, ValueRandom right)
	{
		return left.Equals(right);
	}

	public static bool operator!=(ValueRandom left, ValueRandom right)
	{
		return !left.Equals(right);
	}

	#endregion

	#region Fields

	private readonly uint _x;
	private readonly uint _y;
	private readonly uint _z;
	private readonly uint _w;

	#endregion

	#region Constructors

	/// <summary>
	///     Initialise a new instance using an int value seed.
	/// </summary>
	public ValueRandom(int seed)
	{
		_x = (uint)seed;
		_y = Y;
		_z = Z;
		_w = W;
	}

	private ValueRandom(uint x, uint y, uint z, uint w)
	{
		_x = x;
		_y = y;
		_z = z;
		_w = w;
	}

	#endregion

	#region Methods

	/// <summary>
	///     Generates the next instance.
	/// </summary>
	public ValueRandom Next()
	{
		return InternalNext();
	}

	/// <summary>
	///     Generates a random int between 0 [inclusive] and int.MaxValue [inclusive].
	/// </summary>
	public int NextInt32(out ValueRandom next)
	{
		next = InternalNext();
		return (int)(int.MaxValue & next._w);
	}

	/// <summary>
	///     Generates a random int between 0 [inclusive] and upperBound [exclusive].
	/// </summary>
	public int NextInt32(int upperBound, out ValueRandom next)
	{
		if(upperBound < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= 0");
		}

		return NextInt32(0, upperBound, out next);
	}

	/// <summary>
	///     Generate a random int between lowerBound [inclusive] and upperBound [exclusive].
	///     upperBound must be >= lowerBound. lowerBound may be negative.
	/// </summary>
	public int NextInt32(int lowerBound, int upperBound, out ValueRandom next)
	{
		if(lowerBound > upperBound)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= lowerBound");
		}

		next = InternalNext();
		int range = upperBound - lowerBound;
		if(range < 0)
		{
			return lowerBound + (int)(REAL_UNIT_UINT * next._w * (upperBound - (long)lowerBound));
		}

		return lowerBound + (int)(REAL_UNIT_INT * (int)(next._w & int.MaxValue) * range);
	}

	/// <summary>
	///     Generates a random uint between 0 [inclusive[ and uint.MaxValue [inclusive] (fastest method).
	/// </summary>
	public uint NextUInt32(out ValueRandom next)
	{
		next = InternalNext();
		return next._w;
	}

	/// <summary>
	///     Generates a random long between 0 [inclusive] and long.MaxValue [inclusive].
	/// </summary>
	public long NextInt64(out ValueRandom next)
	{
		Span<byte> buf = stackalloc byte[8];
		NextBytes(buf, out next);
		return Math.Abs(BitConverter.ToInt64(buf));
	}

	/// <summary>
	///     Generates a random long between 0 [inclusive] and upperBound [exclusive].
	/// </summary>
	public long NextInt64(long upperBound, out ValueRandom next)
	{
		if(upperBound < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= 0");
		}

		return NextInt64(0L, upperBound, out next);
	}

	/// <summary>
	///     Generates a random long between lowerBound [inclusive] and upperBound [exclusive].
	///     upperBound must be >= lowerBound. lowerBound may be negative.
	/// </summary>
	public long NextInt64(long lowerBound, long upperBound, out ValueRandom next)
	{
		if(lowerBound > upperBound)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= lowerBound");
		}

		Span<byte> buf = stackalloc byte[8];
		NextBytes(buf, out next);
		return Math.Abs(BitConverter.ToInt64(buf) % (upperBound - lowerBound)) + lowerBound;
	}

	/// <summary>
	///     Generates a random double between 0.0 [inclusive] and 1.0 [exclusive].
	/// </summary>
	public double NextDouble(out ValueRandom next)
	{
		next = InternalNext();
		return REAL_UNIT_INT * (int)(next._w & int.MaxValue);
	}

	/// <summary>
	///     Generates a random double between 0.0 [inclusive] and upperBound [exclusive].
	/// </summary>
	public double NextDouble(double upperBound, out ValueRandom next)
	{
		if(upperBound < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= 0");
		}

		return NextDouble(0.0, upperBound, out next);
	}

	/// <summary>
	///     Generates a random double between lowerBound [inclusive] and upperBound [exclusive].
	///     upperBound must be >= lowerBound. lowerBound may be negative.
	/// </summary>
	public double NextDouble(double lowerBound, double upperBound, out ValueRandom next)
	{
		if(lowerBound > upperBound)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= lowerBound");
		}

		double value = NextDouble(out next);
		return value * (upperBound - lowerBound) + lowerBound;
	}

	/// <summary>
	///     Generates a random single between 0.0 [inclusive] and 1.0 [exclusive].
	/// </summary>
	public float NextSingle(out ValueRandom next)
	{
		return (float)NextDouble(out next);
	}

	/// <summary>
	///     Generates a random single between 0.0 [inclusive] and upperBound [exclusive].
	/// </summary>
	public float NextSingle(float upperBound, out ValueRandom next)
	{
		if(upperBound < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= 0");
		}

		return NextSingle(0f, upperBound, out next);
	}

	/// <summary>
	///     Generates a random single between lowerBound [inclusive] and upperBound [exclusive].
	///     upperBound must be >= lowerBound. lowerBound may be negative.
	/// </summary>
	public float NextSingle(float lowerBound, float upperBound, out ValueRandom next)
	{
		if(lowerBound > upperBound)
		{
			throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBounds must be >= lowerBound");
		}

		float value = NextSingle(out next);
		return value * (upperBound - lowerBound) + lowerBound;
	}

	/// <summary>
	///     Generates a random bool with an equal chance of either being true or false.
	/// </summary>
	public bool NextBool(out ValueRandom next)
	{
		uint value = NextUInt32(out next);
		return value > uint.MaxValue / 2;
	}

	/// <summary>
	///     Generates a random bool [1/denominator]. A denominator of 2 equals a 50% chance.
	/// </summary>
	public bool NextBool(int denominator, out ValueRandom next)
	{
		if(denominator > 0)
		{
			return NextDouble(out next) < 1 / (double)denominator;
		}

		next = this;
		return false;
	}

	/// <summary>
	///     Generates a random bool with a chance between 0.0 [0%] and 1.0 [100%].
	/// </summary>
	public bool NextBool(float chance, out ValueRandom next)
	{
		return NextSingle(out next) < chance;
	}

	/// <summary>
	///     Generates a random bool with a chance between 0.0 [0%] and 1.0 [100%].
	/// </summary>
	public bool NextBool(double chance, out ValueRandom next)
	{
		return NextDouble(out next) < chance;
	}

	/// <summary>
	///     Fills the provided byte array with random bytes.
	/// </summary>
	public void NextBytes(byte[] buffer, out ValueRandom next)
	{
		if(buffer == null)
		{
			throw new ArgumentNullException(nameof(buffer));
		}

		next = this;
		if(buffer.Length == 0)
		{
			return;
		}

		int i = 0;

		// Generate 4 random bytes at a time to increase performance
		for(int bound = buffer.Length - 3; i < bound;)
		{
			next = next.InternalNext();
			buffer[i++] = (byte)next._w;
			buffer[i++] = (byte)(next._w >> 8);
			buffer[i++] = (byte)(next._w >> 16);
			buffer[i++] = (byte)(next._w >> 24);
		}

		// Fill up any remaining bytes in the buffer
		if(i < buffer.Length)
		{
			next = next.InternalNext();
			buffer[i++] = (byte)next._w;
			if(i < buffer.Length)
			{
				buffer[i++] = (byte)(next._w >> 8);
				if(i < buffer.Length)
				{
					buffer[i++] = (byte)(next._w >> 16);
					if(i < buffer.Length)
					{
						buffer[i + 1] = (byte)(next._w >> 24);
					}
				}
			}
		}
	}

	/// <summary>
	///     Fills the provided byte span with random bytes.
	/// </summary>
	public void NextBytes(Span<byte> buffer, out ValueRandom next)
	{
		next = this;
		if(buffer.Length == 0)
		{
			return;
		}

		int i = 0;

		// Generate 4 random bytes at a time to increase performance
		for(int bound = buffer.Length - 3; i < bound;)
		{
			next = next.InternalNext();
			buffer[i++] = (byte)next._w;
			buffer[i++] = (byte)(next._w >> 8);
			buffer[i++] = (byte)(next._w >> 16);
			buffer[i++] = (byte)(next._w >> 24);
		}

		// Fill up any remaining bytes in the buffer
		if(i < buffer.Length)
		{
			next = next.InternalNext();
			buffer[i++] = (byte)next._w;
			if(i < buffer.Length)
			{
				buffer[i++] = (byte)(next._w >> 8);
				if(i < buffer.Length)
				{
					buffer[i++] = (byte)(next._w >> 16);
					if(i < buffer.Length)
					{
						buffer[i + 1] = (byte)(next._w >> 24);
					}
				}
			}
		}
	}

	/// <summary>
	///     Gets a random element from the provided span.
	/// </summary>
	public T NextElement<T>(ReadOnlySpan<T> source, out ValueRandom next) where T : struct
	{
		switch(source.Length)
		{
			case 0:
				next = this;
				return default;
			case 1:
				next = this;
				return source[0];
			default:
				return source[NextInt32(source.Length, out next)];
		}
	}

	/// <summary>
	///     Gets a random element from the provided array.
	/// </summary>
	public T NextElement<T>(T[] source, out ValueRandom next)
	{
		if(source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}
		
		switch(source.Length)
		{
			case 0:
				next = this;
				return default;
			case 1:
				next = this;
				return source[0];
			default:
				return source[NextInt32(source.Length, out next)];
		}
	}

	/// <summary>
	///     Gets a random element from the provided list.
	/// </summary>
	public T NextElement<T>(IReadOnlyList<T> source, out ValueRandom next)
	{
		if(source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}
		
		switch(source.Count)
		{
			case 0:
				next = this;
				return default;
			case 1:
				next = this;
				return source[0];
			default:
				return source[NextInt32(source.Count, out next)];
		}
	}

	/// <summary>
	///     Gets a random element from the provided enumerable.
	/// </summary>
	public T NextElement<T>(IEnumerable<T> source, out ValueRandom next)
	{
		if(source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}
		
		// https://stackoverflow.com/a/648240
		T current = default;
		int count = 0;
		next = this;
		foreach(T element in source)
		{
			count++;
			if(NextInt32(count, out next) == 0)
			{
				current = element;
			}
		}

		return current;
	}
	
	/// <summary>
	///     Gets a random element from the provided enumerable. Returns the provided default value if the enumerable is empty.
	/// </summary>
	public T NextElement<T>(IEnumerable<T> source, T defaultValue, out ValueRandom next)
	{
		if(source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}
		
		// https://stackoverflow.com/a/648240
		T current = defaultValue;
		int count = 0;
		next = this;
		foreach(T element in source)
		{
			count++;
			if(NextInt32(count, out next) == 0)
			{
				current = element;
			}
		}

		return current;
	}

	/// <summary>
	///     Chooses either -1 or 1 with equal chance.
	/// </summary>
	public int MinusOneOrOne(out ValueRandom next)
	{
		return NextBool(out next) ? -1 : 1;
	}
	
	public bool Equals(ValueRandom other)
	{
		return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
	}

	public override bool Equals(object obj)
	{
		return obj is ValueRandom other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_x, _y, _z, _w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	private ValueRandom InternalNext()
	{
		uint t = _x ^ (_x << 11);
		return new ValueRandom(_y, _z, _w, _w ^ (_w >> 19) ^ t ^ (t >> 8));
	}
	
	#endregion
}
