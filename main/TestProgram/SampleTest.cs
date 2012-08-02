using Automation.Testing.iOS;

namespace TestProgram
{
    /// <summary>abstract class for any test that tests the application TestApp</summary>
    public abstract class TestAppTest : iOSTest
    {
        public override iOSClient.Configuration Config { get { return new iOSClient.Configuration(iOSDevice.iPhone, "", "", "TestApp.xcodeproj", "TestApp.app", "TestApp", "iphonesimulator5.1", null, null); } }
    }

    /// <summary>sample test of TestApp</summary>
    public class SampleTest : TestAppTest
    {
        /// <summary>name of the test</summary>
        public override string Name { get { return "iPhone Login Test"; } }
        
        /// <summary>text field for integer a</summary>
        public static iOSHook IntegerATextField = new iOSHook("mainWindow.textFields()[0]");
        /// <summary>test field for integer b</summary>
        public static iOSHook IntegerBTextField = new iOSHook("mainWindow.textFields()[1]");
        /// <summary>button the user taps to compute the sum</summary>
        public static iOSHook ComputeSumButton = new iOSHook("mainWindow.buttons()[\"Compute Sum\"]");
        /// <summary>label which displays the sum after its computed</summary>
        public static iOSHook ResultLabel = new iOSHook("mainWindow.staticTexts()[0]");

        /// <summary>constructor</summary>
        public SampleTest() : base()
        {

        }

        /// <summary>main test execution method</summary>
        /// <returns>true if successful</returns>
        public override bool Run()
        {
            int randA = Automation.Utility.Random.RandomNumber(0, 100);
            int randB = Automation.Utility.Random.RandomNumber(0, 100);

            Client.RunBatchJob(
                () => Client.SetValue(IntegerATextField, randA.ToString()),
                () => Client.SetValue(IntegerBTextField, randB.ToString()),
                () => Client.Tap(ComputeSumButton)
            );
            // previous statement could alternatively be written like this for a non-batched implementation
            //Client.SetValue(IntegerATextField, randA.ToString());
            //Client.SetValue(IntegerBTextField, randB.ToString());
            //Client.Tap(ComputeSumButton);

            Log.Verify((randA + randB).ToString(), Client.GetValue(ResultLabel), "Verifying the sum was computed correctly.");
            return Log.HasNoErrorsOrFailures;
        }
    }
}
