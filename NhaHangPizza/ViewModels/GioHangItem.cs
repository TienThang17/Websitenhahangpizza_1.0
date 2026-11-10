using System;
using System.Collections.Generic;

namespace NhaHangPizza.ViewModels
{
    /// <summary>
    /// Đại diện một mục trong giỏ hàng (1 pizza hoặc 1 nước uống)
    /// </summary>
    public class GioHangItem
    {
        // Khóa duy nhất trong session (GUID để tránh trùng)
        public string ItemKey { get; set; } = Guid.NewGuid().ToString();

        // --- Pizza ---
        public int? MaPizza { get; set; }
        public string TenPizza { get; set; }
        public int? MaKichThuoc { get; set; }
        public string TenKichThuoc { get; set; }
        public int? MaVoBanh { get; set; }
        public string TenVoBanh { get; set; }

        // Thành phần đã chọn (chỉ áp dụng cho Pizza tự làm)
        public List<ThanhPhanItem> ThanhPhanDaChon { get; set; } = new List<ThanhPhanItem>();

        // --- Nước uống ---
        public int? MaNuoc { get; set; }
        public string TenNuoc { get; set; }

        // --- Chung ---
        public int SoLuong { get; set; } = 1;
        public decimal DonGia { get; set; }       // Giá 1 đơn vị (đã tính topping)
        public string GhiChu { get; set; }
        public bool LaPizza => MaPizza.HasValue;
        public bool LaNuoc => MaNuoc.HasValue;

        // Thành tiền = DonGia * SoLuong
        public decimal ThanhTien => DonGia * SoLuong;
    }

    /// <summary>
    /// Thành phần/topping được chọn trong giỏ hàng
    /// </summary>
    public class ThanhPhanItem
    {
        public int MaTP { get; set; }
        public string TenTP { get; set; }
        public decimal GiaTien { get; set; }
    }
}
