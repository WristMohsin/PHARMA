using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Reflection;

namespace PHARMA.DataAccess.Repositories
{
    public abstract class BaseRepository
    {
        protected IDbConnection GetConnection()
        {
            return DbConnectionFactory.CreateConnection();
        }

        protected T Map<T>(IDataReader reader) where T : new()
        {
            var obj = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var prop = Array.Find(props, p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.CanWrite && !reader.IsDBNull(i))
                {
                    var val = reader.GetValue(i);
                    try
                    {
                        if (prop.PropertyType == typeof(string))
                            prop.SetValue(obj, val.ToString(), null);
                        else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                            prop.SetValue(obj, Convert.ToInt32(val), null);
                        else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                            prop.SetValue(obj, Convert.ToDecimal(val), null);
                        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                            prop.SetValue(obj, Convert.ToDateTime(val), null);
                        else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                            prop.SetValue(obj, Convert.ToBoolean(val), null);
                        else
                            prop.SetValue(obj, val, null);
                    }
                    catch { /* skip bad conversion */ }
                }
            }
            return obj;
        }

        protected T QuerySingleOrDefault<T>(string sql, params object[] parameters) where T : class, new()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParams(cmd, parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return Map<T>(reader);
                    return null;
                }
            }
        }

        protected List<T> Query<T>(string sql, params object[] parameters) where T : new()
        {
            var list = new List<T>();
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParams(cmd, parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(Map<T>(reader));
                }
            }
            return list;
        }

        protected int Execute(string sql, params object[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParams(cmd, parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        protected T ExecuteScalar<T>(string sql, params object[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParams(cmd, parameters);
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return default(T);
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        private void AddParams(IDbCommand cmd, object[] parameters)
        {
            if (parameters == null) return;
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@p" + i;
                p.Value = parameters[i] ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }
        }
    }
}