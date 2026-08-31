using Microsoft.Xna.Framework.Input;

namespace ImGuiNET;

internal partial class ImGuiRenderer
{
    /// <summary>
    /// Maps XNA Keys to ImGui key codes for input handling.
    /// </summary>
    private static readonly Dictionary<Keys, ImGuiKey> KeyMappings = new()
    {
        [Keys.Back] = ImGuiKey.Backspace,
        [Keys.Tab] = ImGuiKey.Tab,
        [Keys.Enter] = ImGuiKey.Enter,
        [Keys.CapsLock] = ImGuiKey.CapsLock,
        [Keys.Escape] = ImGuiKey.Escape,
        [Keys.Space] = ImGuiKey.Space,
        [Keys.PageUp] = ImGuiKey.PageUp,
        [Keys.PageDown] = ImGuiKey.PageDown,
        [Keys.End] = ImGuiKey.End,
        [Keys.Home] = ImGuiKey.Home,
        [Keys.Left] = ImGuiKey.LeftArrow,
        [Keys.Right] = ImGuiKey.RightArrow,
        [Keys.Up] = ImGuiKey.UpArrow,
        [Keys.Down] = ImGuiKey.DownArrow,
        [Keys.PrintScreen] = ImGuiKey.PrintScreen,
        [Keys.Insert] = ImGuiKey.Insert,
        [Keys.Delete] = ImGuiKey.Delete,
        [Keys.Multiply] = ImGuiKey.KeypadMultiply,
        [Keys.Add] = ImGuiKey.KeypadAdd,
        [Keys.Subtract] = ImGuiKey.KeypadSubtract,
        [Keys.Decimal] = ImGuiKey.KeypadDecimal,
        [Keys.Divide] = ImGuiKey.KeypadDivide,
        [Keys.NumLock] = ImGuiKey.NumLock,
        [Keys.Scroll] = ImGuiKey.ScrollLock,
        [Keys.LeftShift] = ImGuiKey.ModShift,
        [Keys.LeftControl] = ImGuiKey.ModCtrl,
        [Keys.LeftAlt] = ImGuiKey.ModAlt,
        [Keys.OemSemicolon] = ImGuiKey.Semicolon,
        [Keys.OemPlus] = ImGuiKey.Equal,
        [Keys.OemComma] = ImGuiKey.Comma,
        [Keys.OemMinus] = ImGuiKey.Minus,
        [Keys.OemPeriod] = ImGuiKey.Period,
        [Keys.OemQuestion] = ImGuiKey.Slash,
        [Keys.OemTilde] = ImGuiKey.GraveAccent,
        [Keys.OemOpenBrackets] = ImGuiKey.LeftBracket,
        [Keys.OemCloseBrackets] = ImGuiKey.RightBracket,
        [Keys.OemPipe] = ImGuiKey.Backslash,
        [Keys.OemQuotes] = ImGuiKey.Apostrophe,
        [Keys.BrowserBack] = ImGuiKey.AppBack,
        [Keys.BrowserForward] = ImGuiKey.AppForward
    };
    
    /// <summary>
    /// Dynamically populates key mappings for alphanumeric keys, numpad, and function keys.
    /// </summary>
    private static void PopulateKeyMappings()
    {
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            switch (key)
            {
                case >= Keys.D0 and <= Keys.D9:
                    KeyMappings.Add(key, ImGuiKey._0 + (key - Keys.D0));
                    break;
                    
                case >= Keys.A and <= Keys.Z:
                    KeyMappings.Add(key, ImGuiKey.A + (key - Keys.A));
                    break;
                    
                case >= Keys.NumPad0 and <= Keys.NumPad9:
                    KeyMappings.Add(key, ImGuiKey.Keypad0 + (key - Keys.NumPad0));
                    break;
                    
                case >= Keys.F1 and <= Keys.F24:
                    KeyMappings.Add(key, ImGuiKey.F1 + (key - Keys.F1));
                    break;
            }
        }
    }
}