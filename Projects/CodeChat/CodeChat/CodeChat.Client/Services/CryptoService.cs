
    using Microsoft.JSInterop;
    using System.Threading.Tasks;
namespace CodeChat.Client.Services
{
    public class CryptoService
    {
        private readonly IJSRuntime _js;

        public CryptoService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string> EncryptAsync(string data)
        {
            return await _js.InvokeAsync<string>("cryptoHelper.encryptWithPublicKey", data);
        }

        public async Task<string> DecryptAsync(string encryptedData)
        {
            return await _js.InvokeAsync<string>("cryptoHelper.decryptWithPrivateKey", encryptedData);
        }
    }
}
