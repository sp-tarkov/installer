namespace SPT_AKI_Installer.Aki.Helper
{
    public class StringHelper
    {
        /// <summary>
        /// string to split, changes oldChar to newChar
        /// </summary>
        /// <param name="toSplit"></param>
        /// <param name="oldChar"></param>
        /// <param name="newChar"></param>
        /// <param name="amount"></param>
        /// <returns>returns the string at a position using amount</returns>
        public string Splitter(string toSplit, char oldChar, char newChar, int amount)
        {
            return toSplit.Replace(oldChar, newChar).Split(newChar)[^amount];
        }
    }
}
