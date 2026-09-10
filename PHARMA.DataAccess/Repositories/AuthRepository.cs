using System;
using System.Collections.Generic;
using System.Diagnostics;
using PHARMA.Common;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class AuthRepository : BaseRepository
    {
        public UserData ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) return null;
            if (password == null) return null;

            string userKey = username.Trim();
            if (userKey.Length == 0) return null;
            if (password.Length == 0) return null;

            UserData userDataRow = null;
            try
            {
                userDataRow = QuerySingleOrDefault<UserData>(
                    "SELECT * FROM UserData WHERE UserName = ?",
                    userKey);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: UserData lookup failed: " + ex.Message);
                throw;
            }

            if (userDataRow != null)
            {
                NormalizeIdentity(userDataRow, userKey);
                if (!VerifyAndMaybeUpgradeUserData(userDataRow, password))
                    return null;
                return userDataRow;
            }

            usertable ut = null;
            try
            {
                ut = QuerySingleOrDefault<usertable>(
                    "SELECT * FROM usertable WHERE Username = ?",
                    userKey);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: usertable lookup failed: " + ex.Message);
                throw;
            }

            if (ut == null || string.IsNullOrEmpty(ut.Username))
                return null;

            if (!VerifyAndMaybeUpgradeUserTable(ut, password))
                return null;

            var mapped = new UserData();
            mapped.UserName = ut.Username.Trim();
            mapped.Company = ut.Company;
            mapped.Grcd = ut.Grcd;
            NormalizeIdentity(mapped, userKey);

            try
            {
                var profile = QuerySingleOrDefault<UserData>(
                    "SELECT * FROM UserData WHERE UserName = ?",
                    mapped.UserName);
                if (profile != null)
                {
                    if (!string.IsNullOrEmpty(profile.SecurityLevel))
                        mapped.SecurityLevel = profile.SecurityLevel;
                    if (!string.IsNullOrEmpty(profile.DisplayName))
                        mapped.DisplayName = profile.DisplayName;
                    if (!string.IsNullOrEmpty(profile.UserPrinter))
                        mapped.UserPrinter = profile.UserPrinter;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: UserData enrich after usertable login failed: " + ex.Message);
            }

            return mapped;
        }

        private bool VerifyAndMaybeUpgradeUserData(UserData row, string password)
        {
            string hashCol = row.PasswordHash;
            string legacy = row.PassWord;

            if (!string.IsNullOrEmpty(hashCol) && PasswordHasher.IsHashedFormat(hashCol))
            {
                return PasswordHasher.VerifyPassword(password, hashCol);
            }

            if (!string.IsNullOrEmpty(legacy) && PasswordHasher.IsHashedFormat(legacy))
            {
                return PasswordHasher.VerifyPassword(password, legacy);
            }

            if (string.IsNullOrEmpty(legacy))
                return false;

            if (!string.Equals(legacy, password, StringComparison.Ordinal))
                return false;

            try
            {
                string newHash = PasswordHasher.CreateHash(password);
                Execute(
                    "UPDATE UserData SET PasswordHash = ?, PassWord = ? WHERE UserName = ?",
                    newHash, string.Empty, row.UserName);
                row.PasswordHash = newHash;
                row.PassWord = string.Empty;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: UserData password upgrade failed: " + ex.Message);
            }

            return true;
        }

        private bool VerifyAndMaybeUpgradeUserTable(usertable row, string password)
        {
            string hashCol = row.PasswordHash;
            string legacy = row.Password;

            if (!string.IsNullOrEmpty(hashCol) && PasswordHasher.IsHashedFormat(hashCol))
            {
                return PasswordHasher.VerifyPassword(password, hashCol);
            }

            if (!string.IsNullOrEmpty(legacy) && PasswordHasher.IsHashedFormat(legacy))
            {
                return PasswordHasher.VerifyPassword(password, legacy);
            }

            if (string.IsNullOrEmpty(legacy))
                return false;

            if (!string.Equals(legacy, password, StringComparison.Ordinal))
                return false;

            try
            {
                string newHash = PasswordHasher.CreateHash(password);
                Execute(
                    "UPDATE usertable SET PasswordHash = ?, Password = ? WHERE Username = ?",
                    newHash, string.Empty, row.Username);
                row.PasswordHash = newHash;
                row.Password = string.Empty;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthRepository: usertable password upgrade failed: " + ex.Message);
            }

            return true;
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
