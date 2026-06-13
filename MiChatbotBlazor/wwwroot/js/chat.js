window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.copyToClipboard = async (text) => {
    try {
        // Primero intentar el método moderno del Clipboard API
        if (navigator.clipboard && window.isSecureContext) {
            // Asegurar que el documento tenga foco
            if (!document.hasFocus()) {
                window.focus();
                await new Promise(resolve => setTimeout(resolve, 100));
            }
            
            await navigator.clipboard.writeText(text);
            console.log('Texto copiado al portapapeles con Clipboard API');
            showCopyNotification('Mensaje copiado al portapapeles');
            return true;
        } else {
            // Método de respaldo usando textarea temporal
            return fallbackCopyTextToClipboard(text);
        }
    } catch (err) {
        console.error('Error al copiar con Clipboard API: ', err);
        // Intentar método de respaldo
        return fallbackCopyTextToClipboard(text);
    }
};

function fallbackCopyTextToClipboard(text) {
    try {
        const textArea = document.createElement("textarea");
        textArea.value = text;
        
        // Evitar que sea visible
        textArea.style.position = "fixed";
        textArea.style.top = "-9999px";
        textArea.style.left = "-9999px";
        textArea.style.width = "2em";
        textArea.style.height = "2em";
        textArea.style.padding = "0";
        textArea.style.border = "none";
        textArea.style.outline = "none";
        textArea.style.boxShadow = "none";
        textArea.style.background = "transparent";
        
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        
        const successful = document.execCommand('copy');
        document.body.removeChild(textArea);
        
        if (successful) {
            console.log('Texto copiado al portapapeles con método de respaldo');
            showCopyNotification('Mensaje copiado al portapapeles');
            return true;
        } else {
            console.error('Falló el comando de copia');
            showCopyNotification('Error al copiar el mensaje', true);
            return false;
        }
    } catch (err) {
        console.error('Error en método de respaldo: ', err);
        showCopyNotification('Error al copiar el mensaje', true);
        return false;
    }
}

function showCopyNotification(message, isError = false) {
    // Crear notificación temporal
    const notification = document.createElement('div');
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${isError ? '#dc3545' : '#28a745'};
        color: white;
        padding: 12px 20px;
        border-radius: 6px;
        font-size: 14px;
        z-index: 10000;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        transition: opacity 0.3s ease;
    `;
    
    document.body.appendChild(notification);
    
    // Remover después de 3 segundos
    setTimeout(() => {
        notification.style.opacity = '0';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};


//SlideBar

(function () {
    'use strict'
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl)
    })
})()