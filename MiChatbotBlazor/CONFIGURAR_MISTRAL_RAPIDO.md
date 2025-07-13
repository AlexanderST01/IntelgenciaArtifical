# 🚀 CONFIGURACIÓN RÁPIDA - MISTRAL AI

## ✅ PASOS PARA CONFIGURAR:

### 1. Obtener API Key
```
👉 Ve a: https://console.mistral.ai/
👉 Registrate (GRATIS - te dan créditos)
👉 Ir a "API Keys"
👉 Crear nueva API key
👉 Copiar la key (empieza con sk-...)
```

### 2. Configurar en el Proyecto
```
👉 Abrir: appsettings.Development.json
👉 Buscar: "PEGA_TU_API_KEY_AQUI"
👉 Reemplazar con tu API key real
👉 Guardar archivo
```

### 3. Ejecutar Proyecto
```bash
dotnet run
```

### 4. Probar
```
👉 Ir a: http://localhost:5021
👉 Navegar a la página de Chat
👉 Escribir un mensaje
👉 ¡El bot debería responder!
```

---

## 🔧 CONFIGURACIÓN AVANZADA

### Cambiar Modelo (Opcional)
En `appsettings.Development.json`, cambiar:
```json
"Model": "mistral-small-latest"     // Rápido y barato ⚡💰
"Model": "mistral-medium-latest"    // Balance ⚡⚡💰💰
"Model": "mistral-large-latest"     // Mejor calidad ⚡⚡⚡💰💰💰
```

### Usar Variables de Entorno (Más Seguro)
```powershell
# En PowerShell:
$env:MISTRAL_API_KEY = "tu-api-key-aqui"
dotnet run
```

---

## ⚠️ IMPORTANTE
- **NO** subas tu API key a Git/GitHub
- Mistral da créditos gratuitos para probar
- `mistral-small-latest` es el más económico

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### Error: "API Key no válida"
✅ Verificar que la API key esté correcta
✅ Verificar que no tenga espacios extra

### Error: "No connection"
✅ Verificar conexión a internet
✅ Verificar que Mistral.ai esté funcionando

### Bot no responde
✅ Verificar que el proyecto esté ejecutándose
✅ Verificar la consola por errores
✅ Verificar que la API key tenga créditos

---

## 📞 ESTADO ACTUAL DEL PROYECTO
✅ Configuración Mistral: ✅ LISTA
✅ Base de datos SQLite: ✅ FUNCIONANDO  
✅ Componentes Blazor: ✅ FUNCIONANDO
✅ JavaScript Interop: ✅ CORREGIDO
✅ Compilación: ✅ SIN ERRORES

**🎯 SOLO FALTA: Configurar tu API Key de Mistral**
