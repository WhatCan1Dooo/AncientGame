﻿using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DropdownReferenceAttribute : ViewAttribute
    {
    }
}