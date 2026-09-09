using System;

namespace PHARMA.Common
{
    public static class Extensions
    {
        public static string SafeString(this object value)
        {
            return value == null || value == DBNull.Value ? string.Empty : value.ToString().Trim();
        }

        public static decimal SafeDecimal(this object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal result;
            return decimal.TryParse(value.ToString(), out result) ? result : 0m;
        }

        public static int SafeInt(this object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            int result;
            return int.TryParse(value.ToString(), out result) ? result : 0;
        }

        public static DateTime? SafeDate(this object value)
        {
            if (value == null || value == DBNull.Value) return null;
            DateTime result;
            return DateTime.TryParse(value.ToString(), out result) ? (DateTime?)result : null;
        }
    }
}