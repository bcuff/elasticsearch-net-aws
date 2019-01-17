using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0de39cec-a00f-409a-9d45-aafa31c77ed9")]

#if !DEBUG
[assembly: AssemblyKeyFile("key.snk")]
#else
[assembly: InternalsVisibleTo("ElasticSearch.Net.Aws.Tests")]
#endif
