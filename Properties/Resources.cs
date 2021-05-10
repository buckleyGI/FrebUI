using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace FREBUI.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (FREBUI.Properties.Resources.resourceMan == null)
          FREBUI.Properties.Resources.resourceMan = new ResourceManager("FREBUI.Properties.Resources", typeof (FREBUI.Properties.Resources).Assembly);
        return FREBUI.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => FREBUI.Properties.Resources.resourceCulture;
      set => FREBUI.Properties.Resources.resourceCulture = value;
    }
  }
}
