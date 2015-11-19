using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SHS.Sage.Common.Linq;
using System.Linq.Expressions;
//using SHS.Sage.Common.Linq.Expressions;
using SHS.Sage.Common.OrientDb.Tests.Things;
using System.IO;
using System.Text;

namespace SHS.Sage.Common.OrientDb.Tests
{
    [TestClass]
    public class LinqTests
    {
        [TestMethod]
        public void CanCreateSimpleImplicitUpdateStatement()
        {
            var saveExpression = new SaveExpression(new Patient() { Id = "#12:1", Name = "Foo", Age = 3 });

            var queryText = Translate(saveExpression);

            // properties will appear in alphabetical order!
            Assert.IsTrue(queryText.Equals("UPDATE #12:1 SET Age=3, Name='Foo' RETURN AFTER", StringComparison.InvariantCultureIgnoreCase), queryText);
        }

        [TestMethod]
        public void CanCreateSimpleImplicitCreateStatement()
        {
            var saveExpression = new SaveExpression(new Patient() { Name = "Foo", Age = 3 });

            var queryText = Translate(saveExpression);

            // properties will appear in alphabetical order!
            Assert.IsTrue(queryText.Equals("INSERT INTO Patient (Age, Name) VALUES(3, 'Foo') RETURN @this", StringComparison.InvariantCultureIgnoreCase), queryText);
        }

        [TestMethod]
        public void CanCreateSimpleExplicitUpdateStatement()
        {
            var saveExpression = new SaveExpression(new Patient() { Id = "#12:1", Name = "Foo", Age = 3 });

            _mappingRegistrar.Add(_mapping); // species all upper case NAME

            var queryText = Translate(saveExpression);

            // properties will appear in alphabetical order!
            Assert.IsTrue(string.Compare(queryText,"UPDATE #12:1 SET Age=3, NAME='Foo' RETURN AFTER") == 0, queryText);
        }

        [TestMethod]
        public void CanCreateSimpleExplicitCreateStatement()
        {
            var saveExpression = new SaveExpression(new Patient() { Name = "Foo", Age = 3 });
            _mappingRegistrar.Add(new Mapping()); // species all upper case NAME
            var queryText = Translate(saveExpression);

            // properties will appear in alphabetical order!
            Assert.IsTrue(string.Compare(queryText, "INSERT INTO Patient (Age, NAME) VALUES(3, 'Foo') RETURN @this") == 0, queryText);
        }

        [TestMethod]
        public void CanRunMultipleTranslations()
        {
            _mappingRegistrar.Add(_mapping); // species all upper case NAME
            var saveExpression = new SaveExpression(new Patient() {Id ="#12:1", Name = "Foo", Age = 3 });
            var queryText = Translate(saveExpression);

            // properties will appear in alphabetical order!
            Assert.IsTrue(string.Compare(queryText, "UPDATE #12:1 SET Age=3, NAME='Foo' RETURN AFTER") == 0, queryText);

            queryText = Translate(saveExpression);

            // properties will appear in alphabetical order!
            Assert.IsTrue(string.Compare(queryText, "UPDATE #12:1 SET Age=3, NAME='Foo' RETURN AFTER") == 0, queryText);
        }

        [TestMethod]
        public void CanCreateSimpleExplicitDeleteStatement()
        {
            var deleteExpression = new DeleteExpression(new Patient() { Id = "#13:1" });
            var queryText = Translate(deleteExpression);
            Assert.IsTrue(string.Compare(queryText, "DELETE VERTEX #13:1") == 0);
        }

        [TestMethod]
        public void CanCreateSimpleWhereStatement()
        {
            var repo = new HealthRepository(null);
            var queryText = TranslatableExpression.Translate(_mappingRegistrar, repo.Patients.Where(p => p.Name == "test").Select(p => new Patient(){ Name = p.Name, Age = p.Age * 2 }).Expression);
            Assert.IsTrue(string.Compare(queryText, "SELECT FROM Patient WHERE Name = 'test'") == 0);
        }

        private string Translate(ITranslateExpressions translator)
        {
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, UTF8Encoding.UTF8, 1024, true))
                {
                    translator.Translate(_mappingRegistrar, sw);
                }
                ms.Position = 0;
                return UTF8Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        OMappingRegistry _mappingRegistrar;
        Mapping _mapping;
        [TestInitialize]
        public void Setup()
        {
            _mappingRegistrar = new OMappingRegistry();
            _mappingRegistrar.Clear();
            OGraphTypeBuilder.Clear();

            _mapping = new Mapping();
        }
    }
}
