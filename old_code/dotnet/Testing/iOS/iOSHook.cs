namespace Automation.Testing.iOS
{
    /// <summary>represents a hook into an ios application</summary>
    public class iOSHook
    {
        /// <summary>javascript code to locate the hook</summary>
        private string _Hook;

        /// <summary>constructor</summary>
        /// <param name="locator">locator for the hook</param>
        public iOSHook(string locator)
        {
            _Hook = locator;
        }

        /// <summary>implicitly castes a string to a an ioshook</summary>
        /// <param name="locator">locator for the hook</param>
        /// <returns>a hook corresponding to the supplied string</returns>
        public static implicit operator iOSHook(string locator)
        {
            return locator == null ? null : new iOSHook(locator);
        }

        /// <summary>converts to a string representation</summary>
        /// <returns>string representation of the hook</returns>
        public override string ToString()
        {
            return _Hook;
        }
    }
}
