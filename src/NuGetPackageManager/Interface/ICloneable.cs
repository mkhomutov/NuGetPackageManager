﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Interface
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
