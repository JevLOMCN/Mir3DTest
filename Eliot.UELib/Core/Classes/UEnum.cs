using System.Collections.Generic;

namespace UELib.Core
{
    /// <summary>Represents a unreal enum.</summary>
    [UnrealRegisterClass]
    public partial class UEnum : UField
    {
        /// <summary>Names of each element in the UEnum.</summary>
        public IList<UName> Names;

        protected override void Deserialize()
        {
            base.Deserialize();

            int count = _Buffer.ReadLength();
            Names = new List<UName>(count);
            for (var i = 0; i < count; ++i)
            {
                Names.Add(_Buffer.ReadNameReference());
            }

#if SPELLBORN
            if (_Buffer.Package.Build == UnrealPackage.GameBuild.BuildName.Spellborn
                && _Buffer.Version > 145)
            {
                uint unknownEnumFlags = _Buffer.ReadUInt32();
                // TODO: store/use unknownEnumFlags if needed
            }
#endif
        }
    }
}
