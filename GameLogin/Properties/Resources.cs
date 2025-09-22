using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace GameLogin.Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
                if (GameLogin.Properties.Resources.resourceMan == null)
                    GameLogin.Properties.Resources.resourceMan = new ResourceManager("GameLogin.Properties.Resources", typeof(GameLogin.Properties.Resources).Assembly);
                return GameLogin.Properties.Resources.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => GameLogin.Properties.Resources.resourceCulture;
            set => GameLogin.Properties.Resources.resourceCulture = value;
        }

        internal static Bitmap GameLauncherImage => (Bitmap)GameLogin.Properties.Resources.ResourceManager.GetObject(nameof(GameLogin), GameLogin.Properties.Resources.resourceCulture);
    }
}
