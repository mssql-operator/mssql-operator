using System;
using System.Collections.Generic;
using System.Text;
using Kanyon.Core;

namespace MSSqlOperator.Kapitan.V1Alpha1
{
    public class GarbageCollectionStrategy : WrappedString
    {
        public GarbageCollectionStrategy(string value = default) : base(value) { }

        public static implicit operator string(GarbageCollectionStrategy v)
        {
            return v.Value;
        }

        public static implicit operator GarbageCollectionStrategy(string v)
        {
            return new GarbageCollectionStrategy(v);
        }

        public static GarbageCollectionStrategy Retain = "Retain";
        public static GarbageCollectionStrategy Delete = "Delete";
        public static GarbageCollectionStrategy BackupAndDelete = "BackupAndDelete";
    }
}
