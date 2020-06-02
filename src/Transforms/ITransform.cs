using System.IO;
using System.Threading.Tasks;

namespace AlejoF.Thumbnailer.Transforms
{
    public interface ITransform
    {
        Task<bool> Execute(Stream input, string blobName, string outputBlobName);
    }
}