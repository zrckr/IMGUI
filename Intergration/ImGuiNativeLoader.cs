using System.Reflection;
using System.Runtime.InteropServices;
using Common;

namespace IMGUI;

/// <summary>
/// Loads platform-specific Dear ImGui native libraries from embedded resources at runtime.
/// </summary>
internal static class ImGuiNativeLoader
{
    private const string ResourcePrefix = "IMGUI.Dependencies.bin";
    
    private const string TempDirectoryIdentifier = "fez_imgui";
    
    private static IntPtr _libraryPointer = IntPtr.Zero;
    
    /// <summary>
    /// Attempts to load the platform-specific native ImGui library from embedded resources.
    /// </summary>
    /// <returns>True if the library was successfully loaded, false otherwise.</returns>
    public static bool TryToLoad()
    {
        if (_libraryPointer != IntPtr.Zero)
        {
            return true;
        }

        var resourceName = GetNativeLibraryResourceName();
        if (resourceName == string.Empty)
        {
            return false;
        }

        var tempLibraryDirectoryPath = GetTempLibraryDirectoryPath();
        if (!Directory.Exists(tempLibraryDirectoryPath))
        {
            Directory.CreateDirectory(tempLibraryDirectoryPath);
        }

        var tempLibraryPath = GetTempLibraryPath();
        using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            using var fileStream = File.Create(tempLibraryPath);
            resourceStream!.CopyTo(fileStream);
        }

        _libraryPointer = NativeLibraryInterop.Load(tempLibraryPath);
        if (_libraryPointer == IntPtr.Zero)
        {
            Logger.Log("IMGUI", "Unable to load native Dear ImGui library.");
            return false;
        }
        
        Logger.Log("IMGUI", "Dear ImGui native library loaded successfully.");
        return true;
    }
    
    /// <summary>
    /// Determines the embedded resource name for the native library based on the current platform and architecture.
    /// </summary>
    /// <returns>The fully qualified resource name, or an empty string if the platform is unsupported.</returns>
    private static string GetNativeLibraryResourceName()
    {
        var platformIdentifier = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win_x86",
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win_x64",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win_arm64",
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux_x64",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "osx",
            _ => string.Empty
        };

        return $"{ResourcePrefix}.{platformIdentifier}.{GetNativeLibraryName()}";
    }
    
    /// <summary>
    /// Returns the platform-specific filename for the native ImGui library.
    /// </summary>
    /// <returns>The library filename (e.g., "cimgui.dll", "libcimgui.so"), or an empty string if unsupported.</returns>
    private static string GetNativeLibraryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "cimgui.dll";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "libcimgui.so";
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "libcimgui.dylib";
        }
        
        return string.Empty;
    }
    
    /// <summary>
    /// Gets the temporary directory path where the native library will be extracted.
    /// </summary>
    /// <returns>The full path to the temporary directory.</returns>
    private static string GetTempLibraryDirectoryPath()
    {
        return Path.Combine(Path.GetTempPath(), TempDirectoryIdentifier);
    }

    /// <summary>
    /// Gets the full file path where the native library will be extracted in the temp directory.
    /// </summary>
    /// <returns>The complete file path to the temporary library file.</returns>
    private static string GetTempLibraryPath()
    {
        return Path.Combine(GetTempLibraryDirectoryPath(), GetNativeLibraryName());
    }
    
    /// <summary>
    /// Unloads the native library and cleans up temporary files.
    /// </summary>
    public static void Unload()
    {
        if(_libraryPointer != IntPtr.Zero)
        {
            NativeLibraryInterop.Free(_libraryPointer);
            DeleteTempLibrary();
            _libraryPointer = IntPtr.Zero;
        }
    }
    
    /// <summary>
    /// Deletes the temporary library directory and all its contents.
    /// </summary>
    private static void DeleteTempLibrary()
    {
        var tempLibraryDirectoryPath = GetTempLibraryDirectoryPath();
        try
        {
            Directory.Delete(tempLibraryDirectoryPath, true);
        } 
        catch (Exception ex)
        {
            Logger.Log("IMGUI", $"Failed to delete temporary library directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Provides platform-specific P/Invoke methods for loading and freeing native libraries.
    /// </summary>
    private static class NativeLibraryInterop
    {
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("libdl", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Dlopen(string fileName, int flags);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("libdl", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Dlclose(IntPtr handle);

        /// <summary>
        /// Loads a native library from the specified file path using platform-specific APIs.
        /// </summary>
        /// <param name="fileName">The full path to the library file.</param>
        /// <returns>A handle to the loaded library, or IntPtr.Zero if loading failed.</returns>
        public static IntPtr Load(string fileName)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? LoadLibrary(fileName)
                : Dlopen(fileName, 1);
        }

        /// <summary>
        /// Frees a loaded native library using platform-specific APIs.
        /// </summary>
        /// <param name="libraryHandle">The handle to the library to free.</param>
        /// <returns>True if the library was successfully freed, false otherwise.</returns>
        public static bool Free(IntPtr libraryHandle)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? FreeLibrary(libraryHandle)
                : Dlclose(libraryHandle) == 0;
        }
    }
}