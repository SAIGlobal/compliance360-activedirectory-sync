using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Filters;
using NUnit.Framework;
using StructureMap.Graph.Scanning;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class UacAttributeFilterTest
    {
        [Test]
        public void TestUacFilterNullValue()
        {
            var filter = new UacAttributeFilter();

            var job = new JobElement();

            var attrib = new AttributeElement();

            var searchResult = SearchResultFactory.Construct(new
            {
                userAccountControl = 0
            });

            var result = (bool)filter.Execute(null, searchResult, job, attrib);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestUacFilterIsInactive()
        {
            var filter = new UacAttributeFilter();

            var job = new JobElement();

            var attrib = new AttributeElement();

            var searchResult = SearchResultFactory.Construct(new
            {
                userAccountControl = 0
            });

            const int currentValue = 0x0002;
            var result = (bool)filter.Execute(currentValue, searchResult, job, attrib);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestUacFilterIsActive()
        {
            var filter = new UacAttributeFilter();

            var job = new JobElement();

            var attrib = new AttributeElement();

            var searchResult = SearchResultFactory.Construct(new
            {
                userAccountControl = 0
            });

            const int currentValue = 0x0001;
            var result = (bool)filter.Execute(currentValue, searchResult, job, attrib);
            Assert.IsTrue(result);
        }
    }
}
