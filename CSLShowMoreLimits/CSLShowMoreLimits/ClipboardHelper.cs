using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace CSLShowMoreLimits
{
    public class ClipboardHelper
    {
        private static PropertyInfo m_systemCopyBufferProperty = null;

        private static PropertyInfo GetSystemCopyBufferProperty()
        {
            if (m_systemCopyBufferProperty == null)
            {
                Type T = typeof(GUIUtility);
                m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
                if (m_systemCopyBufferProperty == null)
                {
                    //try future unity 5.2.3 - above is for before that build.
                    m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
                    if (m_systemCopyBufferProperty == null)
                    { 
                        Helper.dbgLog("**Could not access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed; are you using unity 5.2.3 or higher?");
                    }
                }

            }
            return m_systemCopyBufferProperty;
        }

        public static string clipBoard
        {
            get
            {
                PropertyInfo P = GetSystemCopyBufferProperty();
                return (string)P.GetValue(null, null);
            }
            set
            {
                PropertyInfo P = GetSystemCopyBufferProperty();
                P.SetValue(null, value, null);
            }
        }
    }
}
 