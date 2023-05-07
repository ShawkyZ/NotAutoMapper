using System;
using System.Collections.Generic;

namespace NotAutoMapper.Common
{
    public class AutoMapAttribute : Attribute
    {
        public Type TargetType { get; }
        public IEnumerable<string> IgnoreProperty { get; }
        public AutoMapAttribute(Type targetType, params string[] ignoreProperty)
        {
            TargetType = targetType;
            IgnoreProperty = ignoreProperty;
        }
        public AutoMapAttribute(Type targetType) 
            => TargetType = targetType;
    }
}
