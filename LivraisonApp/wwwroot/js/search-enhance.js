/* =================================================================
   LivraisonApp — Search Enhancements
   - Wire custom search bars to DataTables instances
   - Auto-submit filter forms with debounce
   - Toggle clear buttons live result counters
   ================================================================= */
(function () {
    'use strict';

    function debounce(fn, wait) {
        var t;
        return function () {
            var ctx = this, args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(ctx, args); }, wait);
        };
    }

    function formatNumber(n) {
        return new Intl.NumberFormat('fr-FR').format(n);
    }

    /* ---------- Custom search bar bound to a DataTable ---------- */
    function initTableSearch(input) {
        var selector = input.getAttribute('data-table-search');
        if (!selector) return;
        var table = document.querySelector(selector);
        if (!table || !window.jQuery || !jQuery.fn || !jQuery.fn.dataTable) return;

        var dt = jQuery(table).DataTable();
        var wrap = input.closest('.search-toolbar') || input.parentElement;
        var clearBtn = wrap ? wrap.querySelector('.search-clear') : null;
        var countNum = wrap ? wrap.querySelector('.search-count-num') : null;
        var totalRows = dt.rows().count();

        function updateCount() {
            if (!countNum) return;
            var n = dt.rows({ search: 'applied' }).count();
            countNum.textContent = formatNumber(n);
            var label = wrap ? wrap.querySelector('.search-count') : null;
            if (label) {
                label.title = n + ' résultat(s) sur ' + totalRows;
            }
        }
        updateCount();
        dt.on('draw', updateCount);

        var doSearch = debounce(function (val) {
            dt.search(val).draw();
        }, 180);

        input.addEventListener('input', function () {
            var v = input.value;
            doSearch(v);
            if (clearBtn) clearBtn.hidden = !v;
        });

        if (clearBtn) {
            clearBtn.addEventListener('click', function () {
                input.value = '';
                input.focus();
                clearBtn.hidden = true;
                dt.search('').draw();
            });
        }

        // Keyboard shortcut: "/" focuses the search
        document.addEventListener('keydown', function (e) {
            if (e.key === '/' && document.activeElement &&
                !/^(INPUT|TEXTAREA|SELECT)$/.test(document.activeElement.tagName)) {
                e.preventDefault();
                input.focus();
                input.select();
            }
        });
    }

    /* ---------- Auto-submit filter forms ---------- */
    function initAutoSubmit(form) {
        var submit = debounce(function () { form.submit(); }, 450);
        form.querySelectorAll('input, select').forEach(function (el) {
            if (el.type === 'submit' || el.type === 'button') return;
            var evt = (el.tagName === 'SELECT' || el.type === 'date') ? 'change' : 'input';
            el.addEventListener(evt, submit);
        });
    }

    /* ---------- Collapsible filter card ---------- */
    function initFilterToggle(header) {
        var body = header.nextElementSibling;
        if (!body) return;
        header.setAttribute('aria-expanded', 'true');
        header.addEventListener('click', function () {
            var collapsed = body.classList.toggle('collapsed');
            header.setAttribute('aria-expanded', String(!collapsed));
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-table-search]').forEach(initTableSearch);
        document.querySelectorAll('form[data-auto-submit]').forEach(initAutoSubmit);
        document.querySelectorAll('.filter-card-header[data-toggle="filter"]').forEach(initFilterToggle);
    });
})();
