// "+ جديد" popups: adds a customer / doctor to the database and puts it straight into the dropdown,
// so the person never has to leave (and lose) the invoice or prescription they are filling in.
//
//   pharmacyQuickAdd('quickCustomerModal', '/Customers/QuickCreate', 'CustomerID');
//
// The popup's <form> must sit OUTSIDE the main form, and the page must contain the main form's
// antiforgery token (any <form method="post" asp-action=...> has one).
function pharmacyQuickAdd(modalId, url, selectId) {
    const modalEl = document.getElementById(modalId);
    if (!modalEl) return;

    const form = modalEl.querySelector('form');
    const errorBox = modalEl.querySelector('.quick-error');
    const saveBtn = form.querySelector('button[type="submit"]');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

    modalEl.addEventListener('shown.bs.modal', function () {
        const first = form.querySelector('input');
        if (first) first.focus();
    });

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        errorBox.classList.add('d-none');
        saveBtn.disabled = true;

        try {
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const data = new FormData(form);
            if (tokenInput) data.append('__RequestVerificationToken', tokenInput.value);

            const response = await fetch(url, { method: 'POST', body: data });
            const result = await response.json().catch(function () { return {}; });

            if (!response.ok) {
                errorBox.textContent = result.message || 'حدث خطأ، حاول مرة أخرى.';
                errorBox.classList.remove('d-none');
                return;
            }

            const select = document.getElementById(selectId);
            select.add(new Option(result.name, result.id, true, true));
            select.dispatchEvent(new Event('change'));

            form.reset();
            modal.hide();
        } catch (err) {
            errorBox.textContent = 'تعذر الاتصال بالخادم، حاول مرة أخرى.';
            errorBox.classList.remove('d-none');
        } finally {
            saveBtn.disabled = false;
        }
    });
}
