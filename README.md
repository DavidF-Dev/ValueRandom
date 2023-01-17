# ValueRandom
Random number generator implemented as a struct in C#.

Based off of [NetRandom - A fast random number generator for .NET](https://github.com/DV8FromTheWorld/C-Sharp-Programming/blob/master/LIBRARIES/Lidgren.Network/Lidgren.Network/NetRandom.cs) by Colin Green (January, 2005).
Algorithm based on a simple and fast xor-shift pseudo random number generator (George Marsaglia, 2003).

- No heap allocation or boxing.
- Up to 8x faster than System.Random depending on which methods are called.
- Light-weight; only 128 bytes.
- Re-initialisation is as easy as creating a new instance with a different seed.

## Examples
```cs
ValueRandom rng = new(Environment.TickCount);
uint sample = rng.NextUInt32(out rng);
uint nextSample = rng.NextUInt32(out rng);
if (rng.NextBool(out rng))
{
  // etc.
```
The methods in ``ValueRandom`` provide the next instance for further use.

```cs
ValueRandom rng = new(Environment.TickCount);
rng.NextDouble(out _) == rng.NextDouble(out _); // TRUE
rng.NextDouble(out rng) == rng.NextDouble(out _); // FALSE
rng.NextDouble(out _) == rng.NextDouble(out rng); // FALSE
rng.NextDouble(out rng) == rng.NextDouble(out rng); // FALSE
```

## Contact
If you have any questions or would like to get in contact, shoot me an email at contact@davidfdev.com. Alternatively, you can send me a direct message on Twitter at [@DavidF_Dev](https://twitter.com/DavidF_Dev).</br></br>
Consider showing support by [buying me a bowl of spaghetti](https://www.buymeacoffee.com/davidfdev) 🍝</br>
View my other projects on my [website](https://www.davidfdev.com/tools) 🔨
