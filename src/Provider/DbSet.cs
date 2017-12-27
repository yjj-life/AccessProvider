using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Provider.Interface;
using Provider.Interface.Implemention;

namespace Provider
{
	public class DbSet
	{
		private DbConnection _connection;
		private DbTransaction _dbTransaction;
		private bool _isTransactionStarted;

		internal DbConnection DbConnection { get { return this._connection; } }
		internal DbTransaction Transaction { get { return this._dbTransaction; } }

		public string ConnectionString
		{
			get
			{
				return _connection.ConnectionString;
			}
		}

		protected DbSet()
		{
			_connection = new SqlConnection();
			_isTransactionStarted = false;
		}

		public DbSet(string connectionString) : this()
		{
			_connection.ConnectionString = connectionString;
		}

		public DbSet(string server, string database, string username, string password) : this()
		{
			_connection.ConnectionString = BuildConnectionString(server, database, username, password);
		}

		public ITableQuery<TTable> CreateTableQuery<TTable>() where TTable : class, new()
		{
			return new TableQuery<TTable>(this);
		}

		public IQueryJoinable<TTable> CreateJoinTableQuery<TTable>() where TTable : class, new()
		{
			return new JoinTableQuery<TTable>(this);
		}

		public IUpdatable<TTable> CreateTableContext<TTable>() where TTable : class, new()
		{
			return new TableContext<TTable>(this);
		}

		public void BeginTransaction()
		{
			Open();
			if (_dbTransaction == null || _isTransactionStarted == false)
			{
				_dbTransaction = _connection.BeginTransaction();
				_isTransactionStarted = true;
			}
		}

		public void SaveChanges()
		{
			if (_dbTransaction != null && _isTransactionStarted)
			{
				_dbTransaction.Commit();
			}
			Close();
		}

		public void Cancel()
		{
			if (_dbTransaction != null && _isTransactionStarted)
			{
				_dbTransaction.Rollback();
			}
			Close();
		}

		private void Open()
		{
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
		}

		private void Close()
		{
			_connection.Close();
			_isTransactionStarted = false;
		}

		private static string BuildConnectionString(string server, string database, string username, string password)
		{
			// Create a connection string builder
			SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder
			{
				DataSource = server
			};

			// Define connection string attributes using three techniques
			csb.Add("Initial Catalog", database);
			csb.Add("User Id", username);
			csb.Add("Password", password);
			//csb["Integrated Security"] = true;

			return csb.ConnectionString;
		}

	}
}
