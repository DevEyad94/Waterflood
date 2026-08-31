namespace BackEndWaterFloodApp.Constants
{
    public static class Constant
    {
        private static string _environment = string.Empty;

        public static void SetEnvironment(string environment)
        {
            _environment = environment;
        }
    }
}
