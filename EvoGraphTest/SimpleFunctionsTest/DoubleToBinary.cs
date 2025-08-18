namespace EvoGraphTest.SimpleFunctionsTest;

public static class DoubleToBinary
{
    public static string ToBinaryString(this double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        return Convert.ToString(bits, 2).PadLeft(64, '0');
    }
    
    public static double BinaryToDouble(this string str) 
    {
        var newBits = Convert.ToInt64(str, 2);
        return BitConverter.Int64BitsToDouble(newBits);
    }
}