using System.Collections.Generic;
using System.Linq;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class AuthRepository : BaseRepository
    {
        public UserData ValidateUser(string username, string password)
        {
            // Try UserData first
            var sql = "SELECT * FROM UserData WHERE UserName = ? AND PassWord = ?";
            var user = QuerySingleOrDefault<UserData>(sql, new { username, password });
            if (user != null) return user;

            // Fallback to usertable
            var sql2 = "SELECT Username as UserName, Password as PassWord, Company, Grcd FROM usertable WHERE Username = ? AND Password = ?";
            return QuerySingleOrDefault<UserData>(sql2, new { username, password });
        }

        public IEnumerable<UserRights> GetUserRights(string username)
        {
            var sql = "SELECT * FROM UserRights WHERE UserName = ? AND (YNO = 'Y' OR YNO = '1')";
            return Query<UserRights>(sql, new { username });
        }

        public IEnumerable<MenuName> GetAllMenus()
        {
            var sql = "SELECT * FROM MenuName ORDER BY MenuTitle, MenuSubTitle, OptionTitle";
            return Query<MenuName>(sql);
        }
    }
}