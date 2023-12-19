using System.ComponentModel;

namespace WinFormsWrapper;

public partial class NotifyIcon : Component
{
    /// <summary>
    /// Initializes a new instance of the NotifyIcon class.
    /// </summary>
    /// <param name="iconStream">The stream containing the icon data.</param>
    /// <param name="tooltipText">The text for the tooltip of the NotifyIcon.</param>
    public NotifyIcon(System.IO.Stream iconStream, string tooltipText)
    {
        InitializeComponent();
        
        notifyIcon1.Icon = new Icon(iconStream);
        notifyIcon1.Text = tooltipText;
    }


    /// <summary>
    /// Initializes a new instance of the NotifyIcon class with a specified container.
    /// </summary>
    /// <param name="container">The container where the NotifyIcon will be added.</param>
    public NotifyIcon(System.IO.Stream iconStream, string tooltipText, IContainer container)
    {
        container.Add(this);

        InitializeComponent();

        notifyIcon1.Icon = new Icon(iconStream);
        notifyIcon1.Text = tooltipText;
    }

    /// <summary>
    /// Adds a menu item to the NotifyIcon with specified text, image, and dropdown items.
    /// </summary>
    /// <param name="text">The text of the menu item.</param>
    /// <param name="imageStream">The stream containing the image data for the menu item.</param>
    /// <param name="dropDownItems">The dropdown items to include in the menu item.</param>
    public void AddMenuItem(string text, System.IO.Stream imageStream,params NotifyIconMenuItem[] dropDownItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds a preconfigured menu item to the NotifyIcon.
    /// </summary>
    /// <param name="menu">The NotifyIconMenuItem to be added.</param>
    public void AddMenuItem(NotifyIconMenuItem menu)
    {
        var toolStripItem = new ToolStripMenuItem();
        toolStripItem.Text = menu.Text;
        toolStripItem.Click += menu.EventHandler;
        if (menu.Width >= 0) {
            toolStripItem.Width = menu.Width;
        }
        if (menu.Height >= 0)
        {
            toolStripItem.Height = menu.Height;
        }
        toolStripItem.ShortcutKeys = (System.Windows.Forms.Keys)Enum.ToObject(typeof(System.Windows.Forms.Keys), menu.ShortcutKeys);

        if (menu.ImageStream != null) {
            toolStripItem.Image = System.Drawing.Image.FromStream(menu.ImageStream);
        }

        contextMenuStrip1.Items.Add(toolStripItem);
    }

    private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {

    }

    /// <summary>
    /// Disposes of the resources (other than memory) used by the NotifyIcon.
    /// </summary>
    public new void Dispose()
    {
        this.notifyIcon1.Dispose();
        base.Dispose();
    }
}

public struct NotifyIconMenuItem
{
    /// <summary>
    /// The text of the menu item.
    /// </summary>
    public readonly string Text;

    /// <summary>
    /// The width of the menu item.
    /// </summary>
    public readonly int Width;

    /// <summary>
    /// The height of the menu item.
    /// </summary>
    public readonly int Height;

    /// <summary>
    /// The event handler for the menu item's action.
    /// </summary>
    public readonly EventHandler EventHandler;

    /// <summary>
    /// The shortcut keys associated with the menu item.
    /// </summary>
    public readonly Keys ShortcutKeys;

    /// <summary>
    /// The stream containing the image data for the menu item.
    /// </summary>
    public readonly System.IO.Stream ImageStream;

    /// <summary>
    /// Initializes a new instance of the NotifyIconMenuItem structure.
    /// </summary>
    /// <param name="text">The text of the menu item.</param>
    /// <param name="eventHandler">The event handler for the menu item's action.</param>
    /// <param name="width">The width of the menu item.</param>
    /// <param name="height">The height of the menu item.</param>
    /// <param name="imageStream">The stream containing the image data for the menu item.</param>
    /// <param name="shortcutKeys">The shortcut keys associated with the menu item.</param>
    public NotifyIconMenuItem(string text, EventHandler eventHandler, int width = -1, int height = -1, System.IO.Stream imageStream = null, Keys shortcutKeys = Keys.None)
    {
        Text = text;
        EventHandler = eventHandler;
        Width = width;
        Height = height;
        ImageStream = imageStream;
        ShortcutKeys = shortcutKeys;
    }
}

[Flags]
[TypeConverter(typeof(KeysConverter))]
public enum Keys
{
    //
    // 概要:
    //     キー値からキー コードを抽出するビット マスク。
    KeyCode = System.Windows.Forms.Keys.KeyCode,
    //
    // 概要:
    //     キー値から修飾子を抽出するビット マスク。
    Modifiers = System.Windows.Forms.Keys.Modifiers,
    //
    // 概要:
    //     押されたキーがありません。
    None = System.Windows.Forms.Keys.None,
    //
    // 概要:
    //     マウスの左ボタン
    LButton = System.Windows.Forms.Keys.LButton,
    //
    // 概要:
    //     マウスの右ボタン
    RButton = System.Windows.Forms.Keys.RButton,
    //
    // 概要:
    //     Cancel キー
    Cancel = System.Windows.Forms.Keys.Cancel,
    //
    // 概要:
    //     マウスの中央ボタン (3 ボタン マウスの場合)
    MButton = System.Windows.Forms.Keys.MButton,
    //
    // 概要:
    //     x マウスの 1 番目のボタン (5 ボタン マウスの場合)
    XButton1 = System.Windows.Forms.Keys.XButton1,
    //
    // 概要:
    //     x マウスの 2 番目のボタン (5 ボタン マウスの場合)
    XButton2 = System.Windows.Forms.Keys.XButton2,
    //
    // 概要:
    //     BackSpace キー。
    Back = System.Windows.Forms.Keys.Back,
    //
    // 概要:
    //     Tab キー。
    Tab = System.Windows.Forms.Keys.Tab,
    //
    // 概要:
    //     ライン フィード キー
    LineFeed = System.Windows.Forms.Keys.LineFeed,
    //
    // 概要:
    //     Clear キー。
    Clear = System.Windows.Forms.Keys.Clear,
    //
    // 概要:
    //     Return キー
    Return = System.Windows.Forms.Keys.Return,
    //
    // 概要:
    //     Enter キー。
    Enter = System.Windows.Forms.Keys.Enter,
    //
    // 概要:
    //     Shift キー
    ShiftKey = System.Windows.Forms.Keys.ShiftKey,
    //
    // 概要:
    //     CTRL キー
    ControlKey = System.Windows.Forms.Keys.ControlKey,
    //
    // 概要:
    //     Alt キー。
    Menu = System.Windows.Forms.Keys.Menu,
    //
    // 概要:
    //     Pause キー。
    Pause = System.Windows.Forms.Keys.Pause,
    //
    // 概要:
    //     CAPS LOCK キー
    Capital = System.Windows.Forms.Keys.Capital,
    //
    // 概要:
    //     CAPS LOCK キー
    CapsLock = System.Windows.Forms.Keys.CapsLock,
    //
    // 概要:
    //     IME かなモード キー。
    KanaMode = System.Windows.Forms.Keys.KanaMode,
    //
    // 概要:
    //     IME ハングル モード キー (互換性を保つために保持されています。HangulMode を使用します)
    HanguelMode = System.Windows.Forms.Keys.HanguelMode,
    //
    // 概要:
    //     IME ハングル モード キー。
    HangulMode = System.Windows.Forms.Keys.HangulMode,
    //
    // 概要:
    //     IME Junja モード キー。
    JunjaMode = System.Windows.Forms.Keys.JunjaMode,
    //
    // 概要:
    //     IME Final モード キー
    FinalMode = System.Windows.Forms.Keys.FinalMode,
    //
    // 概要:
    //     IME Hanja モード キー。
    HanjaMode = System.Windows.Forms.Keys.HanjaMode,
    //
    // 概要:
    //     IME 漢字モード キー。
    KanjiMode = System.Windows.Forms.Keys.KanjiMode,
    //
    // 概要:
    //     Esc キー。
    Escape = System.Windows.Forms.Keys.Escape,
    //
    // 概要:
    //     IME 変換キー
    IMEConvert = System.Windows.Forms.Keys.IMEConvert,
    //
    // 概要:
    //     IME 無変換キー
    IMENonconvert = System.Windows.Forms.Keys.IMENonconvert,
    //
    // 概要:
    //     IME Accept キー (System.Windows.Forms.Keys.IMEAceept の代わりに使用します)
    IMEAccept = System.Windows.Forms.Keys.IMEAccept,
    //
    // 概要:
    //     IME Accept キー 互換性を維持するために残されています。代わりに System.Windows.Forms.Keys.IMEAccept を使用してください。
    IMEAceept = System.Windows.Forms.Keys.IMEAceept,
    //
    // 概要:
    //     IME モード変更キー
    IMEModeChange = System.Windows.Forms.Keys.IMEModeChange,
    //
    // 概要:
    //     Space キー。
    Space = System.Windows.Forms.Keys.Space,
    //
    // 概要:
    //     Page Up キー。
    Prior = System.Windows.Forms.Keys.Prior,
    //
    // 概要:
    //     Page Up キー。
    PageUp = System.Windows.Forms.Keys.PageUp,
    //
    // 概要:
    //     Page Down キー。
    Next = System.Windows.Forms.Keys.Next,
    //
    // 概要:
    //     Page Down キー。
    PageDown = System.Windows.Forms.Keys.PageDown,
    //
    // 概要:
    //     End キー。
    End = System.Windows.Forms.Keys.End,
    //
    // 概要:
    //     Home キー。
    Home = System.Windows.Forms.Keys.Home,
    //
    // 概要:
    //     ←キー。
    Left = System.Windows.Forms.Keys.Left,
    //
    // 概要:
    //     ↑キー。
    Up = System.Windows.Forms.Keys.Up,
    //
    // 概要:
    //     →キー。
    Right = System.Windows.Forms.Keys.Right,
    //
    // 概要:
    //     ↓キー。
    Down = System.Windows.Forms.Keys.Down,
    //
    // 概要:
    //     Select キー。
    Select = System.Windows.Forms.Keys.Select,
    //
    // 概要:
    //     Print キー。
    Print = System.Windows.Forms.Keys.Print,
    //
    // 概要:
    //     Execute キー。
    Execute = System.Windows.Forms.Keys.Execute,
    //
    // 概要:
    //     Print Screen キー。
    Snapshot = System.Windows.Forms.Keys.Snapshot,
    //
    // 概要:
    //     Print Screen キー。
    PrintScreen = System.Windows.Forms.Keys.PrintScreen,
    //
    // 概要:
    //     INS キー
    Insert = System.Windows.Forms.Keys.Insert,
    //
    // 概要:
    //     DEL キー
    Delete = System.Windows.Forms.Keys.Delete,
    //
    // 概要:
    //     Help キー。
    Help = System.Windows.Forms.Keys.Help,
    //
    // 概要:
    //     0 キー。
    D0 = System.Windows.Forms.Keys.D0,
    //
    // 概要:
    //     1 キー。
    D1 = System.Windows.Forms.Keys.D1,
    //
    // 概要:
    //     2 キー。
    D2 = System.Windows.Forms.Keys.D2,
    //
    // 概要:
    //     3 キー。
    D3 = System.Windows.Forms.Keys.D3,
    //
    // 概要:
    //     4 キー。
    D4 = System.Windows.Forms.Keys.D4,
    //
    // 概要:
    //     5 キー。
    D5 = System.Windows.Forms.Keys.D5,
    //
    // 概要:
    //     6 キー。
    D6 = System.Windows.Forms.Keys.D6,
    //
    // 概要:
    //     7 キー。
    D7 = System.Windows.Forms.Keys.D7,
    //
    // 概要:
    //     8 キー。
    D8 = System.Windows.Forms.Keys.D8,
    //
    // 概要:
    //     9 キー。
    D9 = System.Windows.Forms.Keys.D9,
    //
    // 概要:
    //     A キー。
    A = System.Windows.Forms.Keys.A,
    //
    // 概要:
    //     B キー。
    B = System.Windows.Forms.Keys.B,
    //
    // 概要:
    //     C キー。
    C = System.Windows.Forms.Keys.C,
    //
    // 概要:
    //     D キー。
    D = System.Windows.Forms.Keys.D,
    //
    // 概要:
    //     E キー。
    E = System.Windows.Forms.Keys.E,
    //
    // 概要:
    //     F キー。
    F = System.Windows.Forms.Keys.F,
    //
    // 概要:
    //     G キー。
    G = System.Windows.Forms.Keys.G,
    //
    // 概要:
    //     H キー。
    H = System.Windows.Forms.Keys.H,
    //
    // 概要:
    //     I キー。
    I = System.Windows.Forms.Keys.I,
    //
    // 概要:
    //     J キー。
    J = System.Windows.Forms.Keys.J,
    //
    // 概要:
    //     K キー。
    K = System.Windows.Forms.Keys.K,
    //
    // 概要:
    //     L キー。
    L = System.Windows.Forms.Keys.L,
    //
    // 概要:
    //     M キー。
    M = System.Windows.Forms.Keys.M,
    //
    // 概要:
    //     N キー。
    N = System.Windows.Forms.Keys.N,
    //
    // 概要:
    //     O キー。
    O = System.Windows.Forms.Keys.O,
    //
    // 概要:
    //     P キー。
    P = System.Windows.Forms.Keys.P,
    //
    // 概要:
    //     Q キー。
    Q = System.Windows.Forms.Keys.Q,
    //
    // 概要:
    //     R キー。
    R = System.Windows.Forms.Keys.R,
    //
    // 概要:
    //     S キー。
    S = System.Windows.Forms.Keys.S,
    //
    // 概要:
    //     T キー。
    T = System.Windows.Forms.Keys.T,
    //
    // 概要:
    //     U キー。
    U = System.Windows.Forms.Keys.U,
    //
    // 概要:
    //     V キー。
    V = System.Windows.Forms.Keys.V,
    //
    // 概要:
    //     W キー。
    W = System.Windows.Forms.Keys.W,
    //
    // 概要:
    //     X キー。
    X = System.Windows.Forms.Keys.X,
    //
    // 概要:
    //     Y キー。
    Y = System.Windows.Forms.Keys.Y,
    //
    // 概要:
    //     Z キー。
    Z = System.Windows.Forms.Keys.Z,
    //
    // 概要:
    //     左 Windows ロゴ キー (Microsoft Natural Keyboard)。
    LWin = System.Windows.Forms.Keys.LWin,
    //
    // 概要:
    //     右 Windows ロゴ キー (Microsoft Natural Keyboard)。
    RWin = System.Windows.Forms.Keys.RWin,
    //
    // 概要:
    //     アプリケーション キー (Microsoft Natural Keyboard)
    Apps = System.Windows.Forms.Keys.Apps,
    //
    // 概要:
    //     コンピューターのスリープ キー
    Sleep = System.Windows.Forms.Keys.Sleep,
    //
    // 概要:
    //     0 キー (テンキー)。
    NumPad0 = System.Windows.Forms.Keys.NumPad0,
    //
    // 概要:
    //     1 キー (テンキー)。
    NumPad1 = System.Windows.Forms.Keys.NumPad1,
    //
    // 概要:
    //     2 キー (テンキー)。
    NumPad2 = System.Windows.Forms.Keys.NumPad2,
    //
    // 概要:
    //     3 キー (テンキー)。
    NumPad3 = System.Windows.Forms.Keys.NumPad3,
    //
    // 概要:
    //     4 キー (テンキー)。
    NumPad4 = System.Windows.Forms.Keys.NumPad4,
    //
    // 概要:
    //     5 キー (テンキー)。
    NumPad5 = System.Windows.Forms.Keys.NumPad5,
    //
    // 概要:
    //     6 キー (テンキー)。
    NumPad6 = System.Windows.Forms.Keys.NumPad6,
    //
    // 概要:
    //     7 キー (テンキー)。
    NumPad7 = System.Windows.Forms.Keys.NumPad7,
    //
    // 概要:
    //     8 キー (テンキー)。
    NumPad8 = System.Windows.Forms.Keys.NumPad8,
    //
    // 概要:
    //     9 キー (テンキー)。
    NumPad9 = System.Windows.Forms.Keys.NumPad9,
    //
    // 概要:
    //     乗算記号 (*) キー
    Multiply = System.Windows.Forms.Keys.Multiply,
    //
    // 概要:
    //     Add キー
    Add = System.Windows.Forms.Keys.Add,
    //
    // 概要:
    //     区切り記号キー
    Separator = System.Windows.Forms.Keys.Separator,
    //
    // 概要:
    //     減算記号 (-) キー
    Subtract = System.Windows.Forms.Keys.Subtract,
    //
    // 概要:
    //     小数点キー
    Decimal = System.Windows.Forms.Keys.Decimal,
    //
    // 概要:
    //     除算記号 (/) キー
    Divide = System.Windows.Forms.Keys.Divide,
    //
    // 概要:
    //     F1 キー。
    F1 = System.Windows.Forms.Keys.F1,
    //
    // 概要:
    //     F2 キー。
    F2 = System.Windows.Forms.Keys.F2,
    //
    // 概要:
    //     F3 キー。
    F3 = System.Windows.Forms.Keys.F3,
    //
    // 概要:
    //     F4 キー。
    F4 = System.Windows.Forms.Keys.F4,
    //
    // 概要:
    //     F5 キー。
    F5 = System.Windows.Forms.Keys.F5,
    //
    // 概要:
    //     F6 キー。
    F6 = System.Windows.Forms.Keys.F6,
    //
    // 概要:
    //     F7 キー。
    F7 = System.Windows.Forms.Keys.F7,
    //
    // 概要:
    //     F8 キー。
    F8 = System.Windows.Forms.Keys.F8,
    //
    // 概要:
    //     F9 キー。
    F9 = System.Windows.Forms.Keys.F9,
    //
    // 概要:
    //     F10 キー。
    F10 = System.Windows.Forms.Keys.F10,
    //
    // 概要:
    //     F11 キー。
    F11 = System.Windows.Forms.Keys.F11,
    //
    // 概要:
    //     F12 キー。
    F12 = System.Windows.Forms.Keys.F12,
    //
    // 概要:
    //     F13 キー。
    F13 = System.Windows.Forms.Keys.F13,
    //
    // 概要:
    //     F14 キー。
    F14 = System.Windows.Forms.Keys.F14,
    //
    // 概要:
    //     F15 キー。
    F15 = System.Windows.Forms.Keys.F15,
    //
    // 概要:
    //     F16 キー。
    F16 = System.Windows.Forms.Keys.F16,
    //
    // 概要:
    //     F17 キー。
    F17 = System.Windows.Forms.Keys.F17,
    //
    // 概要:
    //     F18 キー。
    F18 = System.Windows.Forms.Keys.F18,
    //
    // 概要:
    //     F19 キー。
    F19 = System.Windows.Forms.Keys.F19,
    //
    // 概要:
    //     F20 キー。
    F20 = System.Windows.Forms.Keys.F20,
    //
    // 概要:
    //     F21 キー。
    F21 = System.Windows.Forms.Keys.F21,
    //
    // 概要:
    //     F22 キー。
    F22 = System.Windows.Forms.Keys.F22,
    //
    // 概要:
    //     F23 キー。
    F23 = System.Windows.Forms.Keys.F23,
    //
    // 概要:
    //     F24 キー。
    F24 = System.Windows.Forms.Keys.F24,
    //
    // 概要:
    //     NUM LOCK キー
    NumLock = System.Windows.Forms.Keys.NumLock,
    //
    // 概要:
    //     ScrollLock キー
    Scroll = System.Windows.Forms.Keys.Scroll,
    //
    // 概要:
    //     左の Shift キー
    LShiftKey = System.Windows.Forms.Keys.LShiftKey,
    //
    // 概要:
    //     右の Shift キー
    RShiftKey = System.Windows.Forms.Keys.RShiftKey,
    //
    // 概要:
    //     左 Ctrl キー。
    LControlKey = System.Windows.Forms.Keys.LControlKey,
    //
    // 概要:
    //     右 Ctrl キー。
    RControlKey = System.Windows.Forms.Keys.RControlKey,
    //
    // 概要:
    //     左 Alt キー。
    LMenu = System.Windows.Forms.Keys.LMenu,
    //
    // 概要:
    //     右 Alt キー。
    RMenu = System.Windows.Forms.Keys.RMenu,
    //
    // 概要:
    //     戻るキー (Windows 2000 以降)
    BrowserBack = System.Windows.Forms.Keys.BrowserBack,
    //
    // 概要:
    //     進むキー (Windows 2000 以降)
    BrowserForward = System.Windows.Forms.Keys.BrowserForward,
    //
    // 概要:
    //     更新キー (Windows 2000 以降)
    BrowserRefresh = System.Windows.Forms.Keys.BrowserRefresh,
    //
    // 概要:
    //     中止キー (Windows 2000 以降)
    BrowserStop = System.Windows.Forms.Keys.BrowserStop,
    //
    // 概要:
    //     検索キー (Windows 2000 以降)
    BrowserSearch = System.Windows.Forms.Keys.BrowserSearch,
    //
    // 概要:
    //     お気に入りキー (Windows 2000 以降)
    BrowserFavorites = System.Windows.Forms.Keys.BrowserFavorites,
    //
    // 概要:
    //     ホーム キー (Windows 2000 以降)
    BrowserHome = System.Windows.Forms.Keys.BrowserHome,
    //
    // 概要:
    //     ミュート キー (Windows 2000 以降)
    VolumeMute = System.Windows.Forms.Keys.VolumeMute,
    //
    // 概要:
    //     音量 - キー (Windows 2000 以降)
    VolumeDown = System.Windows.Forms.Keys.VolumeDown,
    //
    // 概要:
    //     音量 + キー (Windows 2000 以降)
    VolumeUp = System.Windows.Forms.Keys.VolumeUp,
    //
    // 概要:
    //     次のトラック キー (Windows 2000 以降)
    MediaNextTrack = System.Windows.Forms.Keys.MediaNextTrack,
    //
    // 概要:
    //     前のトラック キー (Windows 2000 以降)
    MediaPreviousTrack = System.Windows.Forms.Keys.MediaPreviousTrack,
    //
    // 概要:
    //     停止キー (Windows 2000 以降)
    MediaStop = System.Windows.Forms.Keys.MediaStop,
    //
    // 概要:
    //     再生/一時停止キー (Windows 2000 以降)
    MediaPlayPause = System.Windows.Forms.Keys.MediaPlayPause,
    //
    // 概要:
    //     メール ホット キー (Windows 2000 以降)
    LaunchMail = System.Windows.Forms.Keys.LaunchMail,
    //
    // 概要:
    //     メディア キー (Windows 2000 以降)
    SelectMedia = System.Windows.Forms.Keys.SelectMedia,
    //
    // 概要:
    //     カスタム ホット キー 1 (Windows 2000 以降)
    LaunchApplication1 = System.Windows.Forms.Keys.LaunchApplication1,
    //
    // 概要:
    //     カスタム ホット キー 2 (Windows 2000 以降)
    LaunchApplication2 = System.Windows.Forms.Keys.LaunchApplication2,
    //
    // 概要:
    //     米国標準キーボード上の OEM セミコロン キー (Windows 2000 以降)
    OemSemicolon = System.Windows.Forms.Keys.OemSemicolon,
    //
    // 概要:
    //     OEM 1 キー。
    Oem1 = System.Windows.Forms.Keys.Oem1,
    //
    // 概要:
    //     国または地域別キーボード上の OEM プラス キー (Windows 2000 以降)
    Oemplus = System.Windows.Forms.Keys.Oemplus,
    //
    // 概要:
    //     国または地域別キーボード上の OEM コンマ キー (Windows 2000 以降)
    Oemcomma = System.Windows.Forms.Keys.Oemcomma,
    //
    // 概要:
    //     国または地域別キーボード上の OEM マイナス キー (Windows 2000 以降)
    OemMinus = System.Windows.Forms.Keys.OemMinus,
    //
    // 概要:
    //     国または地域別キーボード上の OEM ピリオド キー (Windows 2000 以降)
    OemPeriod = System.Windows.Forms.Keys.OemPeriod,
    //
    // 概要:
    //     米国標準キーボード上の OEM 疑問符キー (Windows 2000 以降)
    OemQuestion = System.Windows.Forms.Keys.OemQuestion,
    //
    // 概要:
    //     OEM 2 キー。
    Oem2 = System.Windows.Forms.Keys.Oem2,
    //
    // 概要:
    //     米国標準キーボード上の OEM ティルダ キー (Windows 2000 以降)
    Oemtilde = System.Windows.Forms.Keys.Oemtilde,
    //
    // 概要:
    //     OEM 3 キー。
    Oem3 = System.Windows.Forms.Keys.Oem3,
    //
    // 概要:
    //     米国標準キーボード上の OEM 左角かっこキー (Windows 2000 以降)
    OemOpenBrackets = System.Windows.Forms.Keys.OemOpenBrackets,
    //
    // 概要:
    //     OEM 4 キー。
    Oem4 = System.Windows.Forms.Keys.Oem4,
    //
    // 概要:
    //     米国標準キーボード上の OEM Pipe キー (Windows 2000 以降)
    OemPipe = System.Windows.Forms.Keys.OemPipe,
    //
    // 概要:
    //     OEM 5 キー。
    Oem5 = System.Windows.Forms.Keys.Oem5,
    //
    // 概要:
    //     米国標準キーボード上の OEM 右角かっこキー (Windows 2000 以降)
    OemCloseBrackets = System.Windows.Forms.Keys.OemCloseBrackets,
    //
    // 概要:
    //     OEM 6 キー。
    Oem6 = System.Windows.Forms.Keys.Oem6,
    //
    // 概要:
    //     米国標準キーボード上の OEM 一重/二重引用符キー (Windows 2000 以降)
    OemQuotes = System.Windows.Forms.Keys.OemQuotes,
    //
    // 概要:
    //     OEM 7 キー。
    Oem7 = System.Windows.Forms.Keys.Oem7,
    //
    // 概要:
    //     OEM 8 キー。
    Oem8 = System.Windows.Forms.Keys.Oem8,
    //
    // 概要:
    //     RT 102 キーのキーボード上の OEM 山かっこキーまたは円記号キー (Windows 2000 以降)
    OemBackslash = System.Windows.Forms.Keys.OemBackslash,
    //
    // 概要:
    //     OEM 102 キー。
    Oem102 = System.Windows.Forms.Keys.Oem102,
    //
    // 概要:
    //     ProcessKey キー
    ProcessKey = System.Windows.Forms.Keys.ProcessKey,
    //
    // 概要:
    //     Unicode 文字がキーストロークであるかのように渡されます。 Packet のキー値は、キーボード以外の入力手段に使用される 32 ビット仮想キー値の下位ワードです。
    Packet = System.Windows.Forms.Keys.Packet,
    //
    // 概要:
    //     Attn キー。
    Attn = System.Windows.Forms.Keys.Attn,
    //
    // 概要:
    //     Crsel キー。
    Crsel = System.Windows.Forms.Keys.Crsel,
    //
    // 概要:
    //     Exsel キー。
    Exsel = System.Windows.Forms.Keys.Exsel,
    //
    // 概要:
    //     Erase Eof キー。
    EraseEof = System.Windows.Forms.Keys.EraseEof,
    //
    // 概要:
    //     Play キー。
    Play = System.Windows.Forms.Keys.Play,
    //
    // 概要:
    //     Zoom キー。
    Zoom = System.Windows.Forms.Keys.Zoom,
    //
    // 概要:
    //     将来使用するために予約されている定数。
    NoName = System.Windows.Forms.Keys.NoName,
    //
    // 概要:
    //     PA1 キー。
    Pa1 = System.Windows.Forms.Keys.Pa1,
    //
    // 概要:
    //     Clear キー。
    OemClear = System.Windows.Forms.Keys.OemClear,
    //
    // 概要:
    //     Shift 修飾子キー
    Shift = System.Windows.Forms.Keys.Shift,
    //
    // 概要:
    //     Ctrl 修飾子キー
    Control = System.Windows.Forms.Keys.Control,
    //
    // 概要:
    //     Alt 修飾子キー
    Alt = System.Windows.Forms.Keys.Alt
}
