namespace PySharp
{
    public struct Definition
    {
        public string Name { get; }
        public string Value { get; }

        public Definition(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static bool TryParse(string str, out Definition def)
        {
            // #define {Name} {Value}

            string cleaned = str.Trim(' ');

            string[] parts = cleaned.Split(new char[] { ' ' }, 3, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 3 && parts[0] == "#define")
            {
                def = new Definition(parts[1], parts[2]);
                return true;
            }

            def = default(Definition);
            return false;
        }
    }
}