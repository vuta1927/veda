namespace VDS.Configuration
{
    /// <summary>
    /// Interface for all configurators. Provides access to the <see cref="IConfigure"/>
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Gets the <see cref="IConfigure"/> to be used.
        /// </summary>
        IConfigure Configure { get; }
    }
}