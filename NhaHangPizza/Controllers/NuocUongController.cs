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
    /// Controller trang chọn nước uống (UC3)
    /// </summary>
    public class NuocUongController : Controller
    {
        private readonly PizzaDBEntities _db = new PizzaDBEntities();

        // GET: /NuocUong
        public ActionResult Index()
        {
            var gioHangSvc = new GioHangService(Session);
            var soBan = gioHangSvc.LaySoBan() ?? 0;
            if (soBan == 0)
                return RedirectToAction("Index", "Home");

            var danhSach = _db.NuocUong.Select(n => new NuocUongItem
            {
                MaNuoc = n.MaNuoc,
                TenNuoc = n.TenNuoc,
                DonGia = n.DonGia,
                SoLuongTon = n.SoLuongTon,
                HinhAnh = n.HinhAnh
            }).ToList();

            var vm = new NuocUongViewModel
            {
                SoBan = soBan,
                DanhSachNuoc = danhSach,
                SoMonTrongGio = gioHangSvc.DemSoMon(soBan),
                TongTienGio = gioHangSvc.TinhTongTien(soBan)
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
