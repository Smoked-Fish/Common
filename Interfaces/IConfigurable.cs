using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IConfigurable
    {
        void SetConfig(string propertyName, object value);
        void InitializeDefaultConfig(string category = null);
    }
}
