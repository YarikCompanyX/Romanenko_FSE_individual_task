using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public interface IFileHandlerFactory
    {
        ILoader CreateLoader();
        ISaver CreateSaver();
    }
}
