
// This file has been generated by the GUI designer. Do not modify.

using System;
using Gtk;

namespace Stetic
{
    internal class Gui
    {
        private static bool initialized;

        internal static void Initialize(Widget iconRenderer)
        {
            if (initialized == false)
            {
                initialized = true;
            }
        }
    }

    internal class ActionGroups
    {
        public static ActionGroup GetActionGroup(Type type)
        {
            return GetActionGroup(type.FullName);
        }

        public static ActionGroup GetActionGroup(string name)
        {
            return null;
        }
    }
}
