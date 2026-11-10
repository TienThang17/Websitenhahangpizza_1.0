using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NhaHangPizza.Models;
using NhaHangPizza.Services;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Controllers
{
    /// <summary>
    /// Controller giỏ hàng (UC3, UC4)
    /// </summary>
    public class GioHangController : Controller
    {
        private readonly PizzaDBEntities _db = new PizzaDBEntities();

        // POST: /GioHang/ThemPizza - Thêm pizza vào giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemPizza(TuyChinPizzaViewModel form)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            if (soBan == 0)
                return RedirectToAction("Index", "Home");

            try
            {
                var pizzaSvc = new PizzaService(_db);

                // Tính giá lại ở server
                var maTP = form.ThanhPhanChon ?? new List<int>();
                decimal gia = pizzaSvc.TinhGiaPizza(
                    form.MaPizza, form.MaKichThuocChon, form.MaVoBanhChon, maTP);

                // Lấy tên kích thước và vỏ bánh
                var kichThuoc = _db.KichThuoc.Find(form.MaKichThuocChon);
                var voBanh = _db.VoBanh.Find(form.MaVoBanhChon);
                var pizza = _db.Pizza.Find(form.MaPizza);

                // Tạo item giỏ hàng
                var item = new GioHangItem
                {
                    MaPizza = form.MaPizza,
                    TenPizza = pizza?.TenPizza ?? form.TenPizza,
                    MaKichThuoc = form.MaKichThuocChon,
                    TenKichThuoc = kichThuoc?.TenKichThuoc,
                    MaVoBanh = form.MaVoBanhChon,
                    TenVoBanh = voBanh?.TenVoBanh,
                    SoLuong = form.SoLuong > 0 ? form.SoLuong : 1,
                    DonGia = gia,
                    GhiChu = form.GhiChu
                };

                // Nếu là pizza tự làm, thêm thành phần đã chọn
                if (pizza?.LoaiPizza == 2 && maTP.Count > 0)
                {
                    var danhSachTP = _db.ThanhPhan
                        .Where(tp => maTP.Contains(tp.MaTP) && tp.TrangThai == 1)
                        .ToList();

                    item.ThanhPhanDaChon = danhSachTP.Select(tp => new ThanhPhanItem
                    {
                        MaTP = tp.MaTP,
                        TenTP = tp.TenTP,
                        GiaTien = tp.GiaTien
                    }).ToList();
                }

                gioHangSvc.ThemVaoGio(soBan, item);

                TempData["ThanhCong"] = $"Đã thêm '{item.TenPizza}' vào giỏ hàng!";
                return RedirectToAction("Index", "GioHang");
            }
            catch (Exception ex)
            {
                TempData["LoiGio"] = ex.Message;
                return RedirectToAction("ChiTiet", "Menu", new { id = form.MaPizza });
            }
        }

        // POST: /GioHang/ThemNuoc - Thêm nước uống vào giỏ
        [HttpPost]
        public JsonResult ThemNuoc(int maNuoc, int soLuong = 1)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            if (soBan == 0)
                return Json(new { thanhCong = false, loi = "Không xác định được số bàn" });

            try
            {
                var nuoc = _db.NuocUong.Find(maNuoc);
                if (nuoc == null)
                    return Json(new { thanhCong = false, loi = "Nước uống không tồn tại" });
                if (nuoc.SoLuongTon < soLuong)
                    return Json(new { thanhCong = false, loi = $"Chỉ còn {nuoc.SoLuongTon} ly" });

                var item = new GioHangItem
                {
                    MaNuoc = maNuoc,
                    TenNuoc = nuoc.TenNuoc,
                    SoLuong = soLuong,
                    DonGia = nuoc.DonGia
                };

                gioHangSvc.ThemVaoGio(soBan, item);

                return Json(new
                {
                    thanhCong = true,
                    soMon = gioHangSvc.DemSoMon(soBan),
                    tongTien = gioHangSvc.TinhTongTien(soBan).ToString("N0") + "đ",
                    thongBao = $"Đã thêm {nuoc.TenNuoc}!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { thanhCong = false, loi = ex.Message });
            }
        }

        // GET: /GioHang - Xem giỏ hàng (UC4)
        public ActionResult Index()
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            if (soBan == 0)
                return RedirectToAction("Index", "Home");

            var vm = new GioHangViewModel
            {
                SoBan = soBan,
                DanhSachMon = gioHangSvc.LayGioHang(soBan)
            };
            return View(vm);
        }

        // POST: /GioHang/CapNhatSoLuong
        [HttpPost]
        public JsonResult CapNhatSoLuong(string itemKey, int soLuong)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            gioHangSvc.CapNhatSoLuong(soBan, itemKey, soLuong);

            return Json(new
            {
                thanhCong = true,
                soMon = gioHangSvc.DemSoMon(soBan),
                tongTien = gioHangSvc.TinhTongTien(soBan).ToString("N0") + "đ"
            });
        }

        // POST: /GioHang/XoaMon
        [HttpPost]
        public JsonResult XoaMon(string itemKey)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            gioHangSvc.XoaKhoiGio(soBan, itemKey);

            return Json(new
            {
                thanhCong = true,
                soMon = gioHangSvc.DemSoMon(soBan),
                tongTien = gioHangSvc.TinhTongTien(soBan).ToString("N0") + "đ"
            });
        }

        // POST: /GioHang/XacNhan - Xác nhận đặt hàng (UC4)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhan()
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            if (soBan == 0)
                return RedirectToAction("Index", "Home");

            var gio = gioHangSvc.LayGioHang(soBan);
            if (gio.Count == 0)
            {
                TempData["LoiGio"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            try
            {
                var donHangSvc = new DonHangService(_db);
                int maDonHang = donHangSvc.XacNhanDatHang(soBan, gio);

                // Xóa giỏ hàng sau khi đặt thành công
                gioHangSvc.XoaGioHang(soBan);

                // Lưu mã đơn hàng vào session để theo dõi
                Session["MaDonHangHienTai"] = maDonHang;

                return RedirectToAction("TheoDoi", "DonHang", new { maDonHang });
            }
            catch (Exception ex)
            {
                TempData["LoiGio"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
