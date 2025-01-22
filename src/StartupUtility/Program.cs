using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    /// <summary>
    /// Default database name we want to use
    /// </summary>
    private const string DefaultDatabaseName = "CatFactsExplorerDb";

    /// <summary>
    /// Default server instance if user doesn't pass one
    /// </summary>
    private const string DefaultServerInstance = "localhostdb";

    private const string user = "SA";
    private const string password = "Your_password123";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting configuration...");

        // 1) Determine which SQL Server instance to use.
        //    If user provided an argument, use that; otherwise fallback to localhost\SQLEXPRESS.
        string serverInstance = args.Length > 0 ? args[0] : DefaultServerInstance;
        Console.WriteLine($"Using server instance: {serverInstance}");

        // 2) Check & Install Node.js
        EnsureNodeJsInstalled();

 
        // 5) Wait for the server to be ready (Windows Auth).
        //    If it's a remote server or you need SQL Auth, you'd have to adapt accordingly.
        await WaitForSqlServerReady(serverInstance);

        // 6) Run init SQL script (Windows Auth)
        RunInitSqlScriptIntegratedAuth(serverInstance);

        // 7) Update connection strings in local config
        var baseDirectory = AppContext.BaseDirectory;
        var api = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "src/CatFactsExplorer.API/appsettings.json"));
        var worker = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "src/CatFactsExplorer.Worker/appsettings.json"));

        UpdateConnectionString(api, serverInstance, DefaultDatabaseName);
        UpdateConnectionString(worker, serverInstance, DefaultDatabaseName);

        // 8) Start other services
        api = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "src/CatFactsExplorer.API"));
        worker = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "src/CatFactsExplorer.Worker"));
        var frontend = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "src/CatFactsExplorer.Frontend"));
        Console.WriteLine("Starting services...");
        StartProcess("dotnet", $"run --launch-profile \"http\" --project \"{api}\"");
        StartProcess("dotnet", $"run --launch-profile \"CatFactsExplorer.Worker\" --project \"{worker}\"");
        StartProcess("cmd.exe", $"/c npm run dev --prefix \"{frontend}\"");

        Console.ReadLine();
    }

    // ------------------------------------------------------------------------------
    //  Check if Node.js is installed, install if not
    // ------------------------------------------------------------------------------
    static void EnsureNodeJsInstalled()
    {
        Console.WriteLine("Checking if Node.js and npm are installed...");

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                string version = process.StandardOutput.ReadToEnd().Trim();
                Console.WriteLine($"npm is installed. Version: {version}");
                return;
            }
        }
        catch
        {
            Console.WriteLine("npm is not installed or not found. Installing Node.js...");
        }

        InstallNodeJs();
    }

    static void InstallNodeJs()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("Auto-install of Node.js is only implemented for Windows in this script. Please install manually.");
            Environment.Exit(1);
        }

        Console.WriteLine("Downloading Node.js for Windows...");
        var installerUrl = "https://nodejs.org/dist/v18.17.1/node-v18.17.1-x64.msi";
        var installerPath = Path.Combine(Path.GetTempPath(), "nodejs-installer.msi");

        using (var client = new WebClient())
        {
            client.DownloadFile(installerUrl, installerPath);
        }

        Console.WriteLine("Installing Node.js...");
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "msiexec",
            Arguments = $"/i \"{installerPath}\" /quiet /norestart",
            UseShellExecute = true
        });
        process.WaitForExit();

        Console.WriteLine("Node.js installed successfully.");
    }

    // ------------------------------------------------------------------------------
    //  Check & (optionally) Install SQL Express 2022 on Windows
    // ------------------------------------------------------------------------------
    static async Task EnsureSqlExpressInstalledAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("SQL Express installation automation is only implemented for Windows in this script.");
            return;
        }

        // Quick check: if sqlcmd is present, assume SQL is installed
        if (IsSqlCmdAvailable())
        {
            Console.WriteLine("sqlcmd is already available. Assuming SQL Express is installed.");
            return;
        }

        // Download and install SQL Express 2022
        Console.WriteLine("SQL Express not found. Installing SQL Server Express 2022...");

        var sqlExpressUrl = "https://go.microsoft.com/fwlink/?linkid=2208931";
        var installerPath = Path.Combine(Path.GetTempPath(), "SQLServer2022-SSEI-Expr.exe");

        using (var client = new WebClient())
        {
            Console.WriteLine($"Downloading SQL Express installer from {sqlExpressUrl} ...");
            await client.DownloadFileTaskAsync(sqlExpressUrl, installerPath);
        }

        Console.WriteLine("Running silent install of SQL Express 2022 (this may take several minutes)...");
        var install = Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = "/QS /ACTION=Install /FEATURES=SQL /INSTANCENAME=SQLEXPRESS /IACCEPTSQLSERVERLICENSETERMS=1",
            UseShellExecute = true,
            Verb = "runas"  // tries to run as administrator
        });
        install.WaitForExit();

        if (install.ExitCode != 0)
        {
            Console.WriteLine("SQL Express install failed or was cancelled. Exit code: " + install.ExitCode);
            Environment.Exit(1);
        }

        Console.WriteLine("SQL Express installed successfully.");
    }

    static bool IsSqlCmdAvailable()
    {
        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "sqlcmd",
                Arguments = "-?",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            p.WaitForExit();
            // Typically '-?' results in exit code 1, but if it runs, we know sqlcmd is present.
            return (p.ExitCode == 0 || p.ExitCode == 1);
        }
        catch
        {
            return false;
        }
    }

    // ------------------------------------------------------------------------------
    //  Enable TCP/IP for SQLEXPRESS (only if user is using localhost\SQLEXPRESS)
    // ------------------------------------------------------------------------------
    static void EnableTcpIpForSqlExpress()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("Skipping TCP/IP enable step: not on Windows.");
            return;
        }

        Console.WriteLine("Enabling TCP/IP for local SQL Server (SQLEXPRESS) on port 1433 via PowerShell...");

        // Adjust the namespace for your SQL version.
        // ComputerManagement15 => SQL 2022,
        // ComputerManagement14 => SQL 2019,
        // etc.
        // If you see a 'Namespace invalid' error, change '15' to '14' or '13', etc.
        string script = @"
Import-Module SQLPS -DisableNameChecking

try {
    Write-Host 'Attempting to enable TCP for SQLEXPRESS...'
    $namespace = 'root\Microsoft\SqlServer\ComputerManagement15'
    $wmi = Get-WmiObject -Namespace $namespace -Class ServerNetworkProtocol -ComputerName .

    $protocol = $wmi | Where-Object { $_.ProtocolName -eq 'Tcp' -and $_.InstanceName -eq 'SQLEXPRESS' }
    if ($protocol -eq $null) {
        Write-Host 'Could not find TCP protocol for instance SQLEXPRESS. Check instance name or version.'
        exit 1
    }

    # Enable TCP
    $protocol.IsEnabled = 1
    $protocol.Put() | Out-Null

    # Set port 1433 in IPAll
    $props = $protocol.GetRelated('ServerNetworkProtocolProperty')
    $ipAll = $props | Where-Object { $_.PropertyName -eq 'TcpPort' }
    if ($ipAll -eq $null) {
        Write-Host 'Could not find TcpPort property. Check your SQL configuration.'
        exit 1
    }
    $ipAll.PropertyStrValue = '1433'
    $ipAll.Put() | Out-Null

    # Restart the service
    Write-Host 'Restarting SQL Server (SQLEXPRESS)...'
    net stop 'SQL Server (SQLEXPRESS)' | Out-Null
    net start 'SQL Server (SQLEXPRESS)' | Out-Null

    Write-Host 'TCP/IP has been enabled for SQLEXPRESS on port 1433.'
    exit 0
}
catch {
    Write-Host $_.Exception.Message
    exit 1
}
";

        string tempScriptPath = Path.Combine(Path.GetTempPath(), "EnableTcpIpSqlExpress.ps1");
        File.WriteAllText(tempScriptPath, script);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempScriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas" // request admin
            }
        };

        process.Start();
        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        Console.WriteLine(output);
        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine(error);
        }

        if (process.ExitCode == 0)
        {
            Console.WriteLine("Successfully enabled TCP/IP on SQLEXPRESS (port 1433).");
        }
        else
        {
            Console.WriteLine("Failed to enable TCP/IP on SQLEXPRESS. Check logs above.");
        }
    }

    // ------------------------------------------------------------------------------
    //  Wait for SQL to be ready (Windows Auth)
    // ------------------------------------------------------------------------------
    static async Task WaitForSqlServerReady(string instanceName)
    {
        const int maxTries = 24; // ~2 minutes if each attempt waits 5s
        for (int i = 1; i <= maxTries; i++)
        {
            Console.WriteLine($"Checking SQL Server connectivity... attempt {i}/{maxTries}");

            var checkProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sqlcmd",
                    // Instead of -E (Windows Auth), use -U and -P for SQL Auth:
                    Arguments = $"-S \"{instanceName}\" -U {user} -P {password} -Q \"SELECT 1\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            checkProcess.Start();
            checkProcess.WaitForExit();

            if (checkProcess.ExitCode == 0)
            {
                Console.WriteLine("SQL Server is ready to accept connections.");
                return;
            }
            else
            {
                Console.WriteLine("SQL Server not ready yet...");
                var error = checkProcess.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine(error);
                }
                await Task.Delay(5000); // Wait 5s then retry
            }
        }

        Console.WriteLine("SQL Server did not become ready in the expected time. Exiting...");
        Environment.Exit(1);
    }

    // ------------------------------------------------------------------------------
    //  Run init SQL script using Windows Auth
    // ------------------------------------------------------------------------------
    static void RunInitSqlScriptIntegratedAuth(string instanceName)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var sqlFile = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "sql-scripts", "init.sql"));

        Console.WriteLine($"Absolute path to SQL script: {sqlFile}");

        if (!File.Exists(sqlFile))
        {
            Console.WriteLine("SQL script not found. Skipping database initialization...");
            return;
        }

        Console.WriteLine("Running SQL script to create the database...");
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "sqlcmd",
            Arguments = $"-S \"{instanceName}\" -U \"{user}\" -P \"{password}\" -i \"{sqlFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("Error running SQL script.");
            Console.WriteLine(process.StandardError.ReadToEnd());
            Environment.Exit(1);
        }

        Console.WriteLine("SQL script executed successfully.");
    }

    // ------------------------------------------------------------------------------
    //  Update ConnectionStrings in local config (keeping DB as CatFactsExplorerDb)
    // ------------------------------------------------------------------------------
    static void UpdateConnectionString(string filePath, string serverInstance, string dbName)
    {
        // Build your new connection string:
        var connectionString = $"Server={serverInstance};Database={dbName};User Id={user};Password={password};Encrypt=True;TrustServerCertificate=True;";

        // Read the existing JSON
        string json = File.ReadAllText(filePath);

        // Parse it as a JObject
        var jObject = JObject.Parse(json);

        // Navigate to "ConnectionStrings" : "MSSQL"
        // (Assuming "ConnectionStrings" is an object, and "MSSQL" is a property inside it)
        var connStrings = (JObject?)jObject["ConnectionStrings"];
        if (connStrings == null)
        {
            // If "ConnectionStrings" doesn't exist, create it
            connStrings = new JObject();
            jObject["ConnectionStrings"] = connStrings;
        }

        // Set the "MSSQL" property
        connStrings["MSSQL"] = connectionString;

        // Re-serialize to pretty JSON (Indented) or minified if you prefer
        string updatedJson = jObject.ToString(Formatting.Indented);

        // Overwrite the file
        File.WriteAllText(filePath, updatedJson);
    }

    // ------------------------------------------------------------------------------
    //  Start processes (API, Worker, Frontend)
    // ------------------------------------------------------------------------------
    static void StartProcess(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false  // This means "don't create a new *hidden* window"
        };

        var process = Process.Start(psi);

        // Optionally read the output asynchronously
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[{fileName}]: {e.Data}");
        };
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[{fileName} ERROR]: {e.Data}");
        };
        process.BeginErrorReadLine();

        // (Optional) If you want to wait for each process to finish:
        // process.WaitForExit();
    }
}