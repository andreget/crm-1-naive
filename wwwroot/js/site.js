// Submit the filter form as soon as the status changes
const statusSelect = document.querySelector(".filters #status");

if (statusSelect) {
    statusSelect.addEventListener("change", () => statusSelect.form.submit());
}
