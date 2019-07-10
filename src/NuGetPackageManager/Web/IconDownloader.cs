namespace NuGetPackageManager.Web
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class IconDownloader
    {
        //readonly HttpSource httpSource = new HttpSource();
        public IconDownloader()
        {
            //this can be danger and considered harmful (we trust to all cerfs)
            //more info https://stackoverflow.com/questions/703272/could-not-establish-trust-relationship-for-ssl-tls-secure-channel-soap/6613434#6613434
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task<Stream> GetByUrlAsync(Uri uri)
        {
            var request = WebRequest.Create(uri);

            var response = await request.GetResponseAsync();

            var stream = response.GetResponseStream();

            return stream;
        }
    }
}
