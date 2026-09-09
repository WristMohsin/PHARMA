using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class AuthRepository : BaseRepository
    {
        public UserData ValidateUser(string username, string password)
        {
            // Try UserData first
            var user = QuerySingleOrDefault<UserData>(
                "SELECT * FROM UserData WHERE UserName = ? AND PassWord = ?",
                username, password);
            if (user != null) return user;

            // Fallback to usertable
            return QuerySingleOrDefault<UserData>(
                "SELECT Username as UserName, Password as PassWord, Company, Grcd FROM usertable WHERE Username = ? AND Password = ?",
                username, password);
        }

        public List<UserRights> GetUserRights(string username)
        {
            return Query<UserRights>(
                "SELECT * FROM UserRights WHERE UserName = ? AND (YNO = 'Y' OR YNO = '1')",
                username);
        }

        public List<MenuName> GetAllMenus()
        {
            return Query<MenuName>("SELECT * FROM MenuName ORDER BY MenuTitle, MenuSubTitle, OptionTitle");
        }
    }
}