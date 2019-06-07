# FileWatcherTestConsoleApp
Console app made to test FileSystemWatcher

## Features

- QueueHandler to avoid overflow of FileSystemWatcher Memory
- ReconnectTimer to ensure the Directory is being watched even after a disconnect
- Directory Scanner to ensure files already present in directory upon startup are handled
