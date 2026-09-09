namespace PHARMA.Common
{
    public static class Constants
    {
        public const string AppName = "PHARMA";
        public const string ConnectionStringName = "PHARMA";

        // Keyboard shortcuts (common)
        public const Keys HotkeyNew = Keys.F2;
        public const Keys HotkeySave = Keys.F5;
        public const Keys HotkeySearch = Keys.F3;
        public const Keys HotkeyDelete = Keys.F8;
        public const Keys HotkeyPrint = Keys.F9;
        public const Keys HotkeyExit = Keys.Escape;

        public static class Stock
        {
            public static bool AllowNegativeStock = false; // configurable
            public static bool WarnOnLowStock = true;
        }
    }

    // Simple Keys enum helper if needed, but use System.Windows.Forms.Keys in UI
}