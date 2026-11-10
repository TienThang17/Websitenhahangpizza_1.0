using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using NhaHangPizza.ViewModels;

namespace NhaHangPizza.Services
{
    /// <summary>
    /// Service quản lý giỏ hàng lưu trong Session
    /// Giỏ hàng được gắn với số bàn, chỉ ghi DB khi khách xác nhận đặt
    /// </summary>
    public class GioHangService
    {
        private const string SESSION_KEY_PREFIX = "GioHang_Ban_";
        private readonly HttpSessionStateBase _session;

        public GioHangService(HttpSessionStateBase session)
        {
            _session = session;
        }

        private string GetKey(int soBan) => SESSION_KEY_PREFIX + soBan;

        /// <summary>
        /// Lấy danh sách món trong giỏ của bàn
        /// </summary>
        public List<GioHangItem> LayGioHang(int soBan)
        {
            var key = GetKey(soBan);
            var json = _session[key] as string;
            if (string.IsNullOrEmpty(json))
                return new List<GioHangItem>();
            return JsonConvert.DeserializeObject<List<GioHangItem>>(json) ?? new List<GioHangItem>();
        }

        private void LuuGioHang(int soBan, List<GioHangItem> items)
        {
            _session[GetKey(soBan)] = JsonConvert.SerializeObject(items);
        }

        /// <summary>
        /// Thêm món vào giỏ hàng
        /// </summary>
        public void ThemVaoGio(int soBan, GioHangItem item)
        {
            var gio = LayGioHang(soBan);
            item.ItemKey = System.Guid.NewGuid().ToString();
            gio.Add(item);
            LuuGioHang(soBan, gio);
        }

        /// <summary>
        /// Xóa một món khỏi giỏ theo khóa
        /// </summary>
        public void XoaKhoiGio(int soBan, string itemKey)
        {
            var gio = LayGioHang(soBan);
            gio.RemoveAll(i => i.ItemKey == itemKey);
            LuuGioHang(soBan, gio);
        }

        /// <summary>
        /// Cập nhật số lượng một món
        /// </summary>
        public void CapNhatSoLuong(int soBan, string itemKey, int soLuongMoi)
        {
            var gio = LayGioHang(soBan);
            var item = gio.Find(i => i.ItemKey == itemKey);
            if (item != null)
            {
                if (soLuongMoi <= 0)
                    gio.Remove(item);
                else
                    item.SoLuong = soLuongMoi;
            }
            LuuGioHang(soBan, gio);
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng (sau khi đặt hoặc huỷ)
        /// </summary>
        public void XoaGioHang(int soBan)
        {
            _session.Remove(GetKey(soBan));
        }

        /// <summary>
        /// Tính tổng tiền giỏ hàng
        /// </summary>
        public decimal TinhTongTien(int soBan)
        {
            var gio = LayGioHang(soBan);
            decimal tong = 0;
            foreach (var item in gio) tong += item.ThanhTien;
            return tong;
        }

        /// <summary>
        /// Đếm tổng số món (tính cả số lượng)
        /// </summary>
        public int DemSoMon(int soBan)
        {
            var gio = LayGioHang(soBan);
            int tong = 0;
            foreach (var item in gio) tong += item.SoLuong;
            return tong;
        }

        /// <summary>
        /// Lưu số bàn vào session
        /// </summary>
        public void LuuSoBan(int soBan)
        {
            _session["SoBan"] = soBan;
        }

        /// <summary>
        /// Lấy số bàn từ session
        /// </summary>
        public int? LaySoBan()
        {
            return _session["SoBan"] as int?;
        }
    }
}
