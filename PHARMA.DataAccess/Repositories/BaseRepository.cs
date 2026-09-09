using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace PHARMA.DataAccess.Repositories
{
    public abstract class BaseRepository
    {
        protected IDbConnection GetConnection()
        {
            return DbConnectionFactory.CreateConnection();
        }

        protected T QuerySingleOrDefault<T>(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.QuerySingleOrDefault<T>(sql, param);
            }
        }

        protected IEnumerable<T> Query<T>(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Query<T>(sql, param);
            }
        }

        protected int Execute(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Execute(sql, param);
            }
        }

        protected T ExecuteScalar<T>(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.ExecuteScalar<T>(sql, param);
            }
        }
    }
}