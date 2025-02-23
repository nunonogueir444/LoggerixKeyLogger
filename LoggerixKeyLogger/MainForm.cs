using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class MainForm : Form
    {
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static MainForm? _instance;
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logger.txt");

        public MainForm()
        {
            InitializeComponent();
            _instance = this;
            _hookID = SetHook(_proc);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnFormClosing(e);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                if (curModule == null)
                {
                    throw new InvalidOperationException("Failed to get the current process module.");
                }
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string key = ((Keys)vkCode).ToString();

                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    switch (key)
                    {
                        case "Oem4":
                            key = "[";
                            break;
                        case "Oem6":
                            key = "]";
                            break;
                        case "Oem3":
                            key = "'";
                            break;
                        case "Oem7":
                            key = "#";
                            break;
                        case "OemSemicolon":
                            key = ";";
                            break;
                        case "Oem2":
                            key = "/";
                            break;
                        case "Oem8":
                            key = "`";
                            break;
                    }

                    _instance.Invoke(new Action(() => _instance.textBox1.AppendText(key + " key pressed" + Environment.NewLine)));
                    File.AppendAllText(logFilePath, key + " key pressed" + Environment.NewLine);

                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    switch (key)
                    {
                        case "Oem4":
                            key = "[";
                            break;
                        case "Oem6":
                            key = "]";
                            break;
                        case "Oem3":
                            key = "'";
                            break;
                        case "Oem7":
                            key = "#";
                            break;
                        case "OemSemicolon":
                            key = ";";
                            break;
                        case "Oem2":
                            key = "/";
                            break;
                        case "Oem8":
                            key = "`";
                            break;
                    }

                    if (vkCode == 0xA0 || vkCode == 0xA1)
                    {
                        _instance.Invoke(new Action(() => _instance.textBox1.AppendText(key + " key released" + Environment.NewLine)));
                        File.AppendAllText(logFilePath, key + " key released" + Environment.NewLine);
                    }
                }

            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;

    }
}