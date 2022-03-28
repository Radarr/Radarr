namespace NzbDrone.Core.Datastore
{
    public class PostgresOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string MainDb { get; set; }
        public string LogDb { get; set; }
    }
}
