using System;
using System.Linq;
using System.Web.Mvc;
using NhaHangPizza.Models;
using NhaHangPizza.Services;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Controllers
{
    /// <summary>
    /// Controller thanh toán (UC6)
    /// </summary>
    public class ThanhToanController : Controller
    {
        private readonly PizzaDBEntities _db = new PizzaDBEntities();

        // GET: /ThanhToan/Index/5 - Hiển thị hóa đơn
        public ActionResult Index(int maDonHang)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;

            try
            {
                var donHangSvc = new DonHangService(_db);
                var vm = donHangSvc.LayHoaDon(maDonHang);

                if (vm == null)
                    return HttpNotFound("Không tìm thấy đơn hàng");

                return View(vm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Loi", "Home", new { thongBao = ex.Message });
            }
        }

        // GET: /ThanhToan/ThanhToanThe - Form nhập thông tin thẻ
        public ActionResult ThanhToanThe(int maDonHang)
        {
            var don = _db.DonHang.Find(maDonHang);
            if (don == null) return HttpNotFound();

            var vm = new ThanhToanTheViewModel
            {
                MaDonHang = maDonHang,
                SoBan = don.SoBan,
                TongTien = don.TongTien
            };
            return View(vm);
        }

        // POST: /ThanhToan/ThanhToanThe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThanhToanThe(ThanhToanTheViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var don = _db.DonHang.Find(form.MaDonHang);
                form.TongTien = don?.TongTien ?? 0;
                form.SoBan = don?.SoBan ?? 0;
                return View(form);
            }

            try
            {
                var thanhToanSvc = new ThanhToanService(_db, new CongThanhToanMock());
                var ketQua = thanhToanSvc.ThanhToanThe(form);

                if (ketQua.ThanhCong)
                {
                    return RedirectToAction("KetQua", new { maDonHang = form.MaDonHang, thanhCong = true });
                }
                else
                {
                    TempData["LoiThanhToan"] = ketQua.ThongBao;
                    return View(form);
                }
            }
            catch (Exception ex)
            {
                TempData["LoiThanhToan"] = "Lỗi hệ thống: " + ex.Message;
                var don = _db.DonHang.Find(form.MaDonHang);
                form.TongTien = don?.TongTien ?? 0;
                return View(form);
            }
        }

        // POST: /ThanhToan/YeuCauTienMat - Yêu cầu thanh toán tiền mặt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YeuCauTienMat(int maDonHang)
        {
            try
            {
                var thanhToanSvc = new ThanhToanService(_db, new CongThanhToanMock());
                thanhToanSvc.YeuCauThanhToanTienMat(maDonHang);

                TempData["ThongBaoTienMat"] = "Nhân viên sẽ đến thu tiền ngay. Vui lòng chờ!";
                return RedirectToAction("ChoThu", new { maDonHang });
            }
            catch (Exception ex)
            {
                TempData["LoiThanhToan"] = ex.Message;
                return RedirectToAction("Index", new { maDonHang });
            }
        }

        // GET: /ThanhToan/ChoThu - Màn hình chờ nhân viên thu tiền
        public ActionResult ChoThu(int maDonHang)
        {
            var don = _db.DonHang.Find(maDonHang);
            if (don == null) return HttpNotFound();
            ViewBag.MaDonHang = maDonHang;
            ViewBag.SoBan = don.SoBan;
            ViewBag.TongTien = don.TongTien;
            return View();
        }

        // GET: /ThanhToan/ThuNgan - Màn hình thu ngân (nhân viên)
        public ActionResult ThuNgan()
        {
            // Lấy tất cả đơn chờ thanh toán tiền mặt
            var dsDon = _db.DonHang
                .Where(d => !d.TrangThaiThanhToan && d.TrangThaiDon > 0)
                .OrderBy(d => d.NgayTao)
                .ToList();
            return View(dsDon);
        }

        // POST: /ThanhToan/XacNhanTienMat - Nhân viên xác nhận thu tiền
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanTienMat(XacNhanTienMatViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoiThuNgan"] = "Vui lòng nhập đúng thông tin";
                return RedirectToAction("ThuNgan");
            }

            try
            {
                var thanhToanSvc = new ThanhToanService(_db, new CongThanhToanMock());
                var ketQua = thanhToanSvc.XacNhanThuTienMat(form.MaDonHang, form.TienKhachDua);

                TempData["KetQuaThuNgan"] = ketQua;
                return RedirectToAction("KetQua", new { maDonHang = form.MaDonHang, thanhCong = true });
            }
            catch (Exception ex)
            {
                TempData["LoiThuNgan"] = ex.Message;
                return RedirectToAction("ThuNgan");
            }
        }

        // GET: /ThanhToan/KetQua - Màn hình hoàn tất
        public ActionResult KetQua(int maDonHang, bool thanhCong = true)
        {
            var don = _db.DonHang.Find(maDonHang);
            if (don == null) return HttpNotFound();

            var vm = new KetQuaThanhToanViewModel
            {
                ThanhCong = thanhCong,
                MaDonHang = maDonHang,
                SoBan = don.SoBan,
                TongTien = don.TongTien,
                ThongBao = thanhCong ? "Cảm ơn bạn đã sử dụng dịch vụ!" : "Thanh toán thất bại"
            };
            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
