using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlobStorage.Models;
using BlobStorage.Repository.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlobStorage.Controllers
{
    public class BlobController :Controller
    {

        private readonly IBlobStorageRepository _blobStorage;

        public BlobController(IBlobStorageRepository blobStorage)
        {
            _blobStorage = blobStorage;
        }
        public async Task< IActionResult> Index()
        {
            await _blobStorage.SetLogAsync("Listelem işlemi başlatıldı.", "systemlog.txt");
            var names = _blobStorage.GetNames(ContainerType.pictures);
            var blobUrl = $"{_blobStorage.BlobPath}{ContainerType.pictures.ToString()}";
            var blobList = names.Select(x => new BlobModel() { Name = x, Url = $"{blobUrl}/{x}" }).ToList();
            ViewBag.logs =await _blobStorage.GetLogAsync("systemlog.txt");
            return View(blobList);
        }
        [HttpPost]
        public async Task<ActionResult> Upload(IFormFile picture)
        {
            await _blobStorage.SetLogAsync("Upload işlemi başlatıldı.", "systemlog.txt");

            var quidPictureName = $"{Guid.NewGuid()}-{picture.FileName}";
            await _blobStorage.UploadAsync(picture.OpenReadStream(), quidPictureName, ContainerType.pictures);
            await _blobStorage.SetLogAsync("Upload işlemi tamamlandı.", "systemlog.txt");
            return RedirectToAction("Index");
        }
        public async Task<FileResult> Download(string fileName)
        {

            Stream stream = await _blobStorage.DownloadAsync(fileName, ContainerType.pictures);
            return File(stream, "application/octet-stream", fileName);
        }


        public async Task<ActionResult> Delete(string fileName)
        {
            await _blobStorage.DeleteAsync(fileName, ContainerType.pictures);
            return RedirectToAction("");
        }
    }
}