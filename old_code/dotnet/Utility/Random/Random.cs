namespace Automation.Utility
{
    /// <summary>contains methods to generate random data</summary>
    public static partial class Random
    {
        /// <summary>random number generator</summary>
        private static System.Random random = new System.Random();

        /// <summary>generates a random number in the supplied range</summary>
        /// <param name="fromInt">lowest number that can be returned</param>
        /// <param name="toInt">highest number that can be returned</param>
        /// <returns>random number in the supplied range</returns>
        public static int RandomNumber(int fromInt, int toInt)
        {
            //Random random = new Random();
            return random.Next(fromInt, toInt+1);
        }
    }
}
