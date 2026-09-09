namespace PHARMA.Common
{
    public static class Constants
    {
        public const string AppName = "PHARMA";
        public const string ConnectionStringName = "PHARMA";

        // Keyboard shortcut names (actual Keys used only in UI project)
        public const string HotkeyNew = "F2";
        public const string HotkeySave = "F5";
        public const string HotkeySearch = "F3";
        public const string HotkeyDelete = "F8";
        public const string HotkeyPrint = "F9";
        public const string HotkeyExit = "Escape";

        public static class Stock
        {
            public static bool AllowNegativeStock = false; // configurable
            public static bool WarnOnLowStock = true;
        }
    }
}