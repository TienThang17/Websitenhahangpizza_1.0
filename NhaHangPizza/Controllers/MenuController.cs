using System;
using System.Web.Mvc;
using NhaHangPizza.Models;
using NhaHangPizza.Services;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Controllers
{
    /// <summary>
    /// Controller menu pizza (UC1, UC2)
    /// </summary>
    public class MenuController : Controller
    {
        private readonly PizzaDBEntities _db = new PizzaDBEntities();

        // GET: /Menu?soBan=5&boLoc=all
        public ActionResult Index(int? soBan, string boLoc = "all")
        {
            // Kiểm tra số bàn
            var gioHangSvc = new GioHangService(Session);
            var soBanHienTai = soBan ?? gioHangSvc.LaySoBan();
            if (!soBanHienTai.HasValue)
                return RedirectToAction("Index", "Home");

            gioHangSvc.LuuSoBan(soBanHienTai.Value);

            try
            {
                var pizzaSvc = new PizzaService(_db);
                var vm = new MenuViewModel
                {
                    SoBan = soBanHienTai.Value,
                    BoLoc = boLoc,
                    DanhSachPizza = pizzaSvc.LayDanhSachPizza(boLoc),
                    SoMonTrongGio = gioHangSvc.DemSoMon(soBanHienTai.Value),
                    TongTienGio = gioHangSvc.TinhTongTien(soBanHienTai.Value)
                };
                return View(vm);
            }
            catch (Exception)
            {
                // UC1: lỗi tải dữ liệu → trang thông báo thân thiện
                ViewBag.LoiMenu = "Không thể tải danh sách bánh pizza. Vui lòng thử lại!";
                return View(new MenuViewModel { SoBan = soBanHienTai.Value });
            }
        }

        // GET: /Menu/ChiTiet/5?soBan=3
        public ActionResult ChiTiet(int id, int? soBan)
        {
            var gioHangSvc = new GioHangService(Session);
            var soBanHienTai = soBan ?? gioHangSvc.LaySoBan();
            if (!soBanHienTai.HasValue)
                return RedirectToAction("Index", "Home");

            try
            {
                var pizzaSvc = new PizzaService(_db);
                var vm = pizzaSvc.LayChiTietPizza(id, soBanHienTai.Value);
                if (vm == null)
                    return HttpNotFound("Không tìm thấy pizza");

                return View(vm);
            }
            catch (Exception)
            {
                return RedirectToAction("Loi", "Home", new { thongBao = "Không thể tải thông tin bánh. Vui lòng thử lại!" });
            }
        }

        /// <summary>
        /// AJAX: Tính giá real-time khi khách thay đổi lựa chọn
        /// </summary>
        [HttpPost]
        public JsonResult TinhGia(int maPizza, int maKichThuoc, int maVoBanh, int[] maThanhPhan)
        {
            try
            {
                var pizzaSvc = new PizzaService(_db);
                var danhSachTP = maThanhPhan != null
                    ? new System.Collections.Generic.List<int>(maThanhPhan)
                    : new System.Collections.Generic.List<int>();

                decimal gia = pizzaSvc.TinhGiaPizza(maPizza, maKichThuoc, maVoBanh, danhSachTP);
                return Json(new { thanhCong = true, gia = gia, giaHienThi = gia.ToString("N0") + "đ" });
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
