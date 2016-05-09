
namespace LicenseManager.Library
{
    public interface IKeyFlagingFunction
    {
#if _KEYGEN
        byte[] GenerateBlockFlags();
#endif

        void DetectKeyIndex(byte[] flags, out int key, out int days);
    }
}
