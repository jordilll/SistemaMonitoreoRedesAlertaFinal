// Sidebar toggle
document.getElementById('sidebarToggle')?.addEventListener('click', function () {
    document.getElementById('sidebar').classList.toggle('show');
});

// Auto-dismiss alerts after 4 seconds
document.querySelectorAll('.alert.alert-success, .alert.alert-info').forEach(function (el) {
    setTimeout(function () {
        const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
        bsAlert.close();
    }, 4000);
});
