using System;

namespace NhaHangPizza.Services
{
    /// <summary>
    /// Kết quả trả về từ cổng thanh toán
    /// </summary>
    public class KetQuaGiaoDich
    {
        public bool ThanhCong { get; set; }
        public string MaGiaoDich { get; set; }
        public string ThongBaoLoi { get; set; }
    }

    /// <summary>
    /// Interface cổng thanh toán - dễ thay thế bằng cổng thật sau
    /// </summary>
    public interface ICongThanhToan
    {
        KetQuaGiaoDich ThanhToanThe(string soThe, string tenChuThe, string ngayHetHan, string cvv, decimal soTien);
    }

    /// <summary>
    /// Cổng thanh toán giả lập (mock)
    /// Quy tắc: thẻ hợp lệ theo Luhn + còn hạn → thành công
    /// </summary>
    public class CongThanhToanMock : ICongThanhToan
    {
        private static readonly Random _rng = new Random();

        public KetQuaGiaoDich ThanhToanThe(string soThe, string tenChuThe, string ngayHetHan, string cvv, decimal soTien)
        {
            // Giả lập độ trễ 1-2 giây
            System.Threading.Thread.Sleep(_rng.Next(1000, 2000));

            // Kiểm tra độ dài CVV
            if (string.IsNullOrEmpty(cvv) || (cvv.Length != 3 && cvv.Length != 4))
                return new KetQuaGiaoDich { ThanhCong = false, ThongBaoLoi = "CVV không hợp lệ" };

            // Kiểm tra ngày hết hạn (định dạng MM/YY)
            if (!KiemTraNgayHetHan(ngayHetHan))
                return new KetQuaGiaoDich { ThanhCong = false, ThongBaoLoi = "Thẻ đã hết hạn" };

            // Kiểm tra số thẻ theo thuật toán Luhn
            if (!KiemTraLuhn(soThe))
                return new KetQuaGiaoDich { ThanhCong = false, ThongBaoLoi = "Số thẻ không hợp lệ" };

            // Thành công: tạo mã giao dịch ngẫu nhiên
            string maGD = "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + _rng.Next(1000, 9999);
            return new KetQuaGiaoDich { ThanhCong = true, MaGiaoDich = maGD };
        }

        /// <summary>
        /// Kiểm tra thuật toán Luhn cho số thẻ
        /// </summary>
        private bool KiemTraLuhn(string soThe)
        {
            if (string.IsNullOrEmpty(soThe)) return false;
            soThe = soThe.Replace(" ", "").Replace("-", "");
            if (soThe.Length < 13 || soThe.Length > 19) return false;

            int tong = 0;
            bool doubleDigit = false;
            for (int i = soThe.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(soThe[i])) return false;
                int digit = soThe[i] - '0';
                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                tong += digit;
                doubleDigit = !doubleDigit;
            }
            return tong % 10 == 0;
        }

        /// <summary>
        /// Kiểm tra ngày hết hạn thẻ (MM/YY)
        /// </summary>
        private bool KiemTraNgayHetHan(string ngayHetHan)
        {
            if (string.IsNullOrEmpty(ngayHetHan)) return false;
            var parts = ngayHetHan.Split('/');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out int thang) || !int.TryParse(parts[1], out int nam)) return false;
            if (thang < 1 || thang > 12) return false;

            int namDay = 2000 + nam;
            var ngayHet = new DateTime(namDay, thang, DateTime.DaysInMonth(namDay, thang));
            return ngayHet >= DateTime.Today;
        }
    }
}
