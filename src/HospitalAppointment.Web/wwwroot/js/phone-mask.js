// Telefon maskesi — +90 (5xx) xxx xx xx
// Tüm .phone-mask sınıflı inputlara otomatik uygulanır
(function () {
    function applyPhoneMask(input) {
        input.addEventListener('input', function (e) {
            var v = this.value.replace(/\D/g, '');

            // Başındaki 90 veya 0'ı at, 10 haneli core numarayı al
            if (v.startsWith('90')) v = v.slice(2);
            else if (v.startsWith('0')) v = v.slice(1);

            // 10 haneyle sınırla
            if (v.length > 10) v = v.slice(0, 10);

            // Formatla: +90 (xxx) xxx xx xx
            var formatted = '+90 ';
            if (v.length > 0) formatted += '(' + v.slice(0, 3);
            if (v.length >= 3) formatted += ') ';
            if (v.length > 3) formatted += v.slice(3, 6);
            if (v.length >= 6) formatted += ' ';
            if (v.length > 6) formatted += v.slice(6, 8);
            if (v.length >= 8) formatted += ' ';
            if (v.length > 8) formatted += v.slice(8, 10);

            this.value = formatted;
        });

        // Backspace ile silmede düzeltme
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Backspace') {
                var pos = this.selectionStart;
                // Parantez, boşluk gibi formatı koruma karakterini atla
                if (pos > 0 && '() '.includes(this.value[pos - 1])) {
                    e.preventDefault();
                    var newVal = this.value.slice(0, pos - 2) + this.value.slice(pos);
                    this.value = newVal;
                    this.setSelectionRange(pos - 2, pos - 2);
                    this.dispatchEvent(new Event('input'));
                }
            }
        });

        // İlk focus'ta prefix koy
        input.addEventListener('focus', function () {
            if (!this.value) this.value = '+90 (';
        });

        input.addEventListener('blur', function () {
            if (this.value === '+90 (') this.value = '';
        });
    }

    document.querySelectorAll('.phone-mask').forEach(applyPhoneMask);
})();
