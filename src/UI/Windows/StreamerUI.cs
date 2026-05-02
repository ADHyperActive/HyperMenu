using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace MalumMenu;

public class StreamerUI : MonoBehaviour
{
    private Camera _captureCamera;
    private Camera _uiCaptureCamera;
    private RenderTexture _renderTexture;
    private Texture2D _captureTexture;
    private int _lastWidth;
    private int _lastHeight;
    private bool _hostStarted;
    private bool _captureQueued;

    private void OnDestroy()
    {
        Cleanup();
        StreamerNativeWindowHost.Stop();
    }

    private void LateUpdate()
    {
        if (!CheatToggles.streamerMode || MalumMenu.isPanicked)
        {
            Cleanup();
            StreamerNativeWindowHost.Stop();
            _captureQueued = false;
            return;
        }

        var sourceCamera = Camera.main;
        if (!sourceCamera)
        {
            Cleanup();
            return;
        }

        if (!_hostStarted)
        {
            StreamerNativeWindowHost.Start();
            _hostStarted = true;
        }

        if (_captureQueued) return;
        _captureQueued = true;
        StartCoroutine(nameof(CaptureFrame));
    }

    private System.Collections.IEnumerator CaptureFrame()
    {
        yield return new WaitForEndOfFrame();

        if (!CheatToggles.streamerMode || MalumMenu.isPanicked)
        {
            _captureQueued = false;
            yield break;
        }

        var sourceCamera = Camera.main;
        if (!sourceCamera)
        {
            Cleanup();
            _captureQueued = false;
            yield break;
        }

        EnsureCaptureCamera(sourceCamera);
        EnsureUiCaptureCamera();
        if (!_captureCamera || !_renderTexture)
        {
            _captureQueued = false;
            yield break;
        }

        var disabledLineRenderers = DisableESPLineRenderers();

        _captureCamera.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);
        _captureCamera.orthographic = sourceCamera.orthographic;
        _captureCamera.orthographicSize = sourceCamera.orthographicSize;
        _captureCamera.fieldOfView = sourceCamera.fieldOfView;
        _captureCamera.nearClipPlane = sourceCamera.nearClipPlane;
        _captureCamera.farClipPlane = sourceCamera.farClipPlane;
        _captureCamera.backgroundColor = sourceCamera.backgroundColor;
        _captureCamera.clearFlags = sourceCamera.clearFlags;
        _captureCamera.cullingMask = sourceCamera.cullingMask;
        _captureCamera.allowHDR = sourceCamera.allowHDR;
        _captureCamera.allowMSAA = sourceCamera.allowMSAA;
        _captureCamera.targetTexture = _renderTexture;
        _captureCamera.Render();

        if (_uiCaptureCamera)
        {
            var hudManager = DestroyableSingleton<HudManager>.Instance;
            if (hudManager)
            {
                _uiCaptureCamera.transform.SetPositionAndRotation(hudManager.UICamera.transform.position, hudManager.UICamera.transform.rotation);
                _uiCaptureCamera.orthographic = hudManager.UICamera.orthographic;
                _uiCaptureCamera.orthographicSize = hudManager.UICamera.orthographicSize;
                _uiCaptureCamera.fieldOfView = hudManager.UICamera.fieldOfView;
                _uiCaptureCamera.nearClipPlane = hudManager.UICamera.nearClipPlane;
                _uiCaptureCamera.farClipPlane = hudManager.UICamera.farClipPlane;
                _uiCaptureCamera.backgroundColor = hudManager.UICamera.backgroundColor;
                _uiCaptureCamera.clearFlags = CameraClearFlags.Depth;
                _uiCaptureCamera.cullingMask = hudManager.UICamera.cullingMask;
                _uiCaptureCamera.allowHDR = hudManager.UICamera.allowHDR;
                _uiCaptureCamera.allowMSAA = hudManager.UICamera.allowMSAA;
                _uiCaptureCamera.targetTexture = _renderTexture;
                _uiCaptureCamera.Render();
            }
        }

        if (_captureTexture == null || _captureTexture.width != _renderTexture.width || _captureTexture.height != _renderTexture.height)
        {
            if (_captureTexture != null)
            {
                Destroy(_captureTexture);
            }

            _captureTexture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var previous = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        _captureTexture.ReadPixels(new Rect(0f, 0f, _renderTexture.width, _renderTexture.height), 0, 0, false);
        _captureTexture.Apply(false, false);
        RenderTexture.active = previous;

        RestoreESPLineRenderers(disabledLineRenderers);
        StreamerNativeWindowHost.UpdateFrame(_captureTexture.GetPixels32(), _captureTexture.width, _captureTexture.height);
        _captureQueued = false;
    }

    private void EnsureCaptureCamera(Camera sourceCamera)
    {
        if (_captureCamera && _renderTexture && _lastWidth == Screen.width && _lastHeight == Screen.height)
        {
            return;
        }

        Cleanup();

        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        var width = Mathf.Max(640, Screen.width);
        var height = Mathf.Max(360, Screen.height);
        _renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _renderTexture.Create();

        var cameraObject = new GameObject("MalumMenu_StreamerCamera")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _captureCamera = cameraObject.AddComponent<Camera>();
        _captureCamera.CopyFrom(sourceCamera);
        _captureCamera.enabled = false;
        _captureCamera.targetTexture = _renderTexture;
    }

    private void EnsureUiCaptureCamera()
    {
        if (_uiCaptureCamera || !DestroyableSingleton<HudManager>.Instance) return;

        var cameraObject = new GameObject("MalumMenu_StreamerUICamera")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _uiCaptureCamera = cameraObject.AddComponent<Camera>();
        _uiCaptureCamera.CopyFrom(DestroyableSingleton<HudManager>.Instance.UICamera);
        _uiCaptureCamera.enabled = false;
    }

    private static List<LineRenderer> DisableESPLineRenderers()
    {
        var disabled = new List<LineRenderer>();

        foreach (var lineRenderer in UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None))
        {
            if (!lineRenderer || !lineRenderer.enabled) continue;

            disabled.Add(lineRenderer);
            lineRenderer.enabled = false;
        }

        return disabled;
    }

    private static void RestoreESPLineRenderers(List<LineRenderer> lineRenderers)
    {
        foreach (var lineRenderer in lineRenderers)
        {
            if (lineRenderer)
            {
                lineRenderer.enabled = true;
            }
        }
    }

    private void Cleanup()
    {
        if (_captureCamera)
        {
            Destroy(_captureCamera.gameObject);
            _captureCamera = null;
        }

        if (_uiCaptureCamera)
        {
            Destroy(_uiCaptureCamera.gameObject);
            _uiCaptureCamera = null;
        }

        if (_renderTexture)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }

        if (_captureTexture)
        {
            Destroy(_captureTexture);
            _captureTexture = null;
        }
    }
}

internal static class StreamerNativeWindowHost
{
    private const int WsOverlappedWindow = 0x00CF0000;
    private const int SwShow = 5;
    private const uint WmDestroy = 0x0002;
    private const uint WmPaint = 0x000F;
    private const uint WmClose = 0x0010;
    private const uint Srccopy = 0x00CC0020;
    private const int DibRgbColors = 0;

    private static readonly object Sync = new();
    private static readonly WndProcDelegate WndProc = WndProcImpl;
    private static readonly IntPtr HInstance = GetModuleHandle(null);
    private static Thread _thread;
    private static IntPtr _hwnd = IntPtr.Zero;
    private static volatile bool _running;
    private static byte[] _frameBytes;
    private static int _frameWidth;
    private static int _frameHeight;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WndClassEx
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintStruct
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public uint[] bmiColors;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public static void Start()
    {
        if (_running) return;

        _running = true;
        _thread = new Thread(WindowThread)
        {
            IsBackground = true,
            Name = "MalumMenu Streamer Window"
        };

        try
        {
            _thread.SetApartmentState(ApartmentState.STA);
        }
        catch { }

        _thread.Start();
    }

    public static void Stop()
    {
        _running = false;

        var hwnd = _hwnd;
        if (hwnd != IntPtr.Zero)
        {
            PostMessage(hwnd, WmClose, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public static void UpdateFrame(Color32[] pixels, int width, int height)
    {
        if (!_running || pixels == null || pixels.Length == 0) return;

        var frame = new byte[width * height * 4];
        var index = 0;
        for (var i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            frame[index++] = pixel.b;
            frame[index++] = pixel.g;
            frame[index++] = pixel.r;
            frame[index++] = pixel.a;
        }

        lock (Sync)
        {
            _frameBytes = frame;
            _frameWidth = width;
            _frameHeight = height;
        }

        var hwnd = _hwnd;
        if (hwnd != IntPtr.Zero)
        {
            InvalidateRect(hwnd, IntPtr.Zero, false);
        }
    }

    private static void WindowThread()
    {
        const string className = "MalumMenuStreamerWindowClass";

        var wndClass = new WndClassEx
        {
            cbSize = (uint)Marshal.SizeOf<WndClassEx>(),
            style = 0x0003,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(WndProc),
            hInstance = HInstance,
            hCursor = LoadCursor(IntPtr.Zero, new IntPtr(32512)),
            hbrBackground = new IntPtr(5),
            lpszClassName = className
        };

        RegisterClassEx(ref wndClass);

        _hwnd = CreateWindowEx(
            0x00040000,
            className,
            "Streamer Mode Preview",
            WsOverlappedWindow,
            unchecked((int)0x80000000),
            unchecked((int)0x80000000),
            1280,
            720,
            IntPtr.Zero,
            IntPtr.Zero,
            HInstance,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
        {
            _running = false;
            return;
        }

        ShowWindow(_hwnd, SwShow);
        UpdateWindow(_hwnd);

        Msg msg;
        while (_running && GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        _hwnd = IntPtr.Zero;
        _running = false;
    }

    private static IntPtr WndProcImpl(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WmPaint:
                {
                    var ps = new PaintStruct();
                    var hdc = BeginPaint(hWnd, ref ps);
                    DrawFrame(hdc);
                    EndPaint(hWnd, ref ps);
                    return IntPtr.Zero;
                }
            case WmClose:
                DestroyWindow(hWnd);
                return IntPtr.Zero;
            case WmDestroy:
                PostQuitMessage(0);
                return IntPtr.Zero;
            default:
                return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    private static void DrawFrame(IntPtr hdc)
    {
        byte[] frameBytes;
        int width;
        int height;

        lock (Sync)
        {
            frameBytes = _frameBytes;
            width = _frameWidth;
            height = _frameHeight;
        }

        if (frameBytes == null || frameBytes.Length == 0 || width <= 0 || height <= 0)
        {
            return;
        }

        var bmi = new BitmapInfo
        {
            bmiHeader = new BitmapInfoHeader
            {
                biSize = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                biWidth = width,
                biHeight = height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0,
                biSizeImage = (uint)frameBytes.Length
            },
            bmiColors = new uint[1]
        };

        var handle = GCHandle.Alloc(frameBytes, GCHandleType.Pinned);
        try
        {
            StretchDIBits(hdc, 0, 0, width, height, 0, 0, width, height, handle.AddrOfPinnedObject(), ref bmi, DibRgbColors, Srccopy);
        }
        finally
        {
            handle.Free();
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern short RegisterClassEx(ref WndClassEx lpwcx);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref Msg lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref Msg lpmsg);

    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hWnd, ref PaintStruct lpPaint);

    [DllImport("user32.dll")]
    private static extern bool EndPaint(IntPtr hWnd, [In] ref PaintStruct lpPaint);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("gdi32.dll")]
    private static extern int StretchDIBits(IntPtr hdc, int xDest, int yDest, int DestWidth, int DestHeight,
        int xSrc, int ySrc, int SrcWidth, int SrcHeight, IntPtr lpBits, ref BitmapInfo lpbmi, int iUsage, uint rop);
}