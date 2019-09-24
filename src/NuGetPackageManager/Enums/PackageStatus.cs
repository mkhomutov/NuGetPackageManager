﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Enums
{
    public enum PackageStatus
    {
        NotInstalled, 
        LastVersionInstalled,
        UpdateAvailable,
        Pending
    }
}
