using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Mods.DaggerLog
{
    public sealed class ExceptionDatabase
    {
        private HashSet<string> database = new HashSet<string>();
        public bool AddException(string exception) =>
            database.Add(exception);
    }
}