using System;
using System.IO;
using GitT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        private class TestCommand : ICommand
        {
            public string ShortInfo => "TestCommand short description";
            public TextReader In { get; set; }
            public TextWriter Out { get; set; }
            public CommandContext Context { get; set; }
            public void Execute()
            {
                Console.WriteLine("Test command executed.");
            }
        }

        private TextWriter _outBackup;
        private StringWriter _out;

        [TestInitialize]
        public void InitializeTest()
        {
            _outBackup = Console.Out;
            _out = new StringWriter();
            Console.SetOut(_out);
        }
        [TestCleanup]
        public void CleanupTest()
        {
            Console.SetOut(_outBackup);
        }

        [TestMethod]
        public void Program_NoArguments()
        {
            // ACTION
            var command = Program.GetCommand(null);

            // ASSERT
            Assert.IsNull(command);

            _out.Flush();
            var output = _out.GetStringBuilder().ToString();

            Assert.IsTrue(output.Contains("Usage"));
            Assert.IsTrue(output.Contains("GitT <command> [command-arguments]"));
            Assert.IsTrue(output.Contains("GitT <command> </? | -? | /h | -h | /help | -help | --help>"));
            Assert.IsTrue(output.Contains("Available commands"));
            Assert.IsTrue(output.Contains("TestCommand"));
        }
        [TestMethod]
        public void Program_Help()
        {
            // ACTION
            var command = Program.GetCommand("-?");

            // ASSERT
            Assert.IsNull(command);

            _out.Flush();
            var output = _out.GetStringBuilder().ToString();

            Assert.IsTrue(output.Contains("Usage"));
            Assert.IsTrue(output.Contains("GitT <command> [command-arguments]"));
            Assert.IsTrue(output.Contains("GitT <command> </? | -? | /h | -h | /help | -help | --help>"));
            Assert.IsTrue(output.Contains("Available commands"));
            Assert.IsTrue(output.Contains("TestCommand"));
        }
        [TestMethod]
        public void Program_UnknownCommand()
        {
            // ACTION
            var command = Program.GetCommand("Command1");

            // ASSERT
            Assert.IsNull(command);

            _out.Flush();
            var output = _out.GetStringBuilder().ToString();

            Assert.IsTrue(output.Contains("Unknown command"));
            Assert.IsTrue(output.Contains("Command1"));
            Assert.IsTrue(output.Contains("Available commands"));
            Assert.IsTrue(output.Contains("TestCommand"));
        }
        [TestMethod]
        public void Program_Command()
        {
            // ACTION
            var command = Program.GetCommand("TestCommand");

            // ASSERT
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(TestCommand));
        }
    }
}
