using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Productividad.Models
{
    public class FileModel
    {
        public string ElementId { get; set; }
        public IFormFile [] files { get; set; }
        public string FileTitle { get; set; }
    }
}