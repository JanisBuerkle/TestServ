using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        // Die URL, auf der der Server lauscht
        string url = "http://localhost:8080/";

        // Erstelle eine Instanz von HttpListener
        using (HttpListener listener = new HttpListener())
        {
            // Füge die URL zur Liste der Prefixe hinzu
            listener.Prefixes.Add(url);

            // Starte den Server
            listener.Start();
            Console.WriteLine($"Server gestartet. Lauscht auf {url}");

            try
            {
                // Warte auf Anfragen
                while (true)
                {
                    // Akzeptiere eine eingehende Anfrage
                    HttpListenerContext context = listener.GetContext();

                    // Verarbeite die Anfrage in einem separaten Thread, um den Server weiterhin zu akzeptieren
                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        // Hier füge eine Ausgabe in die Konsole ein
                        Console.WriteLine("Button wurde gedrückt!");

                        // Pfade zur Batch-Datei und zum Arbeitsverzeichnis
                        string batchFilePath = @"C:\Users\bks\Downloads\test.bat";
                        string workingDirectory = @"C:\Users\bks\Downloads\";

                        try
                        {
                            // Erstelle einen neuen Prozess
                            using (Process process = new Process())
                            {
                                // Konfiguriere den Prozess
                                ProcessStartInfo startInfo = new ProcessStartInfo
                                {
                                    FileName = batchFilePath,
                                    WorkingDirectory = workingDirectory,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                };

                                // Starte den Prozess
                                process.StartInfo = startInfo;
                                process.Start();

                                // Warte, bis der Prozess beendet ist
                                process.WaitForExit();

                                // Lese die Ausgabe des Prozesses
                                string output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();

                                // Gib die Ausgabe in die Konsole aus
                                Console.WriteLine($"Ausgabe des Prozesses: {output}");
                                Console.WriteLine($"Fehler des Prozesses: {error}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fehler beim Ausführen der Batch-Datei: {ex.Message}");
                        }

                        // Erstelle eine einfache Antwort mit mehr Informationen
                        string responseString = "Hallo, dies ist dein Server!\n";
                        responseString += $"Angeforderte URL: {context.Request.Url}\n";
                        responseString += $"HTTP-Methode: {context.Request.HttpMethod}\n";

                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                        // Sende die Antwort zurück an den Client
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
            }
            finally
            {
                // Stoppe den Server, wenn er nicht mehr benötigt wird
                listener.Stop();
                Console.WriteLine("Server gestoppt.");
            }
        }
    }
}
