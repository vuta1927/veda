using ApiServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.Merge
{
    public interface IMergeService
    {
        Task Execute(MergeModel.Merge mergeData);
    }
}
