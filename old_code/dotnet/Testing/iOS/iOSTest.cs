namespace Automation.Testing.iOS
{
    /// <summary>test that is run using the ios device</summary>
    public abstract class iOSTest : Test
    {
        /// <summary>ios client configuration</summary>
        public abstract iOSClient.Configuration Config { get; }

        /// <summary>represents an ios test</summary>
        public iOSTest() : base()
        {

        }

        /// <summary>test client used to execute the test</summary>
        public iOSClient Client { get; set; }

        /// <summary>sets up the test</summary>
        /// <returns>true if setup completes</returns>
        public override bool ClassSetup()
        {
            Client = new iOSClient(Config, Log);
            Log.AddListener(new iOSLogListener(Client));
            return Client != null;
        }

        /// <summary>tears down the iphone test</summary>
        /// <returns>true if tear down completes</returns>
        public override bool ClassTeardown()
        {
            bool result = Client.StopAutomation();
            Client.Teardown();
            return result;
        }
    }
}