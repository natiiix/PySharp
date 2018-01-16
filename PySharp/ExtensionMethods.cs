namespace PySharp
{
    public static class ExtensionMethods
    {
        public static string Repeat(this string str, int count)
        {
            string result = string.Empty;

            for (int i = 0; i < count; i++)
            {
                result += str;
            }

            return result;
        }
    }
}