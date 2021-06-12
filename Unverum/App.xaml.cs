﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System;

namespace Unverum
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected static bool AlreadyRunning()
        {
            bool running = false;
            try
            {
                // Getting collection of process  
                Process currentProcess = Process.GetCurrentProcess();

                // Check with other process already running   
                foreach (var p in Process.GetProcesses())
                {
                    if (p.Id != currentProcess.Id) // Check running process   
                    {
                        if (p.ProcessName.Equals(currentProcess.ProcessName) && p.MainModule.FileName.Equals(currentProcess.MainModule.FileName))
                        {
                            running = true;
                            break;
                        }
                    }
                }
            }
            catch { }
            return running;
        }
        protected async override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            RegistryConfig.InstallGBHandler();
            MainWindow mw = new MainWindow();
            bool running = AlreadyRunning();
            if (!running)
            {
                mw.Show();
                // Only check for updates if Unverum wasn't launched by 1-click install button
                if (e.Args.Length == 0)
                    if (await AutoUpdater.CheckForUnverumUpdate(new CancellationTokenSource()))
                        mw.Close();
            }
            if (e.Args.Length > 1 && e.Args[0] == "-download")
                new ModDownloader().Download(e.Args[1], running);
            else if (running)
                MessageBox.Show("Unverum is already running", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Unhandled exception occured:\n{e.Exception.Message}\n\nInner Exception:\n{e.Exception.InnerException}" +
                $"\n\nStack Trace:\n{e.Exception.StackTrace}", "Error", MessageBoxButton.OK,
                             MessageBoxImage.Error);

            e.Handled = true;
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ((MainWindow)Current.MainWindow).ModGrid.IsHitTestVisible = true;
                ((MainWindow)Current.MainWindow).ConfigButton.IsHitTestVisible = true;
                ((MainWindow)Current.MainWindow).BuildButton.IsHitTestVisible = true;
                ((MainWindow)Current.MainWindow).LaunchButton.IsHitTestVisible = true;
                ((MainWindow)Current.MainWindow).OpenModsButton.IsHitTestVisible = true;
                ((MainWindow)Current.MainWindow).UpdateButton.IsHitTestVisible = true;
            });
        }
    }
}
