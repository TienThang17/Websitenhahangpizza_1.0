using System.Collections.Generic;
using NhaHangPizza.Models;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// ViewModel cho trang menu (UC1)
    /// </summary>
    public class MenuViewModel
    {
        public int SoBan { get; set; }
        public List<PizzaCardViewModel> DanhSachPizza { get; set; } = new List<PizzaCardViewModel>();
        public string BoLoc { get; set; } = "all"; // "all" / "lam-san" / "tu-lam"
        public int SoMonTrongGio { get; set; }
        public decimal TongTienGio { get; set; }
    }

    /// <summary>
    /// Thẻ pizza hiển thị trong lưới menu
    /// </summary>
    public class PizzaCardViewModel
    {
        public int MaPizza { get; set; }
        public string TenPizza { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; }         // Giá cơ bản (size nhỏ)
        public int LoaiPizza { get; set; }           // 1=LamSan, 2=TuLam
        public string TenLoai => LoaiPizza == 2 ? "Pizza tự làm" : "Pizza làm sẵn";
        public string MoTa { get; set; }             // Chỉ có ở PizzaLamSan
    }
}
