namespace Newegg.OZZO.DataAccess
{
	using Common;
	using Microsoft.Practices.EnterpriseLibrary.Data;
	using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Data.SqlClient;
	using System.Globalization;
	using Utility;

	public static class EnterpriseDataAccess
	{
		static readonly int s_DefaultCommandTimeout = 60;

		#region Execute SQL List

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nonQuerySqlList"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static bool ExecuteNonQuery(IList<string> nonQuerySqlList, string serverName = "")
		{
			bool succeed = false;
			if (nonQuerySqlList.OZZOSafeCheckValue())
			{
				Database database = GetSqlDatabase(serverName);
				if (database == null)
				{
					return succeed;
				}
				using (DbConnection dbconn = database.CreateConnection())
				{
					dbconn.Open();
					DbTransaction dbTran = dbconn.BeginTransaction();
					try
					{
						foreach (string sql in nonQuerySqlList)
						{
							if (string.IsNullOrWhiteSpace(sql))
							{
								continue;
							}
							DbCommand dbCommand = database.GetSqlStringCommand(sql);
							if (dbCommand != null)
							{
								database.ExecuteNonQuery(dbCommand);
							}
						}
						dbTran.Commit();
						succeed = true;
					}
					catch
					{
						dbTran.Rollback();
						throw;
					}
				}
			}
			return succeed;
		}

		#endregion

		#region Execute SQL With Parameters

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nonQuerySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static bool ExecuteNonQuery(string nonQuerySql, IList<SqlParameter> parameterList = null, int commandTimeout = 0, string serverName = "")
		{
			bool successful = false;
			if (string.IsNullOrEmpty(nonQuerySql))
			{
				return successful;
			}
			Database database = GetSqlDatabase(serverName);
			if (database == null)
			{
				return successful;
			}
			if (commandTimeout < 1)
			{
				commandTimeout = s_DefaultCommandTimeout;
			}
			using (DbCommand dbCommand = database.GetSqlStringCommand(nonQuerySql))
			{
				dbCommand.CommandTimeout = commandTimeout;
				BuildDBParameter(database, dbCommand, parameterList);
				successful = database.ExecuteNonQuery(dbCommand) > 0;
			}
			return successful;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scalarSql"></param>
		/// <param name="parameterList"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static T ExecuteScalar<T>(string scalarSql, IList<SqlParameter> parameterList = null, int commandTimeout = 0, string serverName = "")
		{
			T scalarValue = default(T);
			if (string.IsNullOrEmpty(scalarSql))
			{
				return scalarValue;
			}
			Database database = GetSqlDatabase(serverName);
			if (database == null)
			{
				return scalarValue;
			}
			if (commandTimeout < 1)
			{
				commandTimeout = s_DefaultCommandTimeout;
			}
			using (DbCommand dbCommand = database.GetSqlStringCommand(scalarSql))
			{
				dbCommand.CommandTimeout = commandTimeout;
				BuildDBParameter(database, dbCommand, parameterList);
				object obj = database.ExecuteScalar(dbCommand);
				scalarValue = (T)(obj == DBNull.Value ? null : obj);
			}
			return scalarValue;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static IDataReader ExecuteReader(string querySql, IList<SqlParameter> parameterList = null, int commandTimeout = 0, string serverName = "")
		{
			IDataReader reader = null;
			if (string.IsNullOrEmpty(querySql))
			{
				return reader;
			}
			Database database = GetSqlDatabase(serverName);
			if (database == null)
			{
				return reader;
			}
			if (commandTimeout < 1)
			{
				commandTimeout = s_DefaultCommandTimeout;
			}
			using (DbCommand dbCommand = database.GetSqlStringCommand(querySql))
			{
				dbCommand.CommandTimeout = commandTimeout;
				BuildDBParameter(database, dbCommand, parameterList);
				reader = database.ExecuteReader(dbCommand);
			}
			return reader;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static DataSet ExecuteDataSet(string querySql, IList<SqlParameter> parameterList = null, int commandTimeout = 0, string serverName = "")
		{
			DataSet returnData = null;
			if (string.IsNullOrEmpty(querySql))
			{
				return returnData;
			}
			Database database = GetSqlDatabase(serverName);
			if (database == null)
			{
				return returnData;
			}
			if (commandTimeout < 1)
			{
				commandTimeout = s_DefaultCommandTimeout;
			}
			using (DbCommand dbCommand = database.GetSqlStringCommand(querySql))
			{
				dbCommand.CommandTimeout = commandTimeout;
				BuildDBParameter(database, dbCommand, parameterList);
				returnData = database.ExecuteDataSet(dbCommand);
			}
			return returnData;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="timeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static DataSet ExecuteReaderDataSet(string querySql, IList<SqlParameter> parameterList = null, int timeout = 0, string serverName = "")
		{
			DataSet returnDataSet = new DataSet();
			returnDataSet.Locale = CultureInfo.InvariantCulture;
			IDataReader reader = ExecuteReader(querySql, parameterList, timeout, serverName);
			if (reader == null)
			{
				return returnDataSet;
			}
			using (reader)
			{
				returnDataSet.Load(reader, LoadOption.PreserveChanges, string.Empty);
			}
			return returnDataSet;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="timeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static DataTable ExecuteDataTable(string querySql, IList<SqlParameter> parameterList = null, int timeout = 0, string serverName = "")
		{
			return ExecuteReaderDataSet(querySql, parameterList, timeout, serverName).OZZOSafeFirstDataTable();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="timeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static DataRow ExecuteDataRow(string querySql, IList<SqlParameter> parameterList = null, int timeout = 0, string serverName = "")
		{
			return ExecuteDataTable(querySql, parameterList, timeout, serverName).OZZOSafeFirstDataRow();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="timeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static IEnumerable<DataRow> ExecuteDataRowList(string querySql, IList<SqlParameter> parameterList = null, int timeout = 0, string serverName = "")
		{
			return ExecuteDataTable(querySql, parameterList, timeout, serverName).OZZOSafeDataRowList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="timeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static T ExecuteObject<T>(string querySql, IList<SqlParameter> parameterList = null, int timeout = 0, string serverName = "")
			where T : class, new()
		{
			return SimpleORM.BindDataRowToObject<T>(ExecuteDataRow(querySql, parameterList, timeout, serverName));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="querySql"></param>
		/// <param name="parameterList"></param>
		/// <param name="timeout"></param>
		/// <param name="serverName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static IList<T> ExecuteObjectList<T>(string querySql, IList<SqlParameter> parameterList = null, int timeout = 0, string serverName = "")
			where T : class, new()
		{
			return SimpleORM.BindDataRowListToObject<T>(ExecuteDataRowList(querySql, parameterList, timeout, serverName));
		}

        /// <summary>
        /// Execute sql return object list
		/// </summary>
		/// <typeparam name="T">Return object type</typeparam>
		/// <param name="querySql">The execute sql</param>
		/// <param name="serverName">The service name or connect string</param>
		/// <param name="parameterList">The sql parameter list</param>
		/// <param name="rowMapper">Object and parameter mapping builder</param>
		/// <param name="timeout">Execute sql timeout</param>
		/// <returns>The object list</returns>
		public static IEnumerable<T> ExecuteQuery<T>(string querySql, string serverName, IList<SqlParameter> parameterList = null, IRowMapper<T> rowMapper = null, int timeout = 0)
            where T : class, new()
        {
            IEnumerable<T> objectList = new List<T>();
            if (string.IsNullOrEmpty(querySql))
            {
                return objectList;
            }
            Database database = GetSqlDatabase(serverName);
            if (rowMapper == null)
            {
                rowMapper = MapBuilder<T>.BuildAllProperties();
            }
            objectList = database.CreateSqlStringAccessor(querySql, new ParameterMapper(parameterList, timeout), rowMapper).Execute();
            return objectList;
        }

        #endregion

        #region Dababase object

        static void BuildDBParameter(Database database, DbCommand dbCommand, IList<SqlParameter> cmdParms)
		{
			if (database == null || dbCommand == null || (!cmdParms.OZZOSafeCheckValue()))
			{
				return;
			}
			foreach (SqlParameter param in cmdParms)
			{
				if (param != null)
				{
					if (param.Direction == ParameterDirection.Input)
					{
						database.AddInParameter(dbCommand, param.ParameterName, param.DbType, param.Value);
					}
					else
					{
						database.AddOutParameter(dbCommand, param.ParameterName, param.DbType, param.Size);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceName"></param>
		/// <exception cref="Exception"></exception>
		/// <returns></returns>
		public static SqlDatabase GetSqlDatabase(string serviceName)
		{
			string connectString = DatabaseHelper.SafeGetConnectionString(serviceName);
			if (string.IsNullOrEmpty(connectString))
			{
				throw new BusinessException("Connect string can not be empty.");
			}
			return new SqlDatabase(connectString);
		}

		#endregion
	}
}
