using System.Collections.Generic;
using System.Linq;
using NhaHangPizza.Models;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Services
{
    /// <summary>
    /// Service xử lý nghiệp vụ liên quan đến Pizza và Menu
    /// </summary>
    public class PizzaService
    {
        private readonly PizzaDBEntities _db;

        public PizzaService(PizzaDBEntities db)
        {
            _db = db;
        }

        /// <summary>
        /// Lấy danh sách pizza đang bán, có thể lọc theo loại
        /// </summary>
        public List<PizzaCardViewModel> LayDanhSachPizza(string boLoc = "all")
        {
            var query = _db.Pizza.Where(p => p.TrangThai == true);

            if (boLoc == "lam-san")
                query = query.Where(p => p.LoaiPizza == 1);
            else if (boLoc == "tu-lam")
                query = query.Where(p => p.LoaiPizza == 2);

            return query.Select(p => new PizzaCardViewModel
            {
                MaPizza = p.MaPizza,
                TenPizza = p.TenPizza,
                HinhAnh = p.HinhAnh,
                DonGia = p.DonGia,
                LoaiPizza = p.LoaiPizza,
                MoTa = p.PizzaLamSan != null ? p.PizzaLamSan.MoTa : null
            }).ToList();
        }

        /// <summary>
        /// Lấy chi tiết pizza để hiện form tùy chỉnh
        /// </summary>
        public TuyChinPizzaViewModel LayChiTietPizza(int maPizza, int soBan)
        {
            var pizza = _db.Pizza
                .Include("PizzaLamSan")
                .Include("PizzaLamSan.ThanhPhan")
                .Include("PizzaTuLam")
                .FirstOrDefault(p => p.MaPizza == maPizza && p.TrangThai == true);

            if (pizza == null) return null;

            var vm = new TuyChinPizzaViewModel
            {
                SoBan = soBan,
                MaPizza = pizza.MaPizza,
                TenPizza = pizza.TenPizza,
                HinhAnh = pizza.HinhAnh,
                DonGiaCoban = pizza.DonGia,
                LoaiPizza = pizza.LoaiPizza,
                MoTa = pizza.PizzaLamSan?.MoTa
            };

            // Lấy danh sách kích thước
            vm.DanhSachKichThuoc = _db.KichThuoc.OrderBy(k => k.HeSoGia).Select(k => new KichThuocOption
            {
                MaKichThuoc = k.MaKichThuoc,
                TenKichThuoc = k.TenKichThuoc,
                HeSoGia = k.HeSoGia
            }).ToList();

            // Lấy danh sách vỏ bánh
            vm.DanhSachVoBanh = _db.VoBanh.OrderBy(v => v.PhuThu).Select(v => new VoBanhOption
            {
                MaVoBanh = v.MaVoBanh,
                TenVoBanh = v.TenVoBanh,
                PhuThu = v.PhuThu
            }).ToList();

            // Mặc định: kích thước nhỏ nhất, vỏ dòn (PhuThu thấp nhất)
            vm.MaKichThuocChon = vm.DanhSachKichThuoc.FirstOrDefault()?.MaKichThuoc ?? 1;
            vm.MaVoBanhChon = vm.DanhSachVoBanh.FirstOrDefault()?.MaVoBanh ?? 1;

            if (pizza.LoaiPizza == 2) // Pizza tự làm
            {
                // Lấy tất cả thành phần để khách chọn
                vm.DanhSachThanhPhan = _db.ThanhPhan.Select(tp => new ThanhPhanOption
                {
                    MaTP = tp.MaTP,
                    TenTP = tp.TenTP,
                    GiaTien = tp.GiaTien,
                    TrangThai = tp.TrangThai,
                    CoSan = false // Sốt cà chua mặc định được xử lý ở view
                }).ToList();
            }
            else // Pizza làm sẵn
            {
                // Thành phần cố định, hiển thị read-only
                vm.ThanhPhanCodinh = pizza.PizzaLamSan?.ThanhPhan
                    .Select(tp => tp.TenTP).ToList() ?? new List<string>();
            }

            return vm;
        }

        /// <summary>
        /// Tính giá pizza real-time (AJAX)
        /// </summary>
        public decimal TinhGiaPizza(int maPizza, int maKichThuoc, int maVoBanh, List<int> maThanhPhanList)
        {
            var pizza = _db.Pizza.Find(maPizza);
            if (pizza == null) return 0;

            var kichThuoc = _db.KichThuoc.Find(maKichThuoc);
            var voBanh = _db.VoBanh.Find(maVoBanh);
            if (kichThuoc == null || voBanh == null) return 0;

            // Giá = DonGia cơ bản × HeSoGia + PhuThu vỏ
            decimal gia = pizza.DonGia * kichThuoc.HeSoGia + voBanh.PhuThu;

            // Cộng thêm giá thành phần (chỉ pizza tự làm)
            if (pizza.LoaiPizza == 2 && maThanhPhanList != null && maThanhPhanList.Count > 0)
            {
                var giaTopping = _db.ThanhPhan
                    .Where(tp => maThanhPhanList.Contains(tp.MaTP) && tp.TrangThai == 1)
                    .Sum(tp => tp.GiaTien);
                gia += giaTopping;
            }

            return gia;
        }
    }
}
