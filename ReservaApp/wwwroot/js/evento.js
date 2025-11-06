/* ============================================ */
/* EVENTOS - JAVASCRIPT PRINCIPAL */
/* ============================================ */

document.addEventListener('DOMContentLoaded', function () {
    initializeEventCards();
    initializeSearchFilter();
    initializeAnimations();
});

/* ============================================ */
/* INICIALIZAR TARJETAS DE EVENTOS */
/* ============================================ */
function initializeEventCards() {
    const cards = document.querySelectorAll('.evento-card');

    cards.forEach((card, index) => {
        // Animación de entrada escalonada
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);

        // Efecto de hover mejorado
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-10px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
}

/* ============================================ */
/* FILTRO DE BÚSQUEDA */
/* ============================================ */
function initializeSearchFilter() {
    // Crear barra de búsqueda si no existe
    const eventosHeader = document.querySelector('.eventos-header');
    if (eventosHeader && !document.getElementById('searchInput')) {
        const searchContainer = document.createElement('div');
        searchContainer.style.width = '100%';
        searchContainer.style.marginTop = '1rem';
        searchContainer.innerHTML = `
            <div style="position: relative; max-width: 500px;">
                <i class="fas fa-search" style="position: absolute; left: 1rem; top: 50%; transform: translateY(-50%); color: #9ca3af;"></i>
                <input type="text" id="searchInput" placeholder="Buscar eventos..." 
                       style="width: 100%; padding: 0.75rem 1rem 0.75rem 2.5rem; border: 2px solid #e5e7eb; border-radius: 0.5rem; font-size: 1rem;">
            </div>
        `;
        eventosHeader.appendChild(searchContainer);

        // Funcionalidad de búsqueda
        const searchInput = document.getElementById('searchInput');
        searchInput.addEventListener('input', function (e) {
            const searchTerm = e.target.value.toLowerCase();
            const cards = document.querySelectorAll('.evento-card');

            cards.forEach(card => {
                const title = card.querySelector('.event-title').textContent.toLowerCase();
                const description = card.querySelector('.event-description')?.textContent.toLowerCase() || '';
                const lugar = card.querySelector('.info-item span')?.textContent.toLowerCase() || '';

                if (title.includes(searchTerm) || description.includes(searchTerm) || lugar.includes(searchTerm)) {
                    card.style.display = 'block';
                    card.style.animation = 'fadeIn 0.3s ease';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }
}

/* ============================================ */
/* ANIMACIONES */
/* ============================================ */
function initializeAnimations() {
    // Animación para alertas
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        alert.style.animation = 'slideInDown 0.5s ease';

        // Auto-cerrar después de 5 segundos
        setTimeout(() => {
            if (alert.querySelector('.btn-close')) {
                alert.style.animation = 'slideOutUp 0.5s ease';
                setTimeout(() => alert.remove(), 500);
            }
        }, 5000);
    });
}

/* ============================================ */
/* CONFIRMAR ELIMINACIÓN */
/* ============================================ */
function confirmDelete(id, nombre) {
    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
    document.getElementById('eventoNombre').textContent = nombre;
    document.getElementById('confirmDeleteBtn').onclick = () => deleteEvento(id);
    modal.show();
}

/* ============================================ */
/* ELIMINAR EVENTO */
/* ============================================ */
async function deleteEvento(id) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    try {
        const response = await fetch(`/Eventos/Delete/${id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        const data = await response.json();

        if (data.success) {
            showNotification(data.message, 'success');

            // Animar y remover la tarjeta
            const card = document.querySelector(`[data-evento-id="${id}"]`) ||
                document.querySelector('.evento-card');

            if (card) {
                card.style.animation = 'fadeOut 0.5s ease';
                setTimeout(() => {
                    card.remove();
                    checkEmptyState();
                }, 500);
            } else {
                setTimeout(() => location.reload(), 1500);
            }
        } else {
            showNotification(data.message, 'error');
        }

        // Cerrar modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('deleteModal'));
        modal.hide();

    } catch (error) {
        console.error('Error:', error);
        showNotification('Error al eliminar el evento', 'error');
    }
}

/* ============================================ */
/* TOGGLE ESTADO ACTIVO */
/* ============================================ */
async function toggleActivo(id, currentState) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    try {
        const response = await fetch(`/Eventos/ToggleActivo/${id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        const data = await response.json();

        if (data.success) {
            const message = data.activo ? 'Evento activado exitosamente' : 'Evento desactivado exitosamente';
            showNotification(message, 'success');

            // Recargar después de un breve delay
            setTimeout(() => location.reload(), 1000);
        } else {
            showNotification(data.message, 'error');
        }

    } catch (error) {
        console.error('Error:', error);
        showNotification('Error al cambiar el estado del evento', 'error');
    }
}

/* ============================================ */
/* MOSTRAR NOTIFICACIÓN */
/* ============================================ */
function showNotification(message, type = 'info') {
    const alertClass = type === 'success' ? 'alert-success' :
        type === 'error' ? 'alert-danger' :
            type === 'warning' ? 'alert-warning' : 'alert-info';

    const icon = type === 'success' ? 'fa-check-circle' :
        type === 'error' ? 'fa-exclamation-circle' :
            type === 'warning' ? 'fa-exclamation-triangle' : 'fa-info-circle';

    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show notification-toast`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 10px 25px rgba(0,0,0,0.1);
        animation: slideInRight 0.3s ease;
    `;

    notification.innerHTML = `
        <i class="fas ${icon}"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    // Auto-remover después de 3 segundos
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

/* ============================================ */
/* VERIFICAR ESTADO VACÍO */
/* ============================================ */
function checkEmptyState() {
    const grid = document.querySelector('.eventos-grid');
    const cards = grid ? grid.querySelectorAll('.evento-card') : [];

    if (cards.length === 0) {
        const container = document.querySelector('.eventos-container');
        const emptyState = `
            <div class="empty-state" style="animation: fadeIn 0.5s ease;">
                <i class="fas fa-calendar-times"></i>
                <h3>No hay eventos creados</h3>
                <p>Comienza creando tu primer evento</p>
                <a href="/Eventos/Create" class="btn btn-primary">
                    <i class="fas fa-plus-circle"></i> Crear Primer Evento
                </a>
            </div>
        `;

        if (grid) {
            grid.remove();
        }

        container.insertAdjacentHTML('beforeend', emptyState);
    }
}

/* ============================================ */
/* ANIMACIONES CSS ADICIONALES */
/* ============================================ */
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
    }
    
    @keyframes fadeOut {
        from { opacity: 1; transform: scale(1); }
        to { opacity: 0; transform: scale(0.8); }
    }
    
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOutRight {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
    
    @keyframes slideInDown {
        from { transform: translateY(-100%); opacity: 0; }
        to { transform: translateY(0); opacity: 1; }
    }
    
    @keyframes slideOutUp {
        from { transform: translateY(0); opacity: 1; }
        to { transform: translateY(-100%); opacity: 0; }
    }
`;
document.head.appendChild(style);

/* ============================================ */
/* EXPORTAR FUNCIONES GLOBALES */
/* ============================================ */
window.confirmDelete = confirmDelete;
window.deleteEvento = deleteEvento;
window.toggleActivo = toggleActivo;
window.showNotification = showNotification;