using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// ViewModel cho trang thanh toán (UC6)
    /// </summary>
    public class ThanhToanViewModel
    {
        public int SoBan { get; set; }
        public int MaDonHang { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayTao { get; set; }
        public List<HoaDonItem> ChiTietHoaDon { get; set; } = new List<HoaDonItem>();

        // Phương thức thanh toán: 1=TienMat, 2=The
        public int PhuongThucChon { get; set; } = 1;
    }

    public class HoaDonItem
    {
        public string TenMon { get; set; }
        public string MoTa { get; set; }     // Size, vỏ, topping
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    /// <summary>
    /// Form thanh toán thẻ tín dụng
    /// </summary>
    public class ThanhToanTheViewModel
    {
        public int MaDonHang { get; set; }
        public int SoBan { get; set; }
        public decimal TongTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số thẻ")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Số thẻ không hợp lệ (13-19 chữ số)")]
        public string SoThe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chủ thẻ")]
        [StringLength(100, ErrorMessage = "Tên chủ thẻ quá dài")]
        public string TenChuThe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày hết hạn")]
        public string NgayHetHan { get; set; }   // Dạng MM/YY

        [Required(ErrorMessage = "Vui lòng nhập CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV không hợp lệ")]
        public string CVV { get; set; }          // Chỉ dùng validate, không lưu
    }

    /// <summary>
    /// Form xác nhận thu tiền mặt (tại bếp/thu ngân)
    /// </summary>
    public class XacNhanTienMatViewModel
    {
        public int MaDonHang { get; set; }
        public decimal TongTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền khách đưa")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ")]
        public decimal TienKhachDua { get; set; }

        public decimal TienThua => TienKhachDua > 0 ? TienKhachDua - TongTien : 0;
    }

    /// <summary>
    /// Kết quả thanh toán
    /// </summary>
    public class KetQuaThanhToanViewModel
    {
        public bool ThanhCong { get; set; }
        public int MaDonHang { get; set; }
        public int SoBan { get; set; }
        public decimal TongTien { get; set; }
        public string ThongBao { get; set; }
        public int PhuongThuc { get; set; }     // 1=TienMat, 2=The
        public decimal? TienThua { get; set; }
        public string MaGiaoDich { get; set; }  // Dùng cho thẻ
    }
}
