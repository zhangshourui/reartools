using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Utility
{
    /// <summary>
    /// 数据访问抽象基础类
    /// Copyright (C) 2016 By Danny
    /// </summary>
    public class DbHelperSQL
    {
        private Log5 log = new Log5();

        //数据库连接字符串(web.config来配置)，可以动态更改ConnectionString支持多数据库.
        public string ConnectionString { get; private set; }

        // public static string conn = ConfigurationManager.ConnectionStrings["LCS_CoreRead"].ConnectionString.ToString();

        public int CommandTimeout = PubConstant.CommandTimeout;

        public DbHelperSQL(string connStr)
        {
            ConnectionString = connStr;
        }

        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            return connection;
        }

        #region 公用方法

        /// <summary>
        /// 判断是否存在某表的某个字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列名称</param>
        /// <returns>是否存在</returns>
        public bool ColumnExists(string tableName, string columnName)
        {
            var sql = string.Format("select count(1) from syscolumns where [id]=object_id('{0}') and [name]='{1}'", tableName, columnName);
            var res = GetSingle(sql);
            if (res == null)
            {
                return false;
            }
            return Convert.ToInt32(res) > 0;
        }

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
            var strsql = string.Format("select count(*) from sysobjects where id = object_id(N'[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1", TableName);
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

        public bool Exists(string strSql, params SqlParameter[] cmdParms)
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
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        public int ExecuteSqlByTime(string SQLString, int Times)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException e)
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
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand())
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

            dbparam.Value = value == null ? DBNull.Value : value;
            if (dbType == DbType.Time)
            {
                //   dbType = (DbType)(int)System.Data.SqlDbType.Time;
                var param = dbparam as SqlParameter;
                param.SqlDbType = SqlDbType.Time;
            }
            cmd.Parameters.Add(dbparam);
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, SqlTransaction trans = null)
        {
            var connection = trans == null ? new SqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        if (trans != null)
                        {
                            cmd.Transaction = trans;
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

        public object GetSingle(string SQLString, int Times, SqlTransaction trans = null)
        {
            var connection = trans == null ? new SqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        if (trans != null)
                        {
                            cmd.Transaction = trans;
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
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string strSQL)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand(strSQL, connection))
                {
                    try
                    {
                        connection.Open();
                        var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        return myReader;
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, SqlTransaction trans = null)
        {
            var connection = trans == null ? new SqlConnection(ConnectionString) : trans.Connection;
            try
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    using (var command = new SqlDataAdapter(SQLString, connection))
                    {
                        if (trans != null)
                        {
                            command.SelectCommand.Transaction = trans;
                        }

                        command.SelectCommand.CommandTimeout = CommandTimeout;
                        command.Fill(ds, "ds");
                    }
                }
                catch (SqlException ex)
                {
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
            }
        }

        public DataSet Query(string SQLString, int Timeout, SqlTransaction trans = null)
        {
            var connection = trans == null ? new SqlConnection(ConnectionString) : trans.Connection;
            try
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    using (var command = new SqlDataAdapter(SQLString, connection))
                    {
                        if (trans != null)
                        {
                            command.SelectCommand.Transaction = trans;
                        }

                        command.SelectCommand.CommandTimeout = Timeout;
                        command.Fill(ds, "ds");
                    }
                }
                catch (SqlException ex)
                {
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
            }
        }

        #endregion 执行简单SQL语句

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand())
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
                        throw e;
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
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand())
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
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand())
                    {
                        try
                        {
                            var count = 0;
                            //循环
                            foreach (var myDE in cmdList)
                            {
                                var cmdText = myDE.CommandText;
                                var cmdParms = (SqlParameter[])myDE.Parameters;
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
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public void ExecuteSqlTranWithIndentity(List<DBCommandInfo> SQLStringList)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand())
                    {
                        try
                        {
                            var indentity = 0;
                            //循环
                            foreach (var myDE in SQLStringList)
                            {
                                var cmdText = myDE.CommandText;
                                var cmdParms = (SqlParameter[])myDE.Parameters;
                                foreach (var q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.InputOutput)
                                    {
                                        q.Value = indentity;
                                    }
                                }

                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                var val = cmd.ExecuteNonQuery();
                                foreach (var q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.Output)
                                    {
                                        indentity = Convert.ToInt32(q.Value);
                                    }
                                }

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
        public void ExecuteSqlTranWithIndentity(Hashtable SQLStringList)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand())
                    {
                        try
                        {
                            var indentity = 0;
                            //循环
                            foreach (DictionaryEntry myDE in SQLStringList)
                            {
                                var cmdText = myDE.Key.ToString();
                                var cmdParms = (SqlParameter[])myDE.Value;
                                foreach (var q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.InputOutput)
                                    {
                                        q.Value = indentity;
                                    }
                                }

                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                var val = cmd.ExecuteNonQuery();
                                foreach (var q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.Output)
                                    {
                                        indentity = Convert.ToInt32(q.Value);
                                    }
                                }

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
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, SqlTransaction trans = null, params SqlParameter[] cmdParms)
        {
            var connection = trans == null ? new SqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new SqlCommand())
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
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        cmd.Parameters.Clear();
                        return myReader;
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    //			finally
                    //			{
                    //				cmd.Dispose();
                    //				connection.Close();
                    //			}
                }
            }
        }

        public DataSet Query(string SQLString, SqlTransaction trans = null, params SqlParameter[] cmdParms)
        {
            return Query(SQLString, CommandTimeout, trans, cmdParms);
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, int timeout, SqlTransaction trans = null, params SqlParameter[] cmdParms)
        {
            var connection = trans == null ? new SqlConnection(ConnectionString) : trans.Connection;
            try
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandTimeout = timeout;

                    PrepareCommand(cmd, connection, trans, SQLString, cmdParms);
                    using (var da = new SqlDataAdapter(cmd))
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
            }
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
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
                    if (parameter == null)
                    {
                        continue;
                    }

                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion 执行带参数的SQL语句

        #region 存储过程操作

        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, SqlConnection connection, SqlTransaction trans, out int returnvalue)
        {
            int result;
            var dataSet = new DataSet();
            using (var sqlDA = new SqlDataAdapter
            {
                SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
            })
            {
                if (trans != null)
                {
                    sqlDA.SelectCommand.Transaction = trans;
                }

                sqlDA.SelectCommand.Parameters.Add(new SqlParameter("ReturnValue",
                    SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                    false, 0, 0, string.Empty, DataRowVersion.Default, null));

                sqlDA.Fill(dataSet, tableName);

                result = (int)sqlDA.SelectCommand.Parameters["ReturnValue"].Value;
            }

            returnvalue = result;
            return dataSet;
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                //int result;
                //connection.Open();
                //SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                //connection.InfoMessage += Connection_InfoMessage;
                //rowsAffected = command.ExecuteNonQuery();
                //result = (int)command.Parameters["ReturnValue"].Value;
                ////Connection.Close();
                //return result;
                return RunProcedure(storedProcName, parameters, connection, null, out rowsAffected);
            }
        }
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, SqlTransaction trans, out int rowsAffected)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                //int result;
                //connection.Open();
                //SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                //connection.InfoMessage += Connection_InfoMessage;
                //rowsAffected = command.ExecuteNonQuery();
                //result = (int)command.Parameters["ReturnValue"].Value;
                ////Connection.Close();
                //return result;
                return RunProcedure(storedProcName, parameters, connection, trans, out rowsAffected);
            }
        }

        /// <summary>
        /// 执行存储过程，rowsAffected返回影响的行数，返回值返回存储过程的返回值
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns>存储过程的返回值</returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, SqlConnection connection, SqlTransaction trans, out int rowsAffected)
        {
            int result;

            var command = BuildIntCommand(connection, storedProcName, parameters);
            if (trans != null)
            {
                command.Transaction = trans;
            }

            connection.InfoMessage += Connection_InfoMessage;
            rowsAffected = command.ExecuteNonQuery();
            result = (int)command.Parameters["ReturnValue"].Value;

            return result;
        }

        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            var command = BuildQueryCommand(connection, storedProcName, parameters);

            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
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
        private SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            var command = new SqlCommand(storedProcName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value.
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }

            return command;
        }

        #endregion 存储过程操作

        private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            var msg = new System.Text.StringBuilder();
            msg.AppendLine(e.Message);
            msg.AppendLine("Detail:");
            if (e.Errors != null && e.Errors.Count > 0)
            {
                foreach (SqlError error in e.Errors)
                {
                    msg.Append($"{error.Message} AT {error.Procedure} {error.LineNumber}: {error.Source}\r\n");
                }
            }
            log.Info(msg.ToString());
        }
    }
}