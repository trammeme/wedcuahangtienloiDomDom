using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.IO;

namespace DD.Controllers
{
    public class QRCodeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Sử dụng View() của Microsoft.AspNetCore.Mvc
        }

        [HttpPost] // Xóa bỏ tham chiếu đến System.Web.Mvc
        public IActionResult GenerateQRCode(string customerData)
        {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(customerData, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20); // Thay đổi kích thước nếu cần

            // Chuyển đổi Bitmap thành một định dạng mà bạn có thể trả về
            using (var memoryStream = new MemoryStream())
            {
                qrCodeImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                var byteArray = memoryStream.ToArray();
                return File(byteArray, "image/png"); // Đảm bảo sử dụng File() của Microsoft.AspNetCore.Mvc
            }
        }
    }
}
