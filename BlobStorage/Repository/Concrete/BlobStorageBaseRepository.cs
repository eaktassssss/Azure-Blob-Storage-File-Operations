using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using BlobStorage.Client;
using BlobStorage.Models;
using BlobStorage.Repository.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlobStorage.Repository.Concrete
{
    public class BlobStorageBaseRepository :IBlobStorageRepository
    {
        public string BlobPath { get; } = "https://astorageaccountexample.blob.core.windows.net/";
        private readonly BlobServiceClient _blobServiceClient;
        public BlobStorageBaseRepository()
        {
            _blobServiceClient = new BlobServiceClient(BlobConnection.ConnectionString);
        }
        public async Task DeleteAsync(string fileName, ContainerType containerType)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerType.ToString());
                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteAsync();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<Stream> DownloadAsync(string fileName, ContainerType containerType)
        {
            try
            {
                /*
                 * Gönderilen container var mı bakılır.
                 */
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerType.ToString());
                /*
                 * Blob'a bağlanılır
                 */
                var blobClient = containerClient.GetBlobClient(fileName);
                /*
                 * Download edilir.Örnek olarak dönen content içeriğe eriştik content.value propertysi üzerinden
                 */
                var contentInfo = await blobClient.DownloadAsync();
                return contentInfo.Value.Content;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<List<string>> GetLogAsync(string fileName)
        {
            try
            {
                /*
                 *Logların okunacağını container'a ulaşıyoruz.
                 */
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerType.logs.ToString());
                /*
                 * Böyle bir container yoksa oluştur diyoruz
                 */
                await containerClient.CreateIfNotExistsAsync();
                /*
                 * Oluşturulan container üzerinden  GetAppendBlobClient ile berlitilen dökümana ait logları alıyoruz.
                 */
                var appendBlobClient = containerClient.GetAppendBlobClient(fileName);
                /*
                 * Böyle bir append blob yoksa oluştur diyoruz. Okuma işlemi yapabilmek için
                 */
                await appendBlobClient.CreateIfNotExistsAsync();

                List<string> logs = new List<string>();
                /*
                 * Logları satır satır okumak için öncelikle download edip elde ediyoruz
                 */
                var contentInfo = await appendBlobClient.DownloadAsync();

                /*
                 * Loglar satır satır okunur
                 */
                using (var read = new StreamReader(contentInfo.Value.Content))
                {
                    string line = string.Empty;
                    while ((line=read.ReadLine()) != null)
                    {
                        logs.Add(read.ReadLine());
                    }
                }
                return logs;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public List<string> GetNames(ContainerType containerType)
        {
            try
            {
                /*
                 * Verilen container ismine ait  blobs'ları getiriyoruz
                 */
                var blobNames = new List<string>();
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerType.ToString());
                var blobs = containerClient.GetBlobs();
                blobs.ToList().ForEach(x =>
                {
                    blobNames.Add(x.Name);
                });
                return blobNames;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task SetLogAsync(string text, string blobName)
        {
            try
            {
                /*
                 *Logların yazılacağı container'a ulaşıyoruz.
                 */
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerType.logs.ToString());
                /*
                 * Oluşturulan container üzerinden  GetAppendBlobClient ile berlitilen dökümana ait logları alıyoruz.
                 */
                var appendBlobClient = containerClient.GetAppendBlobClient(blobName);
                /*
                 * Böyle bir append blob yoksa oluştur diyoruz. Okuma işlemi yapabilmek için
                 */
                await appendBlobClient.CreateIfNotExistsAsync();

                /*
                 * Elimizde bulunan string veriyi memory stream'e yazıyoruz. Daha sonra bunu metoda parametre olarak geçicez.
                 */
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                    {
                        streamWriter.Write($"CreateTime{DateTime.Now}:{text}\n");
                        streamWriter.Flush();
                        memoryStream.Position = 0;
                        /*
                         * Yazma işlemini tamamlıyoruz.
                         */
                        await appendBlobClient.AppendBlockAsync(memoryStream);
                    }
                }


            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task UploadAsync(Stream stream, string name, ContainerType containerType)
        {
            try
            {
                /*
                 * Gönderilen container var mı bakılır.
                 */
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerType.ToString());
                /*
                 * Eğer container yoksa oluşturulucak
                 */
                await containerClient.CreateIfNotExistsAsync();
                /*
                 * Container'ı dış dünyaya açıyoruz. Bunu var olan bir container için Azure portalden de gerçekleştirebiliriz.
                 * Blob seviyesinde , container seviyesinde ve gizli olarak 3  erişim türü  vardır
                 */
                await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

                /*
                 * Bir döküman kaydetmek için Blob Client oluşturulur.
                 */

                var blobClient = containerClient.GetBlobClient(name);
                var contentInfo = await blobClient.UploadAsync(stream);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
    }
}
