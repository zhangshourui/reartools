using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Utility
{
    public class DALBase
    {
        /// <summary>
        /// 将Table数据转化为对象列表
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="table">数据表</param>
        /// <returns></returns>
        public static IList<T> MapRows<T>(DataTable table) where T : new()
        {
            IList<T> lstT = new List<T>();

            if (!table.HasData())
            {
                return lstT;
            }

            var type = typeof(T);
            var typeProperties = type.GetProperties();
            foreach (DataRow dataRow in table.Rows)
            {
                var t = new T();
                ///只有1列，并且数据类型是系统类型，则直接赋值给类型
                if ((typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime) || typeof(T).IsPrimitive) && table.Columns.Count == 1)
                {
                    var dbv = dataRow[0];
                    if (dbv != null)
                    {
                        if (dbv != DBNull.Value)
                        {
                            t = (T)Utility.CommonTools.ConvertObject(dbv, type);
                            lstT.Add(t);
                            continue;
                        }

                    }
                }

                lstT.Add(t);


                foreach (DataColumn dataColumn in table.Columns)
                {
                    var properties = typeProperties.Where(p => p.Name.Equals(dataColumn.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (!properties.Any()) //没找到与字段名相同的属性
                    {
                        continue;
                    }

                    var propertyInfo = properties.First();
                    var obj = (dataRow[dataColumn.ColumnName] == DBNull.Value) ? null : dataRow[dataColumn.ColumnName];
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                    {
                        if (dataColumn.DataType == typeof(DateTime) && propertyInfo.PropertyType == typeof(string))
                        {
                            obj = ((DateTime)dataRow[dataColumn.ColumnName]).ToString();
                        }
                        else if (dataColumn.DataType == typeof(string) && propertyInfo.PropertyType == typeof(DateTime))
                        {
                            DateTime temp;
                            if (DateTime.TryParse(obj.ToString(), out temp))
                            {
                                obj = temp;
                            }
                            else
                            {
                                throw new ArgumentException("不能将string转换为DateTime类型, 字段：" + dataColumn.ColumnName);
                            }
                        }
                        else
                        {
                            obj = Utility.CommonTools.ConvertObject(obj, propertyInfo.PropertyType);
                        }

                    }
                    //if(propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    //{

                    //}
                    //时间类型或者整形,则跳过赋值
                    if ((obj == null || string.IsNullOrEmpty(obj.ToString()))
                        && (propertyInfo.PropertyType == typeof(DateTime)
                            || propertyInfo.PropertyType == typeof(int)
                            || propertyInfo.PropertyType == typeof(short)
                            || propertyInfo.PropertyType == typeof(int)
                            || propertyInfo.PropertyType == typeof(long)
                           )
                        )
                    {
                        continue;
                    }
                    try
                    {
                        propertyInfo.SetValue(t, obj, null);
                    }
#if DEBUG
                    catch (Exception ex)
                    {
                        throw ex;
                    }
#else
					catch (Exception)
					{
						//throw ex;
					}
#endif
                }
            }

            return lstT;
        }

        /// <summary>
        /// DataTable转为List类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IList<T> ConvertDataTableList<T>(DataTable dataTable)
        {
            if (!dataTable.HasData())
            {
                return new List<T>();
            }

            IList<T> lstT = new List<T>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                if (dataRow == null)
                {
                    continue;
                }
                try
                {
                    var type = default(T);
                    var tbType = typeof(T);
                    type = (T)tbType.Assembly.CreateInstance(tbType.FullName);
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var column = dataTable.Columns[j];
                        if (string.IsNullOrWhiteSpace(column.ColumnName))
                        {
                            continue;
                        }
                        try
                        {
                            var pi = tbType.GetProperty(column.ColumnName);
                            if (pi == null || dataRow[j] == null || DBNull.Value == dataRow[j])
                            {
                                continue;
                            }
                            pi.SetValue(type, dataRow[j], null);
                        }
                        catch
                        {
                            continue;
                        }

                    }
                    lstT.Add(type);
                }
                catch
                {
                    continue;
                }
            }
            return lstT;
        }
        /// <summary>
        /// 作者：danny
        /// 时间：2015年10月22日11:16:38
        /// 功能：在DataSet 提取 DataTable
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static DataTable GetTable(DataSet ds)
        {
            var dt = new DataTable();
            if (!CheckDs(ds))
            {
                return dt;
            }

            if (ds.Tables.Count <= 0)
            {
                return dt;
            }

            if (ds.Tables[0] == null)
            {
                return dt;
            }

            if (ds.Tables[0].Rows.Count <= 0)
            {
                return dt;
            }

            if (ds.Tables[0].Rows[0] == null)
            {
                return dt;
            }

            return ds.Tables[0];
        }
        public static bool CheckDs(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
    public static class DataTableExtension
    {
        /// <summary>
        /// 是否含有某列名
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static bool HasData(this DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return false;
            }
            return true;
        }

    }
}
