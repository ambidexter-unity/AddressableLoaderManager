// ReSharper disable once CheckNamespace
namespace Common.BundleManager
{
	public interface IAddressableLoaderManager
	{
		IAddressableLoader LoadAddressable(params string[] args);
	}
}