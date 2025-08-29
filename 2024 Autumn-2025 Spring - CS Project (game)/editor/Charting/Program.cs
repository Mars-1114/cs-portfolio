using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;

namespace Charting;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        try {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e) {
            string folder = Environment.CurrentDirectory + "\\CrashLog";
            string fileName = "Log" + DateTime.Now.ToString("yyMMdd-hhmmss") + ".txt";
            string crashLog = "Whoops! Looks like the editor has just self combusted...\n" +
                              "But don't worry! Your chart has been saved in the Autosave folder!\n" +
                              "If you're able to, please send this log to the developer so we can fix it (hopefully!)\n\n" +
                              "====================\n" +
                              "== CAUSE OF CRASH ==\n" +
                              "====================\n" +
                              e.ToString();
            Directory.CreateDirectory(folder);
            File.WriteAllText(folder + "\\" + fileName, crashLog);
            Process.Start("explorer.exe", "/select, \"" + folder + "\\" + fileName + "\"");
        }
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
