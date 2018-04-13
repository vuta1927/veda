namespace VDS
{
    /// <summary>
    /// Represents that the implemented classes are the objects that can be purged.
    /// </summary>
    public interface IPurgeable
    {
        /// <summary>
        /// Performs the purge operation.
        /// </summary>
        void Purge();
    }
}
