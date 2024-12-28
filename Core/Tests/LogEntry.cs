using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace  BN.PROJECT.Core;

public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}
