using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using Provider.Sql;
using Provider.Error;

namespace Provider.Excution
{
	internal class Update
	{
		DbConnection connection;

		public Update(DbConnection connection)
		{
			this.connection = connection;
		}

		internal DbError Excute<TTable>(string sqlCommand, List<Parameter> parameters, DbTransaction transaction) where TTable : new()
		{
			DbError dbError = new DbError();

			int result = -1;
			try
			{
				// Create the command and open the connection
				var command = connection.CreateCommand();
				command.CommandText = sqlCommand;
				command.CommandTimeout = 15;
				command.CommandType = CommandType.Text;
				command.Transaction = transaction;
				command.Parameters.Clear();
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

				result = command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				dbError.Code = ErrorCode.DatabaseException;
				dbError.Text = e.Message;
			}
			finally
			{
				if (result == 0)
				{
					dbError.Code = ErrorCode.Conflict;
					dbError.Text = "The record has be changed.";
				}
			}

			return dbError;
		}

		internal DbError ExcuteBatch<TTable>(string sqlCommand, List<List<Parameter>> listParameters, DbTransaction transaction) where TTable : new()
		{
			DbError dbError = new DbError();

			int result = -1;
			try
			{
				// Create the command and open the connection
				var command = connection.CreateCommand();
				command.CommandText = sqlCommand;
				command.CommandTimeout = 15;
				command.CommandType = CommandType.Text;
				command.Transaction = transaction;
				command.Parameters.Clear();
				if (listParameters != null)
				{
					for (int index = 0; index < listParameters.Count; index++)
					{
						result = -1;
						List<Parameter> parameters = listParameters[index];
						for (int i = 0; i < parameters.Count; i++)
						{
							if (index == 0)
							{
								var dbParameter = command.CreateParameter();
								dbParameter.ParameterName = parameters[i].Name;
								dbParameter.Value = parameters[i].Value;

								command.Parameters.Add(dbParameter);
							}
							else
							{
								var dbParameter = command.Parameters[i];
								dbParameter.Value = parameters[i].Value;
							}
						}
						result = command.ExecuteNonQuery();
						if (result == 0)
						{
							break;
						}
					}
				}
			}
			catch(Exception e)
			{
				dbError.Code = ErrorCode.DatabaseException;
				dbError.Text = e.Message;
			}
			finally
			{
				if (result == 0)
				{
					dbError.Code = ErrorCode.Conflict;
					dbError.Text = "The record has be changed.";
				}
			}

			return dbError;
		}
	}
}
