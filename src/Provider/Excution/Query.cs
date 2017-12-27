using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Provider.Error;
using Provider.Sql;
using Provider.ExpFunc;

namespace Provider.Excution
{
	internal class Query
	{
		DbConnection connection;
		internal DbError dbError;

		internal Query(DbConnection connection)
		{
			this.connection = connection;
		}

		internal List<RecordCache<TTable>> Excute<TTable>(string sqlCommand, List<Parameter> parameters, List<bool> fieldsUsable) where TTable : new()
		{
			dbError = new DbError();

			if (string.IsNullOrEmpty(connection.ConnectionString))
			{
				return null;
			}

			Func<DbDataReader, TTable> readRowFunc = ExpressionFunc.GetReader<TTable>(fieldsUsable);
			Func<TTable, TTable> cloneFunc = ExpressionFunc.PrimaryKeyClone<TTable>();
			if (readRowFunc == null || cloneFunc == null)
			{
				return null;
			}

			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			try
			{
				// Create the command and open the connection
				var command = connection.CreateCommand();
				command.CommandText = sqlCommand;
				command.CommandTimeout = 15;
				command.CommandType = CommandType.Text;
				if (parameters != null)
				{
					for (int index = 0; index < parameters.Count; index++)
					{
						var dbParameter = command.CreateParameter();
						dbParameter.ParameterName = parameters[index].Name;
						dbParameter.Value = parameters[index].Value;

						command.Parameters.Add(dbParameter);
					}
				}

				// Create the DataReader to retrieve data
				using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
				{
					return ReadRecords<TTable>(dr, readRowFunc, cloneFunc);
				}
			}
			catch(Exception e)
			{
				dbError.Code = ErrorCode.DatabaseException;
				dbError.Text = e.Message;

				return null;
			}
		}

		// 读取所有记录
		private List<RecordCache<TTable>> ReadRecords<TTable>(DbDataReader reader, Func<DbDataReader, TTable> readRowFunc, Func<TTable, TTable> cloneFunc)
		{
			List<RecordCache<TTable>> recordsCache = new List<RecordCache<TTable>>();

			Stopwatch watch = new Stopwatch();
			watch.Start();

			while (reader.Read())
			{
				// 读取数据并分配到TableClass中
				TTable record = readRowFunc(reader);

				recordsCache.Add(new RecordCache<TTable>
				{
					recordOut = record,
					recordCache = cloneFunc(record)
				});
			}

			watch.Stop();
			var timespan = watch.ElapsedMilliseconds.ToString();

			return recordsCache;
		}
	}
}
