using System.Collections.Generic;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// ViewModel cho trang chọn nước uống (UC3)
    /// </summary>
    public class NuocUongViewModel
    {
        public int SoBan { get; set; }
        public List<NuocUongItem> DanhSachNuoc { get; set; } = new List<NuocUongItem>();
        public int SoMonTrongGio { get; set; }
        public decimal TongTienGio { get; set; }
    }

    public class NuocUongItem
    {
        public int MaNuoc { get; set; }
        public string TenNuoc { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongTon { get; set; }
        public string HinhAnh { get; set; }
        public bool ConHang => SoLuongTon > 0;
    }
}
