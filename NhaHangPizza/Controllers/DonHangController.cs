using System;
using System.Linq;
using System.Web.Mvc;
using NhaHangPizza.Models;
using NhaHangPizza.Services;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Controllers
{
    /// <summary>
    /// Controller theo dõi đơn hàng và trang bếp (UC5)
    /// </summary>
    public class DonHangController : Controller
    {
        private readonly PizzaDBEntities _db = new PizzaDBEntities();

        // GET: /DonHang/TheoDoi/5 - Theo dõi tiến độ làm bánh
        public ActionResult TheoDoi(int maDonHang)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;

            try
            {
                var donHangSvc = new DonHangService(_db);
                var vm = donHangSvc.LayTheoDoiDon(maDonHang);

                if (vm == null)
                    return HttpNotFound("Không tìm thấy đơn hàng");

                return View(vm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Loi", "Home", new { thongBao = ex.Message });
            }
        }

        /// <summary>
        /// AJAX: Polling trạng thái đơn hàng (3-5 giây/lần)
        /// </summary>
        [HttpGet]
        public JsonResult LayTrangThai(int maDonHang)
        {
            try
            {
                var don = _db.DonHang.Find(maDonHang);
                if (don == null)
                    return Json(new { thanhCong = false, loi = "Không tìm thấy đơn" }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    thanhCong = true,
                    trangThaiDon = don.TrangThaiDon,
                    trangThaiThanhToan = don.TrangThaiThanhToan,
                    daMaDonHang = don.MaDonHang
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { thanhCong = false, loi = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Bep - Trang quản lý bếp (không cần đăng nhập, chỉ nội bộ)
        public ActionResult Bep()
        {
            try
            {
                var donHangSvc = new DonHangService(_db);
                var buocs = System.Linq.Enumerable.Select(
                    _db.BuocLamBanh.OrderBy(b => b.ThuTu),
                    b => new BuocViewModel { MaBuoc = b.MaBuoc, TenBuoc = b.TenBuoc, ThuTu = b.ThuTu });

                var vm = new BepViewModel
                {
                    DanhSachDon = donHangSvc.LayDanhSachDonChoKep(),
                    DanhSachBuoc = System.Linq.Enumerable.ToList(buocs)
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Loi", "Home", new { thongBao = ex.Message });
            }
        }

        // POST: /DonHang/ChuyenBuoc - Chuyển bước làm bánh
        [HttpPost]
        public JsonResult ChuyenBuoc(int maDonHang)
        {
            try
            {
                var donHangSvc = new DonHangService(_db);
                donHangSvc.ChuyenBuocTiepTheo(maDonHang);
                var don = _db.DonHang.Find(maDonHang);

                return Json(new
                {
                    thanhCong = true,
                    trangThaiMoi = don?.TrangThaiDon
                });
            }
            catch (Exception ex)
            {
                return Json(new { thanhCong = false, loi = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
