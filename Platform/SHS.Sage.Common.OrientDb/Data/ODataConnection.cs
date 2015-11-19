using Orient.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Data
{
    public class ODataConnection : DbConnection
    {
        ODatabase _database;
        public ODataConnection(ODatabase database)
        {
            _database = database;
        }

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override string ConnectionString
        {
            get;
            set;
        }

        protected override DbCommand CreateDbCommand()
        {
            return new ODataCommand(this._database);
        }

        public override string DataSource
        {
            get { throw new NotImplementedException(); }
        }

        public override string Database
        {
            get { throw new NotImplementedException(); }
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public override string ServerVersion
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Data.ConnectionState State
        {
            get { throw new NotImplementedException(); }
        }
    }
}
