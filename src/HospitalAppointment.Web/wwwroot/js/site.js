// Hastane Randevu Sistemi — Site JS

// Bootstrap alert'leri 5 saniye sonra otomatik kapat
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert.alert-success, .alert.alert-danger');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});
