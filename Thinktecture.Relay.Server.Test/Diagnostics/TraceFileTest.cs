using System.Collections.Generic;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.Diagnostics
{
    [TestClass]
    public class TraceFileTest
    {
        [TestMethod]
        public void IsViewable_is_false_when_headers_are_not_set()
        {
            var traceFile = new TraceFile();

            traceFile.IsViewable.Should().BeFalse();
        }

        [TestMethod]
        public void IsViewable_is_enabled_when_content_type_is_text_plain()
        {
            var traceFile = new TraceFile()
            {
                Headers = {{"Content-Type", "text/plain"}}
            };

            traceFile.IsViewable.Should().BeTrue();
        }

        [TestMethod]
        public void IsViewable_is_false_when_content_type_is_not_a_viewable_content_type()
        {
            var traceFile = new TraceFile()
            {
                Headers = {{"Content-Type", "image/png"}}
            };

            traceFile.IsViewable.Should().BeFalse();
        }

        [TestMethod]
        public void IsContentAvailable_is_false_when_no_content_length_is_set()
        {
            var traceFile = new TraceFile();

            traceFile.IsContentAvailable.Should().BeFalse();
        }

        [TestMethod]
        public void IsContentAvailable_is_false_when_content_length_is_zero()
        {
            var traceFile = new TraceFile()
            {
                Headers = {{"Content-Length", "0"}}
            };

            traceFile.IsContentAvailable.Should().BeFalse();
        }

        [TestMethod]
        public void IsContentAvailable_is_true_when_content_length_is_greater_than_zero()
        {
            var traceFile = new TraceFile()
            {
                Headers = {{"Content-Length", "10"}}
            };

            traceFile.IsContentAvailable.Should().BeTrue();
        }
    }
}