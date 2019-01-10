using System;
using System.IO;

namespace FileWatcherTestConsoleApp
{
    public class Watcher
    {
        public static void StartWatcher(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = path,
                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                //TODO: Change filter if necessary to appropriate file type
                // Only watch text files.
                //Filter = "*.txt"
            };

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            //watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Watcher is set up and watching directory: " + path);
        }

        //private static void OnChanged(object source, FileSystemEventArgs e)
        //{
        //    // Specify what is done when a file is changed or deleted.
        //    Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        //}

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }        

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is created.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);            

            // Wait intial span before reading file
            System.Threading.Thread.Sleep(5000);

            //Identifier
            Guid associationIdentifier = Guid.NewGuid();

            //Directory for this File
            string fileDirectory = System.IO.Path.Combine("C:/TestTEC/Export", associationIdentifier.ToString());

            string workingFilePath = System.IO.Path.Combine(fileDirectory, associationIdentifier + System.IO.Path.GetExtension(e.Name));
            
            //Move the file into it's own directory
            if (!File.Exists(e.FullPath))// || IsFileLocked(e.FullPath))
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

                    Console.WriteLine("FileCreated: " + e.Name);

                    File.Copy(e.FullPath, workingFilePath, false);
                    File.Delete(e.FullPath);
                }
                catch
                {
                    Console.WriteLine("Error Writing File: " + e.Name);
                    return;
                }
            }
        }
    }
}
