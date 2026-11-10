using System.Collections.Generic;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// ViewModel cho form tùy chỉnh pizza (UC2)
    /// </summary>
    public class TuyChinPizzaViewModel
    {
        public int SoBan { get; set; }
        public int MaPizza { get; set; }
        public string TenPizza { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGiaCoban { get; set; }     // Giá cơ bản (size nhỏ)
        public int LoaiPizza { get; set; }            // 1=LamSan, 2=TuLam
        public string MoTa { get; set; }

        // Danh sách kích thước và vỏ bánh để render dropdown
        public List<KichThuocOption> DanhSachKichThuoc { get; set; } = new List<KichThuocOption>();
        public List<VoBanhOption> DanhSachVoBanh { get; set; } = new List<VoBanhOption>();

        // Thành phần (chỉ dùng cho pizza tự làm)
        public List<ThanhPhanOption> DanhSachThanhPhan { get; set; } = new List<ThanhPhanOption>();

        // Thành phần cố định (chỉ dùng cho pizza làm sẵn, hiển thị read-only)
        public List<string> ThanhPhanCodinh { get; set; } = new List<string>();

        // Giá trị form
        public int MaKichThuocChon { get; set; } = 1;   // Mặc định: nhỏ
        public int MaVoBanhChon { get; set; } = 1;       // Mặc định: dòn
        public int SoLuong { get; set; } = 1;
        public string GhiChu { get; set; }
        public List<int> ThanhPhanChon { get; set; } = new List<int>(); // MaTP đã chọn
    }

    public class KichThuocOption
    {
        public int MaKichThuoc { get; set; }
        public string TenKichThuoc { get; set; }
        public decimal HeSoGia { get; set; }
    }

    public class VoBanhOption
    {
        public int MaVoBanh { get; set; }
        public string TenVoBanh { get; set; }
        public decimal PhuThu { get; set; }
    }

    public class ThanhPhanOption
    {
        public int MaTP { get; set; }
        public string TenTP { get; set; }
        public decimal GiaTien { get; set; }
        public int TrangThai { get; set; }         // 1=còn, 0=hết
        public bool CoSan { get; set; }            // Sốt cà chua = true, không thể bỏ
    }
}
