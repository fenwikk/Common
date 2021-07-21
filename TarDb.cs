using System;
using System.IO;
using System.Threading.Tasks;
using Utf8Json;

namespace Tarsier.Common
{
    public class TarDb<T> : IDisposable where T : new()
    {
        public string path;

        public T Data
        {
            get
            {
                lastActivity = DateTime.UtcNow;
                if (Disposed)
                    InitDb();

                return _data;
            }
            set
            {
                lastActivity = DateTime.UtcNow;
                _data = value;
            }
        }

        private T _data;
        private DateTime lastActivity;
        private TimeSpan timeInMemory;
        public bool Disposed { get; private set; }

        public TarDb(string path, int timeInMemory = 60)
        {
            this.path = path;
            this.timeInMemory = TimeSpan.FromSeconds(timeInMemory);

            _data = new();
            SaveData();

            InitDb();

            Disposed = false;
            lastActivity = DateTime.UtcNow;
        }
        
        public byte[] SaveData()
        {
            lastActivity = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(Data);
            File.WriteAllBytes(path, json);
            return json;
        }

        public void Dispose()
        {
            Disposed = true;
            _data = default(T);
        }

        private void InitDb()
        {
            Disposed = false;
            var json = File.ReadAllBytes(path);

            Data = JsonSerializer.Deserialize<T>(json);

            Task.Run((Action) BackgroundStuf);
        }

        private void BackgroundStuf()
        {
            while (DateTime.UtcNow < lastActivity + timeInMemory);
            Dispose();
        }
    }
}