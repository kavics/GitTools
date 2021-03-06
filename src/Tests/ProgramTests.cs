﻿using System;
using System.IO;
using GitT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Tools.CommandLineArguments;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        #region Nested classes
        private class TestCmd1 : ICommand
        {
            public string ShortInfo => "TestCmd1 short description";
            public TextReader In { get; set; }
            public TextWriter Out { get; set; }
            public CommandContext Context { get; set; }

            public void Execute()
            {
                if (Context.ParseArguments(out TestCommandArgs _))
                    return;

                Console.WriteLine("Test command executed.");
            }
        }
        private class TestCommandArgs
        {
            [CommandLineArgument("String", false, "s,S", "HelpText of TestCommand")]
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedMember.Global
            public string StringParam { get; set; }
        }
        private class TestCmd2Command : ICommand
        {
            public string ShortInfo => "TestCmd2 short description";
            public TextReader In { get; set; }
            public TextWriter Out { get; set; }
            public CommandContext Context { get; set; }

            public void Execute()
            {
                if (Context.ParseArguments(out TestCommandArgs _))
                    return;

                Console.WriteLine("Test command executed.");
            }
        }
        #endregion

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
            Assert.IsTrue(output.Contains("TestCmd1"));
            Assert.IsTrue(output.Contains("TestCmd2"));
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
            Assert.IsTrue(output.Contains("TestCmd1"));
            Assert.IsTrue(output.Contains("TestCmd2"));
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
            Assert.IsTrue(output.Contains("TestCmd1"));
            Assert.IsTrue(output.Contains("TestCmd2"));
        }

        [TestMethod]
        public void Program_Command()
        {
            // ACTION
            var command = Program.GetCommand("TestCmd1");

            // ASSERT
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(TestCmd1));
        }
        [TestMethod]
        public void Program_CommandSuffix()
        {
            // ACTION
            var command1 = Program.GetCommand("TestCmd1");
            var command2 = Program.GetCommand("TestCmd2");

            // ASSERT
            Assert.IsNotNull(command1);
            Assert.IsInstanceOfType(command1, typeof(TestCmd1));
            Assert.IsNotNull(command2);
            Assert.IsInstanceOfType(command2, typeof(TestCmd2Command));
        }
    }
}
