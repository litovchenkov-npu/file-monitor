using System;
using System.IO;

// Клас, що містить додаткову інформацію про файлові події
public class FileEventArgs : EventArgs
{
    public string FilePath { get; }
    public long FileSize { get; }
    public DateTime Timestamp { get; }

    public FileEventArgs(string filePath, long fileSize, DateTime timestamp)
    {
        FilePath = filePath;
        FileSize = fileSize;
        Timestamp = timestamp;
    }
}

// Делегат для визначення подій файлового монітора
public delegate void FileMonitorDelegate(FileEventArgs e);

// Клас для відстеження файлових подій
public class FileMonitor
{
    // Події для різних файлових операцій
    public event FileMonitorDelegate OnFileCreated;
    public event FileMonitorDelegate OnFileDeleted;
    public event FileMonitorDelegate OnFileModified;
    public event FileMonitorDelegate OnFileRenamed;

    private FileSystemWatcher fileSystemWatcher;

    // Конструктор класу, приймає шлях та фільтр за замовчуванням "*.*"
    public FileMonitor(string directoryPath, string filter = "*.*")
    {
        // Ініціалізація об'єкта FileSystemWatcher для відстеження змін у каталозі
        fileSystemWatcher = new FileSystemWatcher(directoryPath, filter);

        // Приєднання обробників подій до відповідних подій FileSystemWatcher
        // Лямбда-вирази використовують e (EventArgs) для доступу до деталей подій
        fileSystemWatcher.Created += (sender, e) => OnFileCreated?.Invoke(new FileEventArgs(e.FullPath, GetFileSize(e.FullPath), DateTime.Now));
        fileSystemWatcher.Deleted += (sender, e) => OnFileDeleted?.Invoke(new FileEventArgs(e.FullPath, 0, DateTime.Now)); // Розмір файлу не доступний при видаленні
        fileSystemWatcher.Changed += (sender, e) => OnFileModified?.Invoke(new FileEventArgs(e.FullPath, GetFileSize(e.FullPath), DateTime.Now));
        fileSystemWatcher.Renamed += (sender, e) => OnFileRenamed?.Invoke(new FileEventArgs(e.FullPath, GetFileSize(e.FullPath), DateTime.Now));
    }

    // Метод для початку моніторингу
    public void StartMonitoring()
    {
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    // Метод для припинення моніторингу
    public void StopMonitoring()
    {
        fileSystemWatcher.EnableRaisingEvents = false;
    }

    // Метод для отримання розміру файлу
    private long GetFileSize(string filePath)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
        catch (Exception)
        {
            return 0; // Помилка отримання розміру файлу
        }
    }
}

class Program
{
    static void Main()
    {
        // Шлях до каталогу, який потрібно відстежувати
        string directoryPath = @"DIR_PATH";

        // Створення об'єкта FileMonitor
        FileMonitor fileMonitor = new FileMonitor(directoryPath, "*.txt");

        // Лямбда-вирази для обробки подій
        fileMonitor.OnFileCreated += e => Console.WriteLine($"Created file: {e.FilePath}, Size: {e.FileSize} bytes, Timestamp: {e.Timestamp}");
        fileMonitor.OnFileDeleted += e => Console.WriteLine($"Deleted file: {e.FilePath}, Timestamp: {e.Timestamp}");
        fileMonitor.OnFileModified += e => Console.WriteLine($"Modified file: {e.FilePath}, Size: {e.FileSize} bytes, Timestamp: {e.Timestamp}");
        fileMonitor.OnFileRenamed += e => Console.WriteLine($"Renamed file: {e.FilePath}, Size: {e.FileSize} bytes, Timestamp: {e.Timestamp}");

        // Початок моніторингу
        fileMonitor.StartMonitoring();

        Console.WriteLine("File monitoring system is on-line. Press Enter to exit.");
        Console.ReadLine();

        // Зупинка моніторингу
        fileMonitor.StopMonitoring();
    }
}