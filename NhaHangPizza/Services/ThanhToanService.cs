using System;
using System.Data.Entity;
using System.Linq;
using NhaHangPizza.Models;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Services
{
    /// <summary>
    /// Service xử lý nghiệp vụ thanh toán
    /// </summary>
    public class ThanhToanService
    {
        private readonly PizzaDBEntities _db;
        private readonly ICongThanhToan _congThanhToan;

        public ThanhToanService(PizzaDBEntities db, ICongThanhToan congThanhToan)
        {
            _db = db;
            _congThanhToan = congThanhToan;
        }

        /// <summary>
        /// Thanh toán bằng tiền mặt: ghi nhận yêu cầu, chờ nhân viên thu
        /// </summary>
        public int YeuCauThanhToanTienMat(int maDonHang)
        {
            var don = _db.DonHang.Find(maDonHang);
            if (don == null) throw new Exception("Không tìm thấy đơn hàng");
            if (don.TrangThaiThanhToan) throw new Exception("Đơn hàng đã được thanh toán");

            // Tạo bản ghi thanh toán ở trạng thái "Chờ thu" (TrangThai=0)
            var tt = new ThanhToan
            {
                MaDonHang = maDonHang,
                PhuongThuc = 1,         // 1 = Tiền mặt
                SoTien = don.TongTien,
                TrangThai = 0           // 0 = Chờ xử lý
            };
            _db.ThanhToan.Add(tt);
            _db.SaveChanges();

            // Thêm chi tiết tiền mặt (chưa biết tiền khách đưa)
            _db.ThanhToanTienMat.Add(new ThanhToanTienMat
            {
                MaThanhToan = tt.MaThanhToan
            });
            _db.SaveChanges();

            return tt.MaThanhToan;
        }

        /// <summary>
        /// Nhân viên xác nhận đã thu tiền mặt
        /// </summary>
        public KetQuaThanhToanViewModel XacNhanThuTienMat(int maDonHang, decimal tienKhachDua)
        {
            var don = _db.DonHang.Find(maDonHang);
            if (don == null) throw new Exception("Không tìm thấy đơn hàng");
            if (don.TrangThaiThanhToan) throw new Exception("Đơn hàng đã được thanh toán");
            if (tienKhachDua < don.TongTien)
                throw new Exception($"Tiền khách đưa ({tienKhachDua:N0}đ) ít hơn tổng tiền ({don.TongTien:N0}đ)");

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // Lấy bản ghi thanh toán đang chờ
                    var tt = _db.ThanhToan.FirstOrDefault(t => t.MaDonHang == maDonHang && t.PhuongThuc == 1 && t.TrangThai == 0);
                    if (tt == null)
                    {
                        // Tạo mới nếu chưa có
                        tt = new ThanhToan
                        {
                            MaDonHang = maDonHang,
                            PhuongThuc = 1,
                            SoTien = don.TongTien,
                            TrangThai = 0
                        };
                        _db.ThanhToan.Add(tt);
                        _db.SaveChanges();
                    }

                    // Cập nhật thanh toán tiền mặt
                    decimal tienThua = tienKhachDua - don.TongTien;
                    var tienMat = _db.ThanhToanTienMat.Find(tt.MaThanhToan);
                    if (tienMat == null)
                    {
                        _db.ThanhToanTienMat.Add(new ThanhToanTienMat
                        {
                            MaThanhToan = tt.MaThanhToan,
                            TienKhachDua = tienKhachDua,
                            TienThua = tienThua
                        });
                    }
                    else
                    {
                        tienMat.TienKhachDua = tienKhachDua;
                        tienMat.TienThua = tienThua;
                    }

                    // Cập nhật trạng thái thanh toán
                    tt.TrangThai = 1; // 1 = Đã thanh toán
                    tt.NgayThanhToan = DateTime.Now;

                    // Cập nhật đơn hàng và giải phóng bàn
                    don.TrangThaiThanhToan = true;
                    var ban = _db.BanAn.Find(don.SoBan);
                    if (ban != null) ban.TrangThaiBan = 0; // 0 = Trống

                    _db.SaveChanges();
                    transaction.Commit();

                    return new KetQuaThanhToanViewModel
                    {
                        ThanhCong = true,
                        MaDonHang = maDonHang,
                        SoBan = don.SoBan,
                        TongTien = don.TongTien,
                        PhuongThuc = 1,
                        TienThua = tienThua,
                        ThongBao = $"Thanh toán thành công! Tiền thừa: {tienThua:N0}đ"
                    };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Thanh toán bằng thẻ tín dụng
        /// </summary>
        public KetQuaThanhToanViewModel ThanhToanThe(ThanhToanTheViewModel form)
        {
            var don = _db.DonHang.Find(form.MaDonHang);
            if (don == null) throw new Exception("Không tìm thấy đơn hàng");
            if (don.TrangThaiThanhToan) throw new Exception("Đơn hàng đã được thanh toán");

            // Gửi đến cổng thanh toán (mock)
            var ketQua = _congThanhToan.ThanhToanThe(
                form.SoThe, form.TenChuThe, form.NgayHetHan, form.CVV, don.TongTien);

            if (!ketQua.ThanhCong)
            {
                return new KetQuaThanhToanViewModel
                {
                    ThanhCong = false,
                    MaDonHang = form.MaDonHang,
                    SoBan = don.SoBan,
                    TongTien = don.TongTien,
                    PhuongThuc = 2,
                    ThongBao = $"Giao dịch thất bại: {ketQua.ThongBaoLoi}"
                };
            }

            // Giao dịch thành công: lưu DB
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var tt = new ThanhToan
                    {
                        MaDonHang = don.MaDonHang,
                        PhuongThuc = 2,         // 2 = Thẻ
                        SoTien = don.TongTien,
                        NgayThanhToan = DateTime.Now,
                        TrangThai = 1           // 1 = Thành công
                    };
                    _db.ThanhToan.Add(tt);
                    _db.SaveChanges();

                    // Lấy cổng thanh toán
                    var cong = _db.CongThanhToan.FirstOrDefault();

                    // Chỉ lưu 4 số cuối thẻ, không lưu CVV
                    string soTheChe = "****" + (form.SoThe.Length >= 4
                        ? form.SoThe.Substring(form.SoThe.Length - 4)
                        : form.SoThe);

                    _db.ThanhToanThe.Add(new ThanhToanThe
                    {
                        MaThanhToan = tt.MaThanhToan,
                        MaCong = cong?.MaCong,
                        SoThe = soTheChe,       // Chỉ lưu 4 số cuối
                        TenChuThe = form.TenChuThe,
                        NgayHetHan = DateTime.ParseExact("01/" + form.NgayHetHan,
                            "dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture),
                        MaGiaoDich = ketQua.MaGiaoDich
                    });

                    // Cập nhật đơn hàng và giải phóng bàn
                    don.TrangThaiThanhToan = true;
                    var ban = _db.BanAn.Find(don.SoBan);
                    if (ban != null) ban.TrangThaiBan = 0;

                    _db.SaveChanges();
                    transaction.Commit();

                    return new KetQuaThanhToanViewModel
                    {
                        ThanhCong = true,
                        MaDonHang = don.MaDonHang,
                        SoBan = don.SoBan,
                        TongTien = don.TongTien,
                        PhuongThuc = 2,
                        MaGiaoDich = ketQua.MaGiaoDich,
                        ThongBao = $"Thanh toán thẻ thành công! Mã GD: {ketQua.MaGiaoDich}"
                    };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
