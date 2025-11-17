/* ============================================ */
/* ARCHIVO: wwwroot/js/auth.js */
/* Sistema de Reservas - JavaScript de Autenticación */
/* ============================================ */

// Esperar a que el DOM esté completamente cargado
document.addEventListener('DOMContentLoaded', function () {
    initializePasswordToggles();
    initializePasswordStrength();
    initializeFormAnimations();
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
            e.stopPropagation();

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
            e.stopPropagation();

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
        strengthBar.style.width = '33%';
        strengthText.textContent = 'Contraseña débil';
        strengthText.style.color = '#f44336';
    } else if (strength <= 4) {
        strengthBar.classList.add('medium');
        strengthBar.style.width = '66%';
        strengthText.textContent = 'Contraseña media';
        strengthText.style.color = '#ff9800';
    } else {
        strengthBar.classList.add('strong');
        strengthBar.style.width = '100%';
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
/* ANIMACIÓN DE ENTRADA DE ALERTAS */
/* ============================================ */
setTimeout(function () {
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
}, 100);

/* ============================================ */
/* ESTILOS DE ANIMACIÓN */
/* ============================================ */
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
`;
document.head.appendChild(style);