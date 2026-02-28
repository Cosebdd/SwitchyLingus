using System.Diagnostics;

namespace SwitchyLingus.Setup
{
    internal class Program
    {
        [Flags]
        private enum SetupJobs
        {
            None = 0,
            CreateSetup = 1,
            CreatePortable = 1 << 1,
            OpenOutputDirectory = 1 << 2,

            Release = CreateSetup | OpenOutputDirectory,
            Portable = CreatePortable | OpenOutputDirectory,
            All = CreateSetup | CreatePortable | OpenOutputDirectory
        }

        private static SetupJobs Job = SetupJobs.Release;
        private static bool Silent;

        private static string ParentDir;
        private static string SetupDir;
        private static string BinDir;
        private static string OutputDir;
        private static string InnoSetupDir;

        private static string AppVersion;
        private static string SetupPath => Path.Combine(OutputDir, $"SwitchyLingus-{AppVersion}-setup.exe");
        private static string PortableZipPath => Path.Combine(OutputDir, $"SwitchyLingus-{AppVersion}-portable.zip");
        private static string PortableOutputDir => Path.Combine(OutputDir, "SwitchyLingus-portable");

        private static readonly string[] InnoSetupSearchPaths =
        {
            @"C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
            @"C:\Program Files\Inno Setup 6\ISCC.exe",
            @"C:\Program Files (x86)\Inno Setup 5\ISCC.exe",
            @"C:\Program Files\Inno Setup 5\ISCC.exe"
        };

        private static readonly string[] BinFiles =
        {
            "SwitchyLingus.UI.exe",
            "SwitchyLingus.UI.exe.config",
            "SwitchyLingus.Core.dll",
            "Hardcodet.Wpf.TaskbarNotification.dll",
            "Newtonsoft.Json.dll",
            "System.Management.Automation.dll",
            "Microsoft.Management.Infrastructure.dll"
        };

        private static void Main(string[] args)
        {
            Console.WriteLine("SwitchyLingus setup started.");

            CheckArgs(args);
            UpdatePaths();

            Console.WriteLine("Job: " + Job);
            Console.WriteLine("Version: " + AppVersion);
            Console.WriteLine();

            if (Directory.Exists(OutputDir))
            {
                Console.WriteLine("Cleaning output directory: " + OutputDir);
                Directory.Delete(OutputDir, true);
            }

            Directory.CreateDirectory(OutputDir);

            if (Job.HasFlag(SetupJobs.CreateSetup))
            {
                CreateSetup();
            }

            if (Job.HasFlag(SetupJobs.CreatePortable))
            {
                CreatePortable();
            }

            if (!Silent && Job.HasFlag(SetupJobs.OpenOutputDirectory))
            {
                Process.Start("explorer.exe", OutputDir);
            }

            Console.WriteLine();
            Console.WriteLine("SwitchyLingus setup successfully completed.");
        }

        private static void CheckArgs(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg.ToUpperInvariant())
                {
                    case "-SETUP":
                        Job = SetupJobs.CreateSetup | SetupJobs.OpenOutputDirectory;
                        break;
                    case "-PORTABLE":
                        Job = SetupJobs.Portable;
                        break;
                    case "-ALL":
                        Job = SetupJobs.All;
                        break;
                    case "-SILENT":
                        Silent = true;
                        break;
                }
            }
        }

        private static void UpdatePaths()
        {
            ParentDir = GetParentDir();
            SetupDir = Path.Combine(ParentDir, "SwitchyLingus.Setup");
            InnoSetupDir = Path.Combine(SetupDir, "InnoSetup");
            BinDir = Path.Combine(ParentDir, "SwitchyLingus.UI", "bin", "Release", "net9.0-windows", "win-x64");
            OutputDir = Path.Combine(ParentDir, "Output");

            if (!Directory.Exists(BinDir))
            {
                throw new DirectoryNotFoundException(
                    $"Release build output not found at: {BinDir}\n" +
                    "Build the solution in Release mode first.");
            }

            var exePath = Path.Combine(BinDir, "SwitchyLingus.UI.exe");
            if (File.Exists(exePath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                AppVersion = versionInfo.FileVersion ?? "1.0.0.0";
                // Trim trailing .0 segments for cleaner filenames (1.0.0.0 -> 1.0.0)
                while (AppVersion.EndsWith(".0") && AppVersion.Count(c => c == '.') > 2)
                    AppVersion = AppVersion.Substring(0, AppVersion.Length - 2);
            }
            else
            {
                AppVersion = "1.0.0";
            }
        }

        private static string GetParentDir()
        {
            // Walk up from the executing assembly to find the repo root (where SwitchyLingus.sln lives)
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "SwitchyLingus.sln")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }

            // Fallback: assume we're in SwitchyLingus.Setup\bin\<config>
            dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(dir, "..", "..", ".."));
        }

        private static void CreateSetup()
        {
            Console.WriteLine("Creating installer...");

            var issPath = Path.Combine(InnoSetupDir, "SwitchyLingus-setup.iss");
            if (!File.Exists(issPath))
                throw new FileNotFoundException("Inno Setup script not found: " + issPath);

            var isccPath = FindInnoSetupCompiler();
            if (isccPath == null)
                throw new FileNotFoundException(
                    "Inno Setup compiler (ISCC.exe) not found.\n" +
                    "Install Inno Setup 6 from https://jrsoftware.org/isdl.php");

            Console.WriteLine("Using ISCC: " + isccPath);

            var outputArg = $"/O\"{OutputDir}\"";
            var outputNameArg = $"/F\"SwitchyLingus-{AppVersion}-setup\"";

            RunProcess(isccPath, $"\"{issPath}\" {outputArg} {outputNameArg}");

            Console.WriteLine("Installer created: " + SetupPath);
        }

        private static void CreatePortable()
        {
            Console.WriteLine("Creating portable package...");

            if (Directory.Exists(PortableOutputDir))
                Directory.Delete(PortableOutputDir, true);

            Directory.CreateDirectory(PortableOutputDir);

            foreach (var file in BinFiles)
            {
                var src = Path.Combine(BinDir, file);
                var dst = Path.Combine(PortableOutputDir, file);
                if (File.Exists(src))
                {
                    File.Copy(src, dst, true);
                    Console.WriteLine("  Copied: " + file);
                }
                else
                {
                    Console.WriteLine("  WARNING: Missing file: " + file);
                }
            }

            // Create portable marker file
            File.WriteAllText(Path.Combine(PortableOutputDir, "portable"), "");

            // Create zip
            Console.WriteLine("Creating zip: " + PortableZipPath);
            System.IO.Compression.ZipFile.CreateFromDirectory(PortableOutputDir, PortableZipPath);

            // Clean up the folder, keep the zip
            Directory.Delete(PortableOutputDir, true);

            Console.WriteLine("Portable package created: " + PortableZipPath);
        }

        private static string FindInnoSetupCompiler()
        {
            return InnoSetupSearchPaths.FirstOrDefault(File.Exists);
        }

        private static void RunProcess(string filePath, string arguments)
        {
            Console.WriteLine($"Running: {filePath} {arguments}");
            Console.WriteLine();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) Console.Error.WriteLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"Process exited with code {process.ExitCode}: {filePath}");
        }
    }
}
