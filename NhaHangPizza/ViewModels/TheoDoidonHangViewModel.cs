using System;
using System.Collections.Generic;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình theo dõi quy trình làm bánh (UC5)
    /// </summary>
    public class TheoDoidonHangViewModel
    {
        public int SoBan { get; set; }
        public int MaDonHang { get; set; }
        public int TrangThaiDon { get; set; }  // 0=Mới, 1-6=bước làm, 7=Hoàn thành
        public int? MaBuocHienTai { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayTao { get; set; }

        // Danh sách các bước (từ bảng BuocLamBanh)
        public List<BuocViewModel> DanhSachBuoc { get; set; } = new List<BuocViewModel>();

        public bool DaHoanThanh => TrangThaiDon >= 6;
        public bool ChoThanhToan => TrangThaiDon >= 6;
    }

    public class BuocViewModel
    {
        public int MaBuoc { get; set; }
        public string TenBuoc { get; set; }
        public int ThuTu { get; set; }
        public bool DaHoanThanh { get; set; }
        public bool DangLam { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang quản lý bếp (/Bep)
    /// </summary>
    public class BepViewModel
    {
        public List<DonHangBepItem> DanhSachDon { get; set; } = new List<DonHangBepItem>();
        public List<BuocViewModel> DanhSachBuoc { get; set; } = new List<BuocViewModel>();
    }

    public class DonHangBepItem
    {
        public int MaDonHang { get; set; }
        public int SoBan { get; set; }
        public int TrangThaiDon { get; set; }
        public int? MaBuocHienTai { get; set; }
        public string TenBuocHienTai { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayTao { get; set; }
        public bool TrangThaiThanhToan { get; set; }

        // Danh sách món trong đơn
        public List<string> TomTatMon { get; set; } = new List<string>();
    }
}
