using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTInstaller.Interfaces
{
    public interface IResult
    {
        public bool Succeeded { get; }
        public string Message { get; }
    }
}
