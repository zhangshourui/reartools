using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace Utility
{
    /// <summary>
    /// 数据访问抽象基础类
    /// Copyright (C) 2016 By Danny
    /// </summary>
    public class DbHelperMysql
    {
        private Log5 log = new Log5();
        private string SlowQueryLog = System.IO.Path.Combine(Log5.BaseLogPath, "%y-%M\\slow-querys\\%y-%M-%d-slow-querys.log");
        //  private string SlowQueryLog = "slow-querys";// System.IO.Path.Combine(Log5.BaseLogPath, "%y-%M\\%f\\%y-%M-%d-slow-querys.log");
        private int SlowQueryThreshold = 3000;// 3秒

        //数据库连接字符串(web.config来配置)，可以动态更改ConnectionString支持多数据库.
        public string ConnectionString { get; private set; }

        // public static string conn = ConfigurationManager.ConnectionStrings["LCS_CoreRead"].ConnectionString.ToString();

        public int CommandTimeout = PubConstant.CommandTimeoutMysql;

        public DbHelperMysql(string connStr)
        {
            log.IsDebugEnabled = true;

            //  log.CreateLog(SlowQueryLog, SlowQueryLog + ".log", true, ShowFileType.none);
            // log.SetAutoGenerateFilename(SlowQueryLog, true, "log\\", "%y-%M\\%f\\%y-%M-%d-%src.log");
            log.CreateLog(new LogEntryConfig() { FilePathTemplate = SlowQueryLog, ShowFile = Log5.ShowFileType.None });

            ConnectionString = connStr;
        }

        public MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            return connection;
        }

        #region 公用方法

        public int GetMaxID(string FieldName, string TableName)
        {
            var strsql = string.Format("select max({0})+1 from {1}", FieldName, TableName);
            var obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        public bool Exists(string strSql)
        {
            var obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public bool TabExists(string TableName)
        {
            var strsql = string.Format("select count(*) from INFORMATION_SCHEMA.tables where table_type = 'base table' and table_name = '{0}'", TableName.MysqlParamFilter());

            //string strsql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + TableName + "]') AND type in (N'U')";
            var obj = GetSingle(strsql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool Exists(string strSql, params IDbDataParameter[] cmdParms)
        {
            var obj = GetSingle(strSql, null, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion 公用方法

        #region 执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                using (var cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        public int ExecuteSqlByTime(string SQLString, int Times)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                using (var cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>
        public int ExecuteSqlTran(List<string> SQLStringList)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    var tx = conn.BeginTransaction();
                    cmd.Transaction = tx;
                    try
                    {
                        var count = 0;
                        for (var n = 0; n < SQLStringList.Count; n++)
                        {
                            var strsql = SQLStringList[n];
                            if (strsql.Trim().Length > 1)
                            {
                                cmd.CommandText = strsql;
                                count += cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                        return count;
                    }
                    catch
                    {
                        tx.Rollback();
                        return 0;
                    }
                }
            }
        }

        public void AddInParameter(DbCommand cmd, string dbParamName, DbType dbType, object value, int size = int.MaxValue)
        {
            var dbparam = cmd.CreateParameter();
            dbparam.ParameterName = dbParamName;
            dbparam.DbType = dbType;
            if (size != int.MaxValue)
            {
                dbparam.Size = size;
            }

            dbparam.Value = value ?? DBNull.Value;

            //if (dbType == DbType.Time)
            //{
            //    //   dbType = (DbType)(int)System.Data.SqlDbType.Time;
            //    var param = dbparam as SqlParameter;
            //    param.SqlDbType = SqlDbType.Time;
            //}
            cmd.Parameters.Add(dbparam);
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, IDbTransaction trans = null)
        {
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new MySqlCommand(SQLString, connection as MySqlConnection))
                {
                    try
                    {
                        if (trans != null)
                        {
                            cmd.Transaction = trans as MySqlTransaction;
                        }

                        connection.Open();
                        var obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        log.Error("{0}\r\n{1}", SQLString, e.ToString());
                        connection.Close();
                        throw e;
                    }
                }
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }
            }
        }

        public object GetSingle(string SQLString, int Times, IDbTransaction trans = null)
        {
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new MySqlCommand(SQLString, connection as MySqlConnection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        if (trans != null)
                        {
                            cmd.Transaction = trans as MySqlTransaction;
                        }

                        var obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, IDbTransaction trans = null)
        {
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            var ticks = System.Environment.TickCount;
            try
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    using (var command = new MySqlDataAdapter(SQLString, connection as MySqlConnection))
                    {
                        if (trans != null)
                        {
                            command.SelectCommand.Transaction = trans as MySqlTransaction;
                        }

                        command.SelectCommand.CommandTimeout = CommandTimeout;
                        command.Fill(ds, "ds");
                    }
                }
                catch (SqlException ex)
                {
                    log.Error("{0}\r\n{1}", SQLString, ex.ToString());
                    throw new Exception(ex.Message);
                }
                return ds;
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }

                if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                {
                    log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={SQLString}");
                }
            }
        }

        public DataSet Query(string SQLString, int Timeout, IDbTransaction trans = null)
        {
            var ticks = System.Environment.TickCount;
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    using (var command = new MySqlDataAdapter(SQLString, connection as MySqlConnection))
                    {
                        if (trans != null)
                        {
                            command.SelectCommand.Transaction = trans as MySqlTransaction;
                        }

                        command.SelectCommand.CommandTimeout = Timeout;
                        command.Fill(ds, "ds");
                    }
                }
                catch (SqlException ex)
                {
                    log.Error("{0}\r\n{1}", SQLString, ex.ToString());
                    throw new Exception(ex.Message);
                }
                return ds;
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }

                if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                {
                    log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={SQLString}");
                }
            }
        }

        #endregion 执行简单SQL语句

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, params IDbDataParameter[] cmdParms)
        {
            var ticks = System.Environment.TickCount;
            using (var connection = new MySqlConnection(ConnectionString))
            {
                using (var cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        var rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        log.Error("{0}\r\n{1}", SQLString, e.ToString());
                        throw e;
                    }
                    finally
                    {
                        if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                        {
                            log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={SQLString} \r\n {DumpParameter(cmdParms)}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new MySqlCommand())
                    {
                        try
                        {
                            //循环
                            foreach (DictionaryEntry myDE in SQLStringList)
                            {
                                var cmdText = myDE.Key.ToString();
                                var cmdParms = (SqlParameter[])myDE.Value;
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                var val = cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public int ExecuteSqlTran(List<DBCommandInfo> cmdList)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new MySqlCommand())
                    {
                        try
                        {
                            var count = 0;

                            //循环
                            foreach (var myDE in cmdList)
                            {
                                var cmdText = myDE.CommandText;
                                var cmdParms = (MySqlParameter[])myDE.Parameters;
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
                                {
                                    if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                                    {
                                        trans.Rollback();
                                        return 0;
                                    }
                                    var obj = cmd.ExecuteScalar();
                                    var isHave = false;
                                    if (obj == null && obj == DBNull.Value)
                                    {
                                        isHave = false;
                                    }

                                    isHave = Convert.ToInt32(obj) > 0;
                                    if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
                                    {
                                        trans.Rollback();
                                        return 0;
                                    }
                                    if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
                                    {
                                        trans.Rollback();
                                        return 0;
                                    }
                                    continue;
                                }
                                var val = cmd.ExecuteNonQuery();
                                count += val;
                                if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
                                {
                                    trans.Rollback();
                                    return 0;
                                }
                                cmd.Parameters.Clear();
                            }
                            trans.Commit();
                            return count;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, IDbTransaction trans = null, params IDbDataParameter[] cmdParms)
        {
            var ticks = System.Environment.TickCount;
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, trans, SQLString, cmdParms);
                        var obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        log.Error("{0}\r\n{1}", SQLString, e.ToString());
                        throw e;
                    }
                }
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }

                if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                {
                    log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={SQLString} \r\n {DumpParameter(cmdParms)}");
                }
            }
        }

        public DataSet Query(string SQLString, IDbTransaction trans = null, params IDbDataParameter[] cmdParms)
        {
            return Query(SQLString, CommandTimeout, trans, cmdParms);
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, int timeout, IDbTransaction trans = null, params IDbDataParameter[] cmdParms)
        {
            var ticks = System.Environment.TickCount;
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandTimeout = timeout;

                    PrepareCommand(cmd, connection, trans, SQLString, cmdParms);
                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        var ds = new DataSet();
                        try
                        {
                            da.Fill(ds, "ds");
                            cmd.Parameters.Clear();
                        }
                        catch (SqlException ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        return ds;
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Error("{0}\r\n{1}", SQLString, ex.ToString());
                throw;
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }

                if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                {
                    log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={SQLString} \r\n {DumpParameter(cmdParms)}");
                }
            }
        }

        private void PrepareCommand(IDbCommand cmd, IDbConnection conn, IDbTransaction trans, string cmdText, IDbDataParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }

            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (var parameter in cmdParms)
                {
                    //if (parameter == null)
                    //    continue;
                    //if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                    //    (parameter.Value == null))
                    //{
                    //    parameter.Value = DBNull.Value;
                    //}
                    //cmd.Parameters.Add(parameter);
                    if (parameter != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value.
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }

                        //command.Parameters.Add(parameter);
                        if (parameter.GetType() != typeof(MySqlParameter))
                        {
                            var mysqlPar = new MySqlParameter
                            {
                                ParameterName = parameter.ParameterName,
                                MySqlDbType = GetMySqlDbType(parameter.DbType),
                                Value = parameter.Value,

                                //mysqlPar.Size = parameter.Size;
                                Direction = parameter.Direction
                            };
                            cmd.Parameters.Add(mysqlPar);
                        }
                        else
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }
                }
            }
        }

        #endregion 执行带参数的SQL语句

        #region 存储过程操作

        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, IDbTransaction trans, out int returnvalue)
        {
            var ticks = System.Environment.TickCount;
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                if (trans == null)
                {
                    connection.Open();
                }

                var result = 0;
                var dataSet = new DataSet();
                using (var sqlDA = new MySqlDataAdapter
                {
                    SelectCommand = BuildQueryCommand(connection, storedProcName, parameters) as MySqlCommand
                })
                {
                    if (trans != null)
                    {
                        sqlDA.SelectCommand.Transaction = trans as MySqlTransaction;
                    }

                    sqlDA.Fill(dataSet, tableName);

                    //sqlDA.SelectCommand.Parameters.Add(new MySqlParameter("ReturnValue",
                    //    MySqlDbType.Int32, 4, ParameterDirection.ReturnValue,
                    //    false, 0, 0, string.Empty, DataRowVersion.Default, null));
                    //result = sqlDA.SelectCommand.Parameters["ReturnValue"].Value==null;
                }

                returnvalue = result;
                if (trans == null)
                {
                    connection.Close();
                }

                return dataSet;
            }
            catch (Exception ex)
            {
                var logStr = new StringBuilder();
                logStr.AppendFormat("call procedule: procedure={1}\r\n msg={0}\r\n", ex.Message, storedProcName);
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        logStr.Append($"\t{p.ParameterName}={p.Value},\r\n");
                    }
                }
                logStr.AppendLine("stacks: " + ex.StackTrace);
                log.Error(logStr.ToString());

                throw;
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }

                if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                {
                    log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={storedProcName} \r\n {DumpParameter(parameters)}");
                }

                //var logStr = new StringBuilder();
                //logStr.AppendFormat("call {0}(", storedProcName);

                //foreach (var p in parameters)
                //{
                //    logStr.Append($"\t/*{p.ParameterName}=*/{p.Value},\r\n");
                //}
                //logStr.Append(")");
                //log.Info(logStr.ToString());
            }
        }

        private string DumpParameter(IEnumerable<IDataParameter> parameters)
        {
            if (parameters == null)
            {
                return string.Empty;
            }

            var logStr = new StringBuilder();
            foreach (var p in parameters)
            {
                logStr.Append($"\t{p.ParameterName}={p.Value},\r\n");
            }
            return logStr.ToString();
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, IDbTransaction trans, out int rowsAffected)
        {
            var ticks = System.Environment.TickCount;
            var connection = trans == null ? new MySqlConnection(ConnectionString) : trans.Connection;
            try
            {
                if (trans == null)
                {
                    connection.Open();
                }

                //int result;
                var command = BuildQueryCommand(connection, storedProcName, parameters);
                if (trans != null)
                {
                    command.Transaction = trans as MySqlTransaction;
                }

                // connection.InfoMessage += Connection_InfoMessage;
                rowsAffected = command.ExecuteNonQuery();

                // result = (int)(command.Parameters["ReturnValue"] as MySqlParameter).Value;
                if (trans == null)
                {
                    connection.Close();
                }

                return 0;
            }
            catch (Exception ex)
            {
                rowsAffected = 0;
                if (parameters == null)
                {
                    return 0;
                }

                var logStr = new StringBuilder();
                logStr.AppendFormat("call procedure: msg={0}, procedure={1}\r\n", ex.Message, storedProcName);
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        logStr.Append($"\t{p.ParameterName}={p.Value},\r\n");
                    }
                }
                logStr.AppendLine("stacks: " + ex.StackTrace);
                log.Error(logStr.ToString());

                throw;
            }
            finally
            {
                if (trans == null)
                {
                    connection.Dispose();
                }

                if (System.Environment.TickCount - ticks > SlowQueryThreshold)
                {
                    log.Log(SlowQueryLog, $"ticks={System.Environment.TickCount - ticks}, sql={storedProcName} \r\n {DumpParameter(parameters)}");
                }
            }
        }

        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private IDbCommand BuildIntCommand(IDbConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            var command = BuildQueryCommand(connection, storedProcName, parameters);

            command.Parameters.Add(new MySqlParameter("ReturnValue",
                MySqlDbType.Int32, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private IDbCommand BuildQueryCommand(IDbConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            var command = new MySqlCommand(storedProcName, connection as MySqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.CommandTimeout = CommandTimeout;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    if (parameter != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value.
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }

                        //command.Parameters.Add(parameter);
                        if (parameter.GetType() != typeof(MySqlParameter))
                        {
                            var mysqlPar = new MySqlParameter
                            {
                                ParameterName = parameter.ParameterName,
                                MySqlDbType = GetMySqlDbType(parameter.DbType),
                                Value = parameter.Value,

                                //mysqlPar.Size = parameter.Size;
                                Direction = parameter.Direction
                            };
                            command.Parameters.Add(mysqlPar);
                        }
                        else
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                }
            }

            return command;
        }

        #endregion 存储过程操作

        private MySqlDbType GetMySqlDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int16: return MySqlDbType.Int16;
                case DbType.Int32: return MySqlDbType.Int32;
                case DbType.Int64: return MySqlDbType.Int64;
                case DbType.String: return MySqlDbType.VarChar;
                case DbType.Date: return MySqlDbType.Date;
                case DbType.Boolean: return MySqlDbType.Bit;
                default: return MySqlDbType.VarString;
            }
        }
    }
}