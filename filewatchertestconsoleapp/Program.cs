using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcherTestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("App Start");
            Watcher.StartWatcher("E:/TestTEC/Source");
            Console.ReadKey(true);
        }

        
    }
}
