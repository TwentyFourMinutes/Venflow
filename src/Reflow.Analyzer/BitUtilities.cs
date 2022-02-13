namespace Reflow.Analyzer
{
    internal static class BitUtilities
    {
        internal static TypeCode GetTypeBySize(int size)
        {
            const int byteSize = 8;

            return size switch
            {
                <= sizeof(byte) * byteSize => TypeCode.Byte,
                <= sizeof(ushort) * byteSize => TypeCode.UInt16,
                <= sizeof(uint) * byteSize => TypeCode.UInt32,
                _ => TypeCode.UInt64,
            };
        }
    }
}
