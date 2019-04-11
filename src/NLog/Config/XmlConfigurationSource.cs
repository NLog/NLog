namespace NLog.Config
{
    internal enum ConfigType { Uri, File }

    internal class XmlConfigurationSource
    {
        internal XmlConfigurationSource(ConfigType configType, string path)
        {
            ConfigType = configType;
            Path = path;
        }
        internal ConfigType ConfigType { get; }
        internal string Path { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (!(obj is XmlConfigurationSource other))
                return false;

            return other.ConfigType == this.ConfigType && other.Path == this.Path;
        }

        public override int GetHashCode()
        {
            return ConfigType.GetHashCode() | Path.GetHashCode();
        }
    }
}