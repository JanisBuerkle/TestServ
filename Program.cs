using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

internal class Program
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_CHAR = 0x0102;
    private static void Main()
    {
        var url = "http://localhost:8080/";

        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add(url);

            listener.Start();
            Console.WriteLine($"Server gestartet. Lauscht auf {url}");

            try
            {
                while (true)
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        var parameterValue = context.Request.QueryString["parameter"];
                        Console.WriteLine($"Button wurde gedrückt! Parameter: {parameterValue}");
                        
                        if (parameterValue == "startServer")
                        {
                            try
                            {
                                using (var process = new Process())
                                {
                                    var exePath = @"C:\Users\bks\Downloads\console.bat";
                                    if (!string.IsNullOrEmpty(exePath))
                                        ExecuteExe(exePath);
                                    else
                                        Console.WriteLine("Ungültiger Pfad. Das Programm wird beendet.");
                                }
                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine($"Fehler beim Ausführen der Batch-Datei: {ex.Message}");
                            } 
                        }
                        else if (parameterValue == "stopServer")
                        {
                            SendKeysToConsole("Command zum Stoppen des Servers = 'stop'");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("Server gestoppt.");
            }
        }
    }
    
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public static void SendKeysToConsole(string text)
    {
        IntPtr hWnd = FindWindow(null, "Server");
        if (hWnd != IntPtr.Zero)
        {
            foreach (char c in text)
            {
                PostMessage(hWnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
            }
            PostMessage(hWnd, WM_CHAR, (IntPtr)0x0D, IntPtr.Zero);
        }
        else
        {
            Console.WriteLine("Fehler: Konnte das Fenster 'Server' nicht finden.");
        }
    }

    private static void ExecuteExe(string path)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    process.WaitForExit();
                    var exitCode = process.ExitCode;
                    Console.WriteLine($"Der Prozess wurde mit dem Exit-Code {exitCode} beendet.");
                }
                else
                {
                    Console.WriteLine("Fehler beim Starten des Prozesses.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }
}