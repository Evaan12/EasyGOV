// ===== Project(5)/Web/wwwroot/js/site.js =====
document.addEventListener("DOMContentLoaded", () => {
    if (typeof Lenis !== 'undefined') {
        const lenis = new Lenis({
            duration: 1.2,
            easing: (t) => Math.min(1, 1.001 - Math.pow(2, -10 * t)),
            smooth: true,
        });

        function raf(time) {
            lenis.raf(time);
            requestAnimationFrame(raf);
        }
        requestAnimationFrame(raf);
    }

    const sidebar = document.querySelector('.sidebar');
    const hamburger = document.querySelector('.hamburger-btn');
    const mobileOverlay = document.querySelector('.mobile-overlay');

    if (hamburger) {
        hamburger.addEventListener('click', () => {
            sidebar?.classList.toggle('mobile-open');
            mobileOverlay?.classList.toggle('active');
        });
    }

    if (mobileOverlay) {
        mobileOverlay.addEventListener('click', () => {
            sidebar?.classList.remove('mobile-open');
            mobileOverlay?.classList.remove('active');
        });
    }

    // Generic AJAX Modal Form Handler (Interceptor for seamless validation mapping)
    document.addEventListener('submit', async (e) => {
        const form = e.target;
        if (form.closest('.modal')) {
            e.preventDefault();
            const submitBtn = form.querySelector('button[type="submit"]');
            let originalText = '';
            
            if (submitBtn) {
                originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Processing...';
            }
            
            try {
                const formData = new FormData(form);
                const response = await fetch(form.action, {
                    method: form.method,
                    body: formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                
                if (response.redirected) {
                    window.location.href = response.url;
                    return;
                }
                
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.indexOf("application/json") !== -1) {
                    const result = await response.json();
                    if (result.success) {
                        window.location.reload();
                    } else {
                        let alertBox = form.querySelector('.modal-error-alert');
                        if (!alertBox) {
                            alertBox = document.createElement('div');
                            alertBox.className = 'alert alert-danger small fw-bold modal-error-alert rounded-0 mb-3 shadow-sm';
                            const modalBody = form.querySelector('.modal-body');
                            if (modalBody) modalBody.prepend(alertBox);
                            else form.prepend(alertBox);
                        }
                        alertBox.innerHTML = '<i class="bi bi-exclamation-octagon me-2"></i>' + result.message;
                        if (submitBtn) {
                            submitBtn.disabled = false;
                            submitBtn.innerHTML = originalText;
                        }
                    }
                } else {
                    window.location.reload(); // Fallback
                }
            } catch (err) {
                console.error(err);
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }
            }
        }
    });
});