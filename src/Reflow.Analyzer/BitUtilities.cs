namespace Reflow.Analyzer
{
    internal static class BitUtilities
    {
        internal static TypeCode GetTypeBySize(int size)
        {
            return size switch
            {
                <= sizeof(byte) * 8 => TypeCode.Byte,
                <= sizeof(ushort) * 8 => TypeCode.UInt16,
                <= sizeof(uint) * 8 => TypeCode.UInt32,
                _ => TypeCode.UInt64,
            };
        }
    }
}
