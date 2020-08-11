using BlobStorage.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlobStorage.Repository.Abstract
{
    public interface IBlobStorageRepository
    {
        public string BlobPath { get;}
        Task UploadAsync(Stream stream, string name, ContainerType containerType);
        Task<Stream> DownloadAsync(string fileName, ContainerType containerType);
        /*
         * Dosya adına göre silme işlemi yapar
         */
        Task DeleteAsync(string fileName,ContainerType containerType);

        /*
         * Loglama işlemi gerçekleştirir.
         */
        Task SetLogAsync(string text, string blobName);

        /*
         * File name'e göre logları getirir
         */
        Task<List<string>> GetLogAsync(string fileName);

        /*
         * Container name'e göre blob içindeki tüm dökümanları çeker
         */
        List<string> GetNames(ContainerType containerType);
    }
}
