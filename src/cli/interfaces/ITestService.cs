using System.Threading.Tasks;

namespace donetcore_cli.interfaces
{
    public interface ITestService
    {
        Task SearchVideo(string videoSearch);
    }
}