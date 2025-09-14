namespace ImageManager.Logging
{
    public static class LoggerFactory
    {
        public static Logger GetLogger(string name) => new(name);
    }
}
