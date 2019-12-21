using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonitoringProject
{
    class Program
    {
        public static List<Task> MonitorServicesTasks = new List<Task>();
        public static List<MonitorConfiguration> Configurations;
        private static AsyncLocal<List<MonitorConfiguration>> ConfigurationsAsync = new AsyncLocal<List<MonitorConfiguration>>();

        static void Main(string[] args)
        {
            string configFileContent;
            using (StreamReader file = new StreamReader(@"C:\Users\just3\source\repos\MonitoringProject\AppConfig.json"))
            {
                configFileContent = file.ReadToEnd();
            }
            Configurations = JsonConvert.DeserializeObject<List<MonitorConfiguration>>(configFileContent);
            ConfigurationsAsync.Value = new List<MonitorConfiguration>();
            ConfigurationsAsync.Value.AddRange(Configurations);

            SetUpChecker();

            SetupServices();

            while (true)
            {
            }
        }

        public static void SetupServices()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            foreach (MonitorConfiguration monitorConfiguration in ConfigurationsAsync.Value)
            {
                MonitorServicesTasks.Add(Start(monitorConfiguration, token));
            }

            var task = Task.WhenAll(MonitorServicesTasks);
        }


        public static async Task Start(MonitorConfiguration configuration, CancellationToken cancellationToken)
        {
            while (true)
            {

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(configuration.WebSite);
                    request.Timeout = configuration.Timeout * 1000; //Timeout after 1000 ms
                    using (var stream = request.GetResponse().GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        Console.WriteLine($"{configuration.WebSite} is available.");
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        EmailService emailService = new EmailService();
                        await emailService.Send("sadasdasd");
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                await Task.Delay(configuration.Delay * 1000, cancellationToken);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void SetUpChecker()
        {

            var s = System.IO.Path.GetDirectoryName("AppConfig.json");
            FileSystemWatcher watcher = new FileSystemWatcher(@"C:\Users\just3\source\repos\MonitoringProject", "AppConfig.json");
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                            | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

            string configFileContent;
            Thread.Sleep(3000);
            using (StreamReader file = new StreamReader(e.FullPath))
            {
                configFileContent = file.ReadToEnd();
            }
            Configurations = JsonConvert.DeserializeObject<List<MonitorConfiguration>>(configFileContent);
            ConfigurationsAsync.Value = new List<MonitorConfiguration>();
            ConfigurationsAsync.Value.AddRange(Configurations);
            SetupServices();
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}
