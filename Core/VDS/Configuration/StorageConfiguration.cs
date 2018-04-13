namespace VDS.Configuration
{
    /// <summary>
    /// An implementation of <see cref="IStorageConfiguration"/>
    /// </summary>
    internal class StorageConfiguration : IStorageConfiguration
    {
        public IConfigure Configure { get; }

        public StorageConfiguration(IConfigure configure)
        {
            Configure = configure;
        }
    }
}