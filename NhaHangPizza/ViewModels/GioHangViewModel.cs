using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình giỏ hàng / xác nhận đặt bánh (UC4)
    /// </summary>
    public class GioHangViewModel
    {
        public int SoBan { get; set; }
        public List<GioHangItem> DanhSachMon { get; set; } = new List<GioHangItem>();

        // Tổng tiền tính lại ở server
        public decimal TongTien => TinhTongTien();

        private decimal TinhTongTien()
        {
            decimal tong = 0;
            foreach (var item in DanhSachMon)
                tong += item.ThanhTien;
            return tong;
        }

        public bool GioRong => DanhSachMon == null || DanhSachMon.Count == 0;
    }

    /// <summary>
    /// ViewModel kết quả sau khi đặt hàng thành công
    /// </summary>
    public class XacNhanDonHangViewModel
    {
        public int SoBan { get; set; }
        public int MaDonHang { get; set; }
        public decimal TongTien { get; set; }
        public string ThongBao { get; set; }
    }
}
