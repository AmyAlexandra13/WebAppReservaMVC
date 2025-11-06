/* ============================================ */
/* ARCHIVO: wwwroot/js/auth.js */
/* Sistema de Reservas - JavaScript de Autenticación */
/* ============================================ */

// Esperar a que el DOM esté completamente cargado
document.addEventListener('DOMContentLoaded', function () {
    initializePasswordToggles();
    initializePasswordStrength();
    initializeFormAnimations();
    initializeFieldValidation();
    initializeParallaxEffect();
});

/* ============================================ */
/* TOGGLE DE VISIBILIDAD DE CONTRASEÑA */
/* ============================================ */
function initializePasswordToggles() {
    // Toggle para el campo de contraseña principal
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('passwordInput');
    const eyeIcon = document.getElementById('eyeIcon');

    if (togglePassword && passwordInput && eyeIcon) {
        togglePassword.addEventListener('click', function (e) {
            e.preventDefault();
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);

            if (type === 'text') {
                eyeIcon.classList.remove('fa-eye');
                eyeIcon.classList.add('fa-eye-slash');
            } else {
                eyeIcon.classList.remove('fa-eye-slash');
                eyeIcon.classList.add('fa-eye');
            }
        });
    }

    // Toggle para confirmar contraseña
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');
    const confirmPasswordInput = document.getElementById('confirmPasswordInput');
    const eyeIconConfirm = document.getElementById('eyeIconConfirm');

    if (toggleConfirmPassword && confirmPasswordInput && eyeIconConfirm) {
        toggleConfirmPassword.addEventListener('click', function (e) {
            e.preventDefault();
            const type = confirmPasswordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            confirmPasswordInput.setAttribute('type', type);

            if (type === 'text') {
                eyeIconConfirm.classList.remove('fa-eye');
                eyeIconConfirm.classList.add('fa-eye-slash');
            } else {
                eyeIconConfirm.classList.remove('fa-eye-slash');
                eyeIconConfirm.classList.add('fa-eye');
            }
        });
    }
}

/* ============================================ */
/* INDICADOR DE FORTALEZA DE CONTRASEÑA */
/* ============================================ */
function initializePasswordStrength() {
    const passwordInput = document.getElementById('passwordInput');

    if (passwordInput && document.getElementById('strengthBarFill')) {
        passwordInput.addEventListener('input', function () {
            checkPasswordStrength(this.value);
        });
    }
}

function checkPasswordStrength(password) {
    const strengthBar = document.getElementById('strengthBarFill');
    const strengthText = document.getElementById('strengthText');

    if (!strengthBar || !strengthText) return;

    let strength = 0;

    if (password.length === 0) {
        strengthBar.style.width = '0%';
        strengthBar.className = 'strength-bar-fill';
        strengthText.textContent = 'Ingresa una contraseña';
        strengthText.style.color = '#6c757d';
        return;
    }

    // Criterios de fortaleza
    if (password.length >= 6) strength++;
    if (password.length >= 10) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z\d]/.test(password)) strength++;

    // Actualizar UI
    strengthBar.classList.remove('weak', 'medium', 'strong');

    if (strength <= 2) {
        strengthBar.classList.add('weak');
        strengthText.textContent = 'Contraseña débil';
        strengthText.style.color = '#f44336';
    } else if (strength <= 4) {
        strengthBar.classList.add('medium');
        strengthText.textContent = 'Contraseña media';
        strengthText.style.color = '#ff9800';
    } else {
        strengthBar.classList.add('strong');
        strengthText.textContent = 'Contraseña fuerte';
        strengthText.style.color = '#4CAF50';
    }
}

/* ============================================ */
/* ANIMACIONES DE ENTRADA DEL FORMULARIO */
/* ============================================ */
function initializeFormAnimations() {
    const formGroups = document.querySelectorAll('.form-group');
    formGroups.forEach((group, index) => {
        group.style.opacity = '0';
        group.style.animation = `fadeIn 0.5s ease-out ${index * 0.1}s forwards`;
    });
}

/* ============================================ */
/* VALIDACIÓN DE CAMPOS EN TIEMPO REAL */
/* ============================================ */
function initializeFieldValidation() {
    const forms = document.querySelectorAll('.auth-form');

    forms.forEach(form => {
        const inputs = form.querySelectorAll('.form-control');

        inputs.forEach(input => {
            // Validar cuando el campo pierde el foco
            input.addEventListener('blur', function () {
                validateField(this);
            });

            // Revalidar mientras se escribe si ya hay un error
            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid')) {
                    validateField(this);
                }
            });
        });

        // Validación final antes de enviar
        form.addEventListener('submit', function (e) {
            let isValid = true;

            inputs.forEach(input => {
                if (!validateField(input)) {
                    isValid = false;
                }
            });

            if (!isValid) {
                e.preventDefault();
            }
        });
    });
}

function validateField(field) {
    const value = field.value.trim();
    const fieldName = field.name || field.id;

    // Limpiar clases previas
    field.classList.remove('is-invalid', 'is-valid');

    // Validaciones específicas
    if (fieldName === 'Email' || field.type === 'email') {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (value && !emailRegex.test(value)) {
            field.classList.add('is-invalid');
            return false;
        }
    }

    if (fieldName === 'Password' && value && value.length < 6) {
        field.classList.add('is-invalid');
        return false;
    }

    if (fieldName === 'ConfirmPassword') {
        const passwordField = document.getElementById('passwordInput');
        if (passwordField && value !== passwordField.value) {
            field.classList.add('is-invalid');
            return false;
        }
    }

    if (field.hasAttribute('required') && !value) {
        field.classList.add('is-invalid');
        return false;
    }

    if (value.length > 0) {
        field.classList.add('is-valid');
    }

    return true;
}

/* ============================================ */
/* EFECTO PARALLAX EN EL FONDO */
/* ============================================ */
function initializeParallaxEffect() {
    let mouseX = 0;
    let mouseY = 0;
    let targetX = 0;
    let targetY = 0;

    document.addEventListener('mousemove', function (e) {
        mouseX = e.clientX / window.innerWidth;
        mouseY = e.clientY / window.innerHeight;
    });

    function animate() {
        // Interpolación suave
        targetX += (mouseX - targetX) * 0.05;
        targetY += (mouseY - targetY) * 0.05;

        const shapes = document.querySelectorAll('.shape');
        shapes.forEach((shape, index) => {
            const speed = (index + 1) * 15;
            const x = (targetX * speed) - (speed / 2);
            const y = (targetY * speed) - (speed / 2);

            shape.style.transform = `translate(${x}px, ${y}px)`;
        });

        requestAnimationFrame(animate);
    }

    animate();
}

/* ============================================ */
/* PREVENIR ZOOM EN INPUTS EN MÓVILES */
/* ============================================ */
if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
    const inputs = document.querySelectorAll('input[type="text"], input[type="email"], input[type="password"]');
    inputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.style.fontSize = '16px';
        });
    });
}

/* ============================================ */
/* ANIMACIÓN DE CARGA EN BOTONES */
/* ============================================ */
document.querySelectorAll('.btn-primary').forEach(button => {
    button.addEventListener('click', function (e) {
        const form = this.closest('form');

        if (form && form.checkValidity()) {
            this.disabled = true;
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';

            // Restaurar el botón después de 3 segundos si algo sale mal
            setTimeout(() => {
                if (this.disabled) {
                    this.disabled = false;
                    const originalIcon = this.getAttribute('data-original-icon') || 'fa-sign-in-alt';
                    const originalText = this.getAttribute('data-original-text') || 'Enviar';
                    this.innerHTML = `<i class="fas ${originalIcon}"></i> ${originalText}`;
                }
            }, 3000);
        }
    });
});

/* ============================================ */
/* ANIMACIÓN DE ENTRADA DE ALERTAS */
/* ============================================ */
const alerts = document.querySelectorAll('.alert');
alerts.forEach(alert => {
    alert.style.opacity = '0';
    alert.style.transform = 'translateY(-20px)';

    setTimeout(() => {
        alert.style.transition = 'all 0.3s ease';
        alert.style.opacity = '1';
        alert.style.transform = 'translateY(0)';
    }, 100);
});

/* ============================================ */
/* FUNCIÓN PARA MOSTRAR NOTIFICACIONES */
/* ============================================ */
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type}`;
    notification.textContent = message;
    notification.style.position = 'fixed';
    notification.style.top = '20px';
    notification.style.right = '20px';
    notification.style.zIndex = '9999';
    notification.style.minWidth = '250px';
    notification.style.animation = 'slideInRight 0.5s ease';

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.5s ease';
        setTimeout(() => {
            notification.remove();
        }, 500);
    }, 3000);
}

// Agregar estilos de animación para notificaciones
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);