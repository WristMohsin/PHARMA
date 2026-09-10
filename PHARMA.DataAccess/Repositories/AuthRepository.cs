using System;
using System.Collections.Generic;
using System.Diagnostics;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class AuthRepository : BaseRepository
    {
        public UserData ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) return null;

            string userKey = username.Trim();

            UserData fromUserData = null;
            try
            {
                fromUserData = QuerySingleOrDefault<UserData>(
                    "SELECT * FROM UserData WHERE UserName = ? AND PassWord = ?",
                    userKey, password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: UserData login query failed: " + ex.Message);
                throw;
            }

            if (fromUserData != null)
            {
                NormalizeIdentity(fromUserData, userKey);
                return fromUserData;
            }

            UserData fromUserTable = null;
            try
            {
                fromUserTable = QuerySingleOrDefault<UserData>(
                    "SELECT Username as UserName, Password as PassWord, Company, Grcd FROM usertable WHERE Username = ? AND Password = ?",
                    userKey, password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: usertable login query failed: " + ex.Message);
                return null;
            }

            if (fromUserTable == null)
                return null;

            NormalizeIdentity(fromUserTable, userKey);

            try
            {
                var profile = QuerySingleOrDefault<UserData>(
                    "SELECT * FROM UserData WHERE UserName = ?",
                    fromUserTable.UserName);
                if (profile != null)
                {
                    if (!string.IsNullOrEmpty(profile.SecurityLevel))
                        fromUserTable.SecurityLevel = profile.SecurityLevel;
                    if (!string.IsNullOrEmpty(profile.DisplayName))
                        fromUserTable.DisplayName = profile.DisplayName;
                    if (!string.IsNullOrEmpty(profile.UserPrinter))
                        fromUserTable.UserPrinter = profile.UserPrinter;
                    if (string.IsNullOrEmpty(fromUserTable.UserName))
                        fromUserTable.UserName = profile.UserName;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: UserData enrich after usertable login failed: " + ex.Message);
            }

            return fromUserTable;
        }

        public UserData GetUserProfileByUserName(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            return QuerySingleOrDefault<UserData>(
                "SELECT * FROM UserData WHERE UserName = ?",
                username.Trim());
        }

        public List<UserRights> GetUserRights(string username)
        {
            if (string.IsNullOrEmpty(username)) return new List<UserRights>();
            return Query<UserRights>(
                "SELECT * FROM UserRights WHERE UserName = ? AND (YNO = 'Y' OR YNO = '1' OR YNO = 'Yes')",
                username.Trim());
        }

        public List<MenuName> GetAllMenus()
        {
            return Query<MenuName>(
                "SELECT * FROM MenuName ORDER BY ButtonName, MenuTitle, MenuSubTitle, OptionTitle");
        }

        private static void NormalizeIdentity(UserData user, string fallbackName)
        {
            if (user == null) return;
            if (string.IsNullOrEmpty(user.UserName))
                user.UserName = fallbackName;
            else
                user.UserName = user.UserName.Trim();

            if (user.SecurityLevel != null)
                user.SecurityLevel = user.SecurityLevel.Trim();

            if (user.SecurityLevel != null && user.SecurityLevel.Length == 0)
                user.SecurityLevel = null;
        }
    }
}
