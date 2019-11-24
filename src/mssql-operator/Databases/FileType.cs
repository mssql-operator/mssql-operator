using System;

namespace MSSqlOperator.Models
{
    public enum FileType {
        Unknown = 0,
        Data = 1,
        Log = 2, 
        Backup = 3,
        Bacpac = 4
    }
}