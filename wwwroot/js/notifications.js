// Toast Notification System
class ToastNotification {
    constructor() {
        this.container = this.createContainer();
        document.body.appendChild(this.container);
    }

    createContainer() {
        const container = document.createElement('div');
        container.className = 'toast-container';
        return container;
    }

    show(message, title = '', type = 'info', duration = 3000) {
        const toast = this.createToast(message, title, type);
        this.container.appendChild(toast);

        // Auto remove after duration
        if (duration > 0) {
            setTimeout(() => {
                this.remove(toast);
            }, duration);
        }

        return toast;
    }

    createToast(message, title, type) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;

        const iconMap = {
            success: 'fa-check',
            error: 'fa-times',
            warning: 'fa-exclamation',
            info: 'fa-info'
        };

        const icon = iconMap[type] || iconMap.info;

        toast.innerHTML = `
            <div class="toast-icon">
                <i class="fas ${icon}"></i>
            </div>
            <div class="toast-content">
                ${title ? `<div class="toast-title">${title}</div>` : ''}
                <div class="toast-message">${message}</div>
            </div>
            <button class="toast-close" type="button">
                <i class="fas fa-times"></i>
            </button>
            <div class="toast-progress">
                <div class="toast-progress-bar"></div>
            </div>
        `;

        const closeButton = toast.querySelector('.toast-close');
        closeButton.addEventListener('click', () => {
            this.remove(toast);
        });

        return toast;
    }

    remove(toast) {
        toast.classList.add('toast-hiding');
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }

    success(message, title = 'Success') {
        return this.show(message, title, 'success');
    }

    error(message, title = 'Error') {
        return this.show(message, title, 'error');
    }

    warning(message, title = 'Warning') {
        return this.show(message, title, 'warning');
    }

    info(message, title = 'Info') {
        return this.show(message, title, 'info');
    }
}

// Initialize global toast instance
window.toast = new ToastNotification();

// Button Loading State Helper
function setButtonLoading(button, loading = true) {
    if (loading) {
        button.classList.add('btn-loading');
        button.disabled = true;
    } else {
        button.classList.remove('btn-loading');
        button.disabled = false;
    }
}

// Page Loading Overlay
function showPageLoading(text = 'Loading...') {
    let overlay = document.getElementById('pageLoadingOverlay');
    
    if (!overlay) {
        overlay = document.createElement('div');
        overlay.id = 'pageLoadingOverlay';
        overlay.className = 'page-loading-overlay';
        overlay.innerHTML = `
            <div class="page-loading-content">
                <div class="page-loading-spinner"></div>
                <div class="page-loading-text">${text}</div>
            </div>
        `;
        document.body.appendChild(overlay);
    } else {
        overlay.querySelector('.page-loading-text').textContent = text;
        overlay.style.display = 'flex';
    }
    
    return overlay;
}

function hidePageLoading() {
    const overlay = document.getElementById('pageLoadingOverlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
}

// Handle TempData messages on page load
document.addEventListener('DOMContentLoaded', function() {
    // Check for success message
    const successMessage = document.querySelector('[data-temp-success]');
    if (successMessage) {
        const message = successMessage.getAttribute('data-temp-success');
        window.toast.success(message);
    }
    
    // Check for error message
    const errorMessage = document.querySelector('[data-temp-error]');
    if (errorMessage) {
        const message = errorMessage.getAttribute('data-temp-error');
        window.toast.error(message);
    }
    
    // Check for warning message
    const warningMessage = document.querySelector('[data-temp-warning]');
    if (warningMessage) {
        const message = warningMessage.getAttribute('data-temp-warning');
        window.toast.warning(message);
    }
    
    // Check for info message
    const infoMessage = document.querySelector('[data-temp-info]');
    if (infoMessage) {
        const message = infoMessage.getAttribute('data-temp-info');
        window.toast.info(message);
    }
});

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ToastNotification, setButtonLoading, showPageLoading, hidePageLoading };
}
