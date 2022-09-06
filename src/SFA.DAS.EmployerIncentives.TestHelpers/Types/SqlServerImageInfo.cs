namespace SFA.DAS.EmployerIncentives.TestHelpers.Types
{
    public class SqlServerImageInfo
    {
        public string DataSource => $"{ServerName}, {Port}";
        public string ServerName { get; private set; }
        public int Port { get; private set; }

        public SqlServerImageInfo(string serverName, int port)
        {
            ServerName = serverName;
            Port = port;
        }
    }
}
