using System.Security.Cryptography;
using System.Text;

namespace Utils;

public class HashUtils
{
    public static async Task<string> HashMd5(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                return Encoding.Default.GetString(await md5.ComputeHashAsync(stream));
            }
        }
    } 
}