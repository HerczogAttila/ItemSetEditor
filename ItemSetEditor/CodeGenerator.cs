using System;

namespace ItemSetEditor
{
    public static class CodeGenerator
    {
        private static Random rnd = new Random();
        private static string[] code = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        private static string GeneratePart(int c)
        {
            string ve = "";
            for (int i = 0; i < c; i++)
                ve += code[rnd.Next(code.Length)];

            return ve;
        }
        public static string Generate()
        {
            return Generate(8, 4, 4, 4, 12);
        }
        public static string Generate(params int[] pars)
        {
            string ve = "LOL";
            if (pars != null)
                foreach (int i in pars)
                    ve += "_" + GeneratePart(i);

            return ve;
        }
    }
}
