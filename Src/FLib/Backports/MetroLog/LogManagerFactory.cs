namespace MetroLog
{
    public class LogManagerFactory
    {
        public class DefaultLogManager
        {
            public static ILogger GetLogger(string name)
            {
                return new StubLogger();
            }
        }
    }
}
