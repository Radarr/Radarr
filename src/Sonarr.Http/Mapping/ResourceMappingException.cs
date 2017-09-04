using System;
using System.Collections.Generic;
using System.Linq;

namespace Lidarr.Http.Mapping
{
    public class ResourceMappingException : ApplicationException
    {
        public ResourceMappingException(IEnumerable<string> error)
            : base(Environment.NewLine + string.Join(Environment.NewLine, error.OrderBy(c => c)))
        {

        }
    }
}