using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using TestRunner.Utilities;
using Microsoft.Win32;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestRunner.Utilities.DTOs;
using System.Linq;

public class InstallChromeDriver
{
    private static readonly HttpClient httpClient = new HttpClient();

    public void Install() => Install(null, false);
    public void Install(string chromeVersion) => Install(chromeVersion, false);
    public void Install(bool forceDownload) => Install(null, forceDownload);

    public void Install(string chromeVersion, bool forceDownload)
    {
        Console.WriteLine("Update Chromedriver Selected");
        Console.WriteLine("Update in Process");
        Console.WriteLine("");

        // Instructions from https://chromedriver.chromium.org/downloads/version-selection
        //   First, find out which version of Chrome you are using. Let's say you have Chrome 72.0.3626.81.
        if (chromeVersion == null)
        {
            chromeVersion = GetChromeVersion();
        }

        chromeVersion = chromeVersion.Substring(0, chromeVersion.LastIndexOf('.'));

        Console.WriteLine($"Chrome Browser Version {chromeVersion} Detected");
        Console.WriteLine("");

        // Json endpoint to get all the chromedriver download info
        string uri = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";
        HttpResponseMessage chromeDriverVersionResponse = httpClient.GetAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
        var chromrDriverInfo = JsonConvert.DeserializeObject<ChromeDriverDto>(chromeDriverVersionResponse.Content.ReadAsStringAsync().Result);
     
        if (!chromeDriverVersionResponse.IsSuccessStatusCode)
        {
            if (chromeDriverVersionResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception($"ChromeDriver version not found for Chrome version {chromeVersion}");
            }
            else
            {
                throw new Exception($"ChromeDriver version request failed with status code: {chromeDriverVersionResponse.StatusCode}, reason phrase: {chromeDriverVersionResponse.ReasonPhrase}");
            }
        }

        string platformName;
        string driverName;
        string folderPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            platformName = "win64";
            driverName = "chromedriver.exe";
            folderPath = "chromedriver-win64/";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            platformName = "linux64";
            driverName = "chromedriver";
            folderPath = "chromedriver-linux64/";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            platformName = "mac-x64";
            driverName = "chromedriver";
            folderPath = "chromedriver-mac-x64/";
        }
        else
        {
            throw new PlatformNotSupportedException("Your operating system is not supported.");
        }

        string targetPath = RunnerConfiguration.ChromedriverPath;
        targetPath = Path.Combine(targetPath, driverName);
        if (!forceDownload && File.Exists(targetPath))
        {
            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = targetPath,
                    ArgumentList = { "--version" },
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            );
            string existingChromeDriverVersion = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Kill(true);

            // expected output is something like "ChromeDriver 88.0.4324.96 (68dba2d8a0b149a1d3afac56fa74648032bcf46b-refs/branch-heads/4324@{#1784})"
            // the following line will extract the version number and leave the rest
            existingChromeDriverVersion = existingChromeDriverVersion.Split(" ")[1];

            Console.WriteLine($"Existing Chromedriver Version: {existingChromeDriverVersion}");
            Console.WriteLine($"Download Candidate (Latest) for: {chromeVersion}");
            Console.WriteLine("");

            if (existingChromeDriverVersion.Contains(chromeVersion))
            {
                Console.WriteLine($"Existing Chromedriver Version: {existingChromeDriverVersion} is equal to Download Candidate: {chromeVersion}");
                Console.WriteLine("--Download Skipped--");
                return;
            }

            Console.WriteLine("Chromedriver needs to be updated! Downloading and Updating Now!");
            Console.WriteLine("");

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"Failed to execute {driverName} --version");
            }
        }

        // Filter the chromeDriverInfo json and find out the version block matching the local chrome browser version
        var version = chromrDriverInfo.versions.FindAll(x => x.version.Contains(chromeVersion)).LastOrDefault();
        // Filter the chromeDriver records based on the platform name
        var chromeDriver = version.downloads.chromedriver.Find(c => c.platform.Equals(platformName));
        HttpResponseMessage chromeDriverZipResponse = httpClient.GetAsync(chromeDriver.url).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!chromeDriverZipResponse.IsSuccessStatusCode)
        {
            throw new Exception($"ChromeDriver download request failed with status code: {chromeDriverZipResponse.StatusCode}, reason phrase: {chromeDriverZipResponse.ReasonPhrase}");
        }

        Console.WriteLine("Downloaded Starting to Unzip and copy over to folder");
        Console.WriteLine("");

        // this reads the zipfile as a stream, opens the archive,
        // and extracts the chromedriver executable to the targetPath without saving any intermediate files to disk
        using (var zipFileStream = chromeDriverZipResponse.Content.ReadAsStreamAsync().Result)
        using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
        using (var chromeDriverWriter = new FileStream(targetPath, FileMode.Create))
        {
            var entry = zipArchive.GetEntry(folderPath+driverName);
            using Stream chromeDriverStream = entry.Open();
            chromeDriverStream.CopyTo(chromeDriverWriter);
        }

        // on Linux/macOS, you need to add the executable permission (+x) to allow the execution of the chromedriver
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList = { "+x", targetPath },
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            );
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Kill(true);

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception("Failed to make chromedriver executable");
            }
        }

        Console.WriteLine("*****Updated ChromeDriver Local bin folder only: If you re-build solution you will need to check the box again!*****");
        Console.WriteLine("Do not include your updated chromedriver in PR unless specified.");
        Console.WriteLine("Instead: Please let the automation team know to submit a Pull Request to update chromedriver for everyone!");
    }

    public string GetChromeVersion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string chromePath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null);
            if (chromePath == null)
            {
                throw new Exception("Google Chrome not found in registry");
            }

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);

            return fileVersionInfo.FileVersion;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "google-chrome",
                        ArgumentList = { "--product-version" },
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                process.Kill(true);

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred trying to execute 'google-chrome --product-version'", ex);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                        ArgumentList = { "--version" },
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                process.Kill(true);

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                output = output.Replace("Google Chrome ", "");
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred trying to execute '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version'", ex);
            }
        }
        else
        {
            throw new PlatformNotSupportedException("Your operating system is not supported.");
        }
    }
}