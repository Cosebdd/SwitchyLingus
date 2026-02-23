using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace SwitchyLingus.Core.Unsafe;

[SupportedOSPlatform("windows")]
internal sealed class InputMethod
{
    private const string KeyboardLayoutsRegistryPath = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";

    private readonly nint _handle;

    private InputMethod(nint handle)
    {
        _handle = handle;
    }

    public static unsafe InputMethod[] GetInstalledInputMethods()
    {
        var size = PInvoke.GetKeyboardLayoutList(0, null);

        var handles = new nint[size];

        fixed (nint* h = handles)
        {
            var written = PInvoke.GetKeyboardLayoutList(size, h);
            Debug.Assert(written == size, $"GetKeyboardLayoutList returned {written} handles, expected {size}");
        }

        var inputLangs = new InputMethod[size];
        for (var i = 0; i < size; i++)
        {
            inputLangs[i] = new InputMethod(handles[i]);
        }

        return inputLangs;
    }

    internal string? GetInputMethodTip()
    {
        var layoutId = GetLayoutId();
        if (layoutId is null)
            return null;
        return $"{layoutId[^4..]}:{layoutId.ToLowerInvariant()}";
    }

    private string? GetLayoutId()
    {
        var device = RegistryWord.HighWord(_handle);

        if ((device & 0xF000) == 0xF000)
        {
            var layoutId = device & 0x0FFF;

            using var key = Registry.LocalMachine.OpenSubKey(KeyboardLayoutsRegistryPath);

            if (key is null)
                return null;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey?.GetValue("Layout Id") is string subKeyLayoutId
                    && Convert.ToInt32(subKeyLayoutId, 16) == layoutId)
                {
                    VerifyThat.IsTrue(subKeyName.Length == 8, $"Incorrect layout id length: {subKey.Name}");
                    return subKeyName.ToUpperInvariant();
                }
            }

            return null;
        }

        if (device == 0)
        {
            device = RegistryWord.LowWord(_handle);
        }

        return device.ToString("X8");
    }

    private static unsafe class PInvoke
    {
        [DllImport("user32.dll")]
        public static extern int GetKeyboardLayoutList(int nBuff, [Out] nint* lpList);
    }

    private static class RegistryWord
    {
        public static int HighWord(nint n)
            => ((int)n >> 16) & 0xffff;

        public static int LowWord(nint n)
            => (int)n & 0xffff;
    }
}