using VDS.Dependency;
using Microsoft.Extensions.Primitives;

namespace VDS.Caching
{
    public interface ISignal : ISingletonDependency
    {
        IChangeToken GetToken(string key);

        void SignalToken(string key);
    }
}
