using EZBlocker3.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EZBlocker3.Settings {
    public static class StartWithSpotify {

        private static readonly string SpotifyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\Spotify.exe";

        public static void SetEnabled(bool enabled) {
            if (IsRedirectionExecutableInstalled()) {
                if (!enabled)
                    Disable();
            } else {
                if (enabled)
                    Enable();
            }
        }
        public static void Enable() {
            InstallRedirectionExecutable(SpotifyPath);
        }
        public static void Disable() {
            UninstallRedirectionExecutable(SpotifyPath);
        }


        public static bool IsRedirectionExecutableInstalled() {
            var spotifyPath = SpotifyPath;
            var realSpotifyPath = Path.ChangeExtension(spotifyPath, "real.exe");
            return File.Exists(realSpotifyPath);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
                                            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
                                            uint cchBuffer);
        public static string GetShortName(string sLongFileName) {
            var buffer = new StringBuilder(259);
            var len = GetShortPathName(sLongFileName, buffer, (uint) buffer.Capacity);
            if (len == 0)
                throw new Win32Exception();
            return buffer.ToString();
        }

        // https://stackoverflow.com/a/11448060/6304917
        static void SaveAsIcon(Bitmap sourceBitmap, string filePath) {
            using var file = new FileStream(filePath, FileMode.Create);
            // ICO header
            file.WriteByte(0); file.WriteByte(0);
            file.WriteByte(1); file.WriteByte(0);
            file.WriteByte(1); file.WriteByte(0);

            // Image size
            file.WriteByte((byte)sourceBitmap.Width);
            file.WriteByte((byte)sourceBitmap.Height);
            // Palette
            file.WriteByte(0);
            // Reserved
            file.WriteByte(0);
            // Number of color planes
            file.WriteByte(0); file.WriteByte(0);
            // Bits per pixel
            file.WriteByte(32); file.WriteByte(0);

            // Data size, will be written after the data
            file.WriteByte(0);
            file.WriteByte(0);
            file.WriteByte(0);
            file.WriteByte(0);

            // Offset to image data, fixed at 22
            file.WriteByte(22);
            file.WriteByte(0);
            file.WriteByte(0);
            file.WriteByte(0);

            // Writing actual data
            sourceBitmap.Save(file, ImageFormat.Png);

            // Getting data length (file length minus header)
            long Len = file.Length - 22;

            // Write it in the correct place
            file.Seek(14, SeekOrigin.Begin);
            file.WriteByte((byte)Len);
            file.WriteByte((byte)(Len >> 8));

            file.Close();
        }

        public static void InstallRedirectionExecutable(string spotifyPath) {
            var spotifyDirectory = Path.GetDirectoryName(spotifyPath);
            var redirectExecutableTempPath = Path.Combine(spotifyDirectory, "EZBlocker3SpotifyRedirector.temp");
            var realSpotifyNewPath = Path.ChangeExtension(spotifyPath, "real.exe");

            using var spotifyIcon = Icon.ExtractAssociatedIcon(spotifyPath);
            var tempIconFilePath = Path.ChangeExtension(spotifyPath, ".temp.ico");
            using var spotifyIconBitmap = spotifyIcon.ToBitmap();
            SaveAsIcon(spotifyIconBitmap, tempIconFilePath);

            GenerateRedirectionExecutable(realSpotifyNewPath, redirectExecutableTempPath, tempIconFilePath);
            File.Move(spotifyPath, realSpotifyNewPath);
            File.Move(redirectExecutableTempPath, spotifyPath);
            File.Delete(tempIconFilePath);
        }
        public static void UninstallRedirectionExecutable(string spotifyPath) {
            var realSpotifyPath = Path.ChangeExtension(spotifyPath, "real.exe");
            // var redirectorPath = Path.ChangeExtension(spotifyPath, "redirector.exe");
            // File.Delete(redirectorPath);
            if (!File.Exists(realSpotifyPath))
                return;
            File.Delete(spotifyPath);
            File.Move(realSpotifyPath, spotifyPath);
        }

        public static bool GenerateRedirectionExecutable(string spotifyPath, string executablePath, string iconPath) {
            var parameters = new CompilerParameters {
                GenerateExecutable = true,
                OutputAssembly = executablePath,
                GenerateInMemory = false
            };
            parameters.ReferencedAssemblies.Add("System.dll");
            var shortName = /*Path.Combine(Path.GetDirectoryName(iconPath), "test.ico"); */ GetShortName(iconPath);
            parameters.CompilerOptions = $"/target:winexe \"/win32icon:{shortName}\"";

            var provider = CodeDomProvider.CreateProvider("CSharp");
            var code = _code
                .Replace("#---EZBLOCKER3PATH---#", App.Location)
                .Replace("#---SPOTIFYPATH---#", spotifyPath)
                .Replace("#---EZBLOCKER3ARGS---#", CliArgs.RedirectedSpotifyStartOption);
            var result = provider.CompileAssemblyFromSource(parameters, code);

            Logger.LogInfo("Settings: Generated redirection executable");
            foreach (CompilerError error in result.Errors)
                Logger.LogWarning($"Settings: Redirection executable generation {(error.IsWarning ? "warning" : "error")}:\n{error.ErrorText}");
            return result.Errors.Count == 0;
        }

        private const string _code = @"
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

public static class RedirectExecutable {
    public static int Main(string[] args) {
        var appPath = @""#---EZBLOCKER3PATH---#"";
        var realSpotifyPath = @""#---SPOTIFYPATH---#"";
        var spotifyPath = Assembly.GetExecutingAssembly().Location;

        var spotifyDirectory = Path.GetDirectoryName(spotifyPath);
        var redirectorTmpPath = Path.Combine(spotifyDirectory, ""Spotify.redirector.exe"");
        File.Delete(redirectorTmpPath);
        File.Move(spotifyPath, redirectorTmpPath);
        File.Move(realSpotifyPath, spotifyPath);

        Process.Start(appPath, ""#---EZBLOCKER3ARGS---#"");

        var process = new Process();
        process.StartInfo = new ProcessStartInfo() {
            FileName = spotifyPath,
            Arguments = string.Join("" "", args),
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        process.EnableRaisingEvents = true;
        process.OutputDataReceived += (s, e) => {
            Console.Out.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (s, e) => {
            Console.Error.WriteLine(e.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        process.CancelOutputRead();
        process.CancelErrorRead();

        if (File.Exists(realSpotifyPath))
            File.Delete(spotifyPath);
        else
            File.Move(spotifyPath, realSpotifyPath);
        File.Move(redirectorTmpPath, spotifyPath);

        return process.ExitCode;
    }
}";
    }
}
