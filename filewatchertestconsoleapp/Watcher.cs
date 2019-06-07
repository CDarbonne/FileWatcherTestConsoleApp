using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FileWatcherTestConsoleApp
{
    public class Watcher
    {
        private static System.Timers.Timer _reconnectTimer = new System.Timers.Timer();
        private static string _path;
        private static FileSystemWatcher _watcher = new FileSystemWatcher();

        public static void StartWatcher(string path)
        {
            _path = path;
            Task.Factory.StartNew(QueueHandler);
            ConnectWatcher(path);
            _reconnectTimer.Interval = 10000;
            _reconnectTimer.Elapsed += _reconnectTimer_Elapsed;
            _reconnectTimer.Enabled = true;
        }

        private static void _reconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_fileQueue.Count == 0)
            {
                ConnectWatcher(_path);
            }
        }

        private static void ConnectWatcher(string path)
        {
            Console.WriteLine("Checking for Directory...");
            if (!Directory.Exists(path))
            {
                if (Directory.Exists(Path.GetPathRoot(path)))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    Console.WriteLine("Directory Not Found. Sleeping...");
                    if (_reconnectTimer.Enabled)
                    {
                        _reconnectTimer.Enabled = false;
                    }
                    Thread.Sleep(5000);
                    ConnectWatcher(path);
                    return;
                }
            }

            if (!_reconnectTimer.Enabled)
            {
                _reconnectTimer.Enabled = true;
            }

            var files = Directory.GetFiles(path);
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    _fileQueue.Add(file);
                }
            }

            if (_watcher.EnableRaisingEvents)
            {
                Console.WriteLine("Restarting Watcher...");

                _watcher.EnableRaisingEvents = false;
                _watcher.EnableRaisingEvents = true;
                return;
            }            

            _watcher.Path = path;
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;


            // Add event handlers.
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            //watcher.Created += new FileSystemEventHandler(OnCreated);
            _watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            _watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            _watcher.EnableRaisingEvents = true;
            Console.WriteLine("Watcher is set up and watching directory: " + path);
            
        }

        private static BlockingCollection<string> _fileQueue = new BlockingCollection<string>();

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} deleted", e.FullPath);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            _fileQueue.Add(e.FullPath);
        }

        static void QueueHandler()
        {
            bool run = true;

            AppDomain.CurrentDomain.DomainUnload += (s, e) =>
            {
                run = false;
                //FileQueue.Take();
            };
            while (run)
            {
                string fileName;
                if (_fileQueue.TryTake(out fileName) && run)
                {
                    HandleFile(fileName, WatcherChangeTypes.Changed, Path.GetFileName(fileName));
                }
            }
        }

        private static void HandleFile(string fullPath, WatcherChangeTypes changeType, string name)
        {
            // Specify what is done when a file is created.
            Console.WriteLine("File: " + fullPath + " " + changeType);

            // Wait intial span before reading file
            System.Threading.Thread.Sleep(5000);

            //Identifier
            Guid associationIdentifier = Guid.NewGuid();

            //Directory for this File
            string fileDirectory = System.IO.Path.Combine("C:/TestTEC/Export", associationIdentifier.ToString());

            string workingFilePath = System.IO.Path.Combine(fileDirectory, associationIdentifier + System.IO.Path.GetExtension(name));

            //Move the file into it's own directory
            if (!File.Exists(fullPath))// || IsFileLocked(e.FullPath))
            {
                return;
            }
            else
            {
                try
                {
                    //Create a Directory
                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    Console.WriteLine("FileCreated: " + name);

                    File.Copy(fullPath, workingFilePath, false);
                    File.Delete(fullPath);
                }
                catch
                {
                    Console.WriteLine("Error Writing File: " + name);
                    return;
                }
            }
        }
    }
}
