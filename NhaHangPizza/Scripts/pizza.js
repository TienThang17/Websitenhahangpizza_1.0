/* pizza.js — Script chung cho Pizza House */
(function ($) {
    'use strict';

    // Cập nhật số lượng và tổng tiền trên header cart
    window.capNhatHeaderCart = function (soMon, tongTien) {
        var el = document.getElementById('cartCount');
        var ep = document.getElementById('cartPrice');
        if (el) el.textContent = soMon;
        if (ep) ep.textContent = tongTien;
    };

    // Hiển thị toast thông báo
    window.hienToastGlobal = function (msg, type) {
        type = type || 'success';
        var t = document.getElementById('toastNuoc');
        if (!t) {
            t = document.createElement('div');
            t.id = 'toastGlobal';
            t.className = 'toast-custom';
            document.body.appendChild(t);
        }
        t.textContent = msg;
        t.className = 'toast-custom alert alert-' + type;
        t.classList.remove('d-none');
        clearTimeout(t._timer);
        t._timer = setTimeout(function () { t.classList.add('d-none'); }, 2800);
    };

    // Cấm zoom bằng pinch trên màn hình cảm ứng (giữ giao diện tablet)
    document.addEventListener('touchmove', function (e) {
        if (e.touches.length > 1) e.preventDefault();
    }, { passive: false });

})(jQuery);
