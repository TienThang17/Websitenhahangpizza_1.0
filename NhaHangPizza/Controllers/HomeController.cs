using System;
using System.Linq;
using System.Web.Mvc;
using NhaHangPizza.Models;
using NhaHangPizza.Services;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Controllers
{
    /// <summary>
    /// Controller trang chủ / chọn bàn
    /// </summary>
    public class HomeController : Controller
    {
        private readonly PizzaDBEntities _db = new PizzaDBEntities();

        // GET: / hoặc /?soBan=5
        public ActionResult Index(int? soBan)
        {
            var gioHangSvc = new GioHangService(Session);

            // Nếu có số bàn trong URL, lưu vào session
            if (soBan.HasValue)
            {
                // Kiểm tra bàn có tồn tại không
                var ban = _db.BanAn.Find(soBan.Value);
                if (ban == null)
                {
                    ViewBag.LoiBan = $"Bàn số {soBan.Value} không tồn tại. Vui lòng chọn lại.";
                    return View("ChonBan", LayDanhSachBan());
                }
                gioHangSvc.LuuSoBan(soBan.Value);
            }

            // Kiểm tra session có số bàn chưa
            var soBanSession = gioHangSvc.LaySoBan();
            if (!soBanSession.HasValue)
            {
                return View("ChonBan", LayDanhSachBan());
            }

            // Chuyển thẳng sang menu
            return RedirectToAction("Index", "Menu", new { soBan = soBanSession.Value });
        }

        // GET: /Home/ChonBan
        public ActionResult ChonBan()
        {
            return View("ChonBan", LayDanhSachBan());
        }

        // POST: /Home/ChonBan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChonBan(int soBan)
        {
            return RedirectToAction("Index", new { soBan });
        }

        private ChonBanViewModel LayDanhSachBan()
        {
            var danhSachBan = _db.BanAn.ToList();
            return new ChonBanViewModel { DanhSachBan = danhSachBan };
        }

        // Trang lỗi thân thiện
        public ActionResult Loi(string thongBao)
        {
            ViewBag.ThongBao = thongBao ?? "Đã xảy ra lỗi. Vui lòng thử lại.";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// ViewModel cho trang chọn bàn
    /// </summary>
    public class ChonBanViewModel
    {
        public System.Collections.Generic.List<BanAn> DanhSachBan { get; set; }
    }
}
