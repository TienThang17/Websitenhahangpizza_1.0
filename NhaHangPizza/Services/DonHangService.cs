using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NhaHangPizza.Models;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Services
{
    /// <summary>
    /// Service xử lý đặt hàng và theo dõi đơn hàng
    /// </summary>
    public class DonHangService
    {
        private readonly PizzaDBEntities _db;

        public DonHangService(PizzaDBEntities db)
        {
            _db = db;
        }

        /// <summary>
        /// Xác nhận đặt hàng: tạo DonHang + ChiTietDonHang trong transaction
        /// Giá được tính lại ở server, không tin client
        /// </summary>
        public int XacNhanDatHang(int soBan, List<GioHangItem> gioHang)
        {
            if (gioHang == null || gioHang.Count == 0)
                throw new Exception("Giỏ hàng trống");

            // Kiểm tra bàn hợp lệ
            var ban = _db.BanAn.Find(soBan);
            if (ban == null)
                throw new Exception($"Bàn số {soBan} không tồn tại");

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    decimal tongTienServer = 0;

                    // --- Tạo đơn hàng ---
                    var donHang = new DonHang
                    {
                        SoBan = soBan,
                        NgayTao = DateTime.Now,
                        TongTien = 0,         // Cập nhật sau khi tính
                        TrangThaiDon = 0,     // 0 = Mới nhận
                        TrangThaiThanhToan = false
                    };
                    _db.DonHang.Add(donHang);
                    _db.SaveChanges(); // Lấy MaDonHang

                    // --- Tạo chi tiết từng món ---
                    foreach (var item in gioHang)
                    {
                        decimal donGiaServer = 0;

                        if (item.LaPizza)
                        {
                            // Kiểm tra pizza còn bán không
                            var pizza = _db.Pizza.Find(item.MaPizza.Value);
                            if (pizza == null || !pizza.TrangThai)
                                throw new Exception($"Bánh '{item.TenPizza}' hiện không còn bán");

                            var kichThuoc = _db.KichThuoc.Find(item.MaKichThuoc.Value);
                            var voBanh = _db.VoBanh.Find(item.MaVoBanh.Value);
                            if (kichThuoc == null || voBanh == null)
                                throw new Exception("Kích thước hoặc vỏ bánh không hợp lệ");

                            // Tính giá lại tại server
                            donGiaServer = pizza.DonGia * kichThuoc.HeSoGia + voBanh.PhuThu;

                            var chiTiet = new ChiTietDonHang
                            {
                                MaDonHang = donHang.MaDonHang,
                                MaPizza = pizza.MaPizza,
                                MaKichThuoc = kichThuoc.MaKichThuoc,
                                MaVoBanh = voBanh.MaVoBanh,
                                SoLuong = item.SoLuong,
                                DonGia = donGiaServer,
                                GhiChu = item.GhiChu
                            };

                            // Với pizza tự làm: thêm thành phần đã chọn
                            if (pizza.LoaiPizza == 2 && item.ThanhPhanDaChon != null)
                            {
                                decimal giaToppingServer = 0;
                                _db.ChiTietDonHang.Add(chiTiet);
                                _db.SaveChanges(); // Lấy MaChiTiet

                                foreach (var tp in item.ThanhPhanDaChon)
                                {
                                    var thanhPhan = _db.ThanhPhan.Find(tp.MaTP);
                                    if (thanhPhan == null) continue;
                                    if (thanhPhan.TrangThai == 0)
                                        throw new Exception($"Thành phần '{thanhPhan.TenTP}' đã hết");

                                    giaToppingServer += thanhPhan.GiaTien;
                                    _db.ChiTietTuLam_ThanhPhan.Add(new ChiTietTuLam_ThanhPhan
                                    {
                                        MaChiTiet = chiTiet.MaChiTiet,
                                        MaTP = thanhPhan.MaTP,
                                        GiaTien = thanhPhan.GiaTien
                                    });
                                }

                                // Cộng thêm giá topping
                                donGiaServer += giaToppingServer;
                                chiTiet.DonGia = donGiaServer;
                                chiTiet.ThanhTien = donGiaServer * chiTiet.SoLuong;
                                _db.SaveChanges();
                            }
                            else
                            {
                                chiTiet.ThanhTien = donGiaServer * chiTiet.SoLuong;
                                _db.ChiTietDonHang.Add(chiTiet);
                            }

                            tongTienServer += donGiaServer * item.SoLuong;
                        }
                        else if (item.LaNuoc)
                        {
                            // Kiểm tra nước còn tồn kho không
                            var nuoc = _db.NuocUong.Find(item.MaNuoc.Value);
                            if (nuoc == null)
                                throw new Exception($"Nước uống không tồn tại");
                            if (nuoc.SoLuongTon < item.SoLuong)
                                throw new Exception($"'{nuoc.TenNuoc}' không đủ số lượng (còn {nuoc.SoLuongTon})");

                            donGiaServer = nuoc.DonGia;

                            _db.ChiTietDonHang.Add(new ChiTietDonHang
                            {
                                MaDonHang = donHang.MaDonHang,
                                MaNuoc = nuoc.MaNuoc,
                                SoLuong = item.SoLuong,
                                DonGia = donGiaServer,
                                ThanhTien = donGiaServer * item.SoLuong,
                                GhiChu = item.GhiChu
                            });

                            // Trừ tồn kho nước
                            nuoc.SoLuongTon -= item.SoLuong;

                            tongTienServer += donGiaServer * item.SoLuong;
                        }
                    }

                    // Cập nhật tổng tiền đơn hàng
                    donHang.TongTien = tongTienServer;

                    // Cập nhật trạng thái bàn: 1 = Đang dùng
                    ban.TrangThaiBan = 1;

                    _db.SaveChanges();
                    transaction.Commit();

                    return donHang.MaDonHang;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Lấy thông tin đơn hàng để hiển thị theo dõi tiến độ
        /// </summary>
        public TheoDoidonHangViewModel LayTheoDoiDon(int maDonHang)
        {
            var don = _db.DonHang
                .Include("ChiTietDonHang")
                .Include("ChiTietDonHang.Pizza")
                .Include("ChiTietDonHang.NuocUong")
                .FirstOrDefault(d => d.MaDonHang == maDonHang);

            if (don == null) return null;

            var buocs = _db.BuocLamBanh.OrderBy(b => b.ThuTu).ToList();

            var vm = new TheoDoidonHangViewModel
            {
                SoBan = don.SoBan,
                MaDonHang = don.MaDonHang,
                TrangThaiDon = don.TrangThaiDon,
                MaBuocHienTai = don.BuocLamBanh?.MaBuoc,
                TongTien = don.TongTien,
                NgayTao = don.NgayTao,
                DanhSachBuoc = buocs.Select(b => new BuocViewModel
                {
                    MaBuoc = b.MaBuoc,
                    TenBuoc = b.TenBuoc,
                    ThuTu = b.ThuTu,
                    // Bước đã hoàn thành nếu TrangThaiDon >= ThuTu
                    DaHoanThanh = don.TrangThaiDon > b.ThuTu,
                    DangLam = don.TrangThaiDon == b.ThuTu
                }).ToList()
            };

            return vm;
        }

        /// <summary>
        /// Chuyển đơn sang bước tiếp theo (dùng cho trang Bếp)
        /// </summary>
        public bool ChuyenBuocTiepTheo(int maDonHang)
        {
            var don = _db.DonHang.Find(maDonHang);
            if (don == null) return false;

            int maxBuoc = _db.BuocLamBanh.Max(b => b.ThuTu);
            if (don.TrangThaiDon < maxBuoc)
            {
                don.TrangThaiDon++;
                _db.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng đang hoạt động cho trang Bếp
        /// </summary>
        public List<DonHangBepItem> LayDanhSachDonChoKep()
        {
            var dons = _db.DonHang
                .Include("ChiTietDonHang.Pizza")
                .Include("ChiTietDonHang.NuocUong")
                .Where(d => !d.TrangThaiThanhToan && d.TrangThaiDon < 7)
                .OrderBy(d => d.NgayTao)
                .ToList();

            return dons.Select(d => new DonHangBepItem
            {
                MaDonHang = d.MaDonHang,
                SoBan = d.SoBan,
                TrangThaiDon = d.TrangThaiDon,
                TongTien = d.TongTien,
                NgayTao = d.NgayTao,
                TrangThaiThanhToan = d.TrangThaiThanhToan,
                TomTatMon = d.ChiTietDonHang.Select(ct =>
                    ct.Pizza != null
                        ? $"{ct.Pizza.TenPizza} x{ct.SoLuong}"
                        : ct.NuocUong != null
                            ? $"{ct.NuocUong.TenNuoc} x{ct.SoLuong}"
                            : "").Where(s => !string.IsNullOrEmpty(s)).ToList()
            }).ToList();
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn để hiển thị trang thanh toán
        /// </summary>
        public ThanhToanViewModel LayHoaDon(int maDonHang)
        {
            var don = _db.DonHang
                .Include("ChiTietDonHang.Pizza")
                .Include("ChiTietDonHang.NuocUong")
                .Include("ChiTietDonHang.KichThuoc")
                .Include("ChiTietDonHang.VoBanh")
                .Include("ChiTietDonHang.ChiTietTuLam_ThanhPhan.ThanhPhan")
                .FirstOrDefault(d => d.MaDonHang == maDonHang);

            if (don == null) return null;

            var vm = new ThanhToanViewModel
            {
                SoBan = don.SoBan,
                MaDonHang = don.MaDonHang,
                TongTien = don.TongTien,
                NgayTao = don.NgayTao
            };

            foreach (var ct in don.ChiTietDonHang)
            {
                string moTa = "";
                if (ct.Pizza != null)
                {
                    moTa = $"{ct.KichThuoc?.TenKichThuoc}, {ct.VoBanh?.TenVoBanh}";
                    if (ct.ChiTietTuLam_ThanhPhan.Any())
                    {
                        var tenTopping = string.Join(", ", ct.ChiTietTuLam_ThanhPhan.Select(tp => tp.ThanhPhan.TenTP));
                        moTa += $" | Topping: {tenTopping}";
                    }
                }

                vm.ChiTietHoaDon.Add(new HoaDonItem
                {
                    TenMon = ct.Pizza?.TenPizza ?? ct.NuocUong?.TenNuoc,
                    MoTa = moTa,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien ?? (ct.DonGia * ct.SoLuong)
                });
            }

            return vm;
        }
    }
}
