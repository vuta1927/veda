using System.Threading.Tasks;

namespace VDS.Configuration
{
    /// <summary>
    /// Represents an interface for a system that gets invoked when configuration has been done.
    /// </summary>
    public interface IWantToKnowWhenConfigurationIsDone
    {
        Task Configured(IConfigure configure);
    }
}