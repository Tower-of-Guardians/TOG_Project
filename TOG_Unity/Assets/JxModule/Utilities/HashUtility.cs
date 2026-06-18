namespace JxModule
{
    public static class HashUtility
    {
        public static float Hash11(uint seed)
        {
            seed ^= seed >> 17;
            seed *= 0xED5AD4BBu;
            seed ^= seed >> 11;
            seed *= 0xAC4C1B51u;
            seed ^= seed >> 15;
            seed *= 0x31848BABu;
            seed ^= seed >> 14;

            return seed / 4294967295f;
        }

        public static float Hash11Signed(uint seed)
        {
            return Hash11(seed) * 2f - 1f;
        }
    }
}