# 🤖 Mi Chatbot Blazor con Mistral AI

Este proyecto es un chatbot moderno construido con **Blazor Server** e integrado con **Mistral AI**, siguiendo las instrucciones del documento DOCUMENTACION_CHATBOT_BLAZOR.md.

## 🚀 Características Implementadas

- ✅ **Interfaz de chat moderna** con Bootstrap y Font Awesome
- ✅ **Integración con Mistral AI** para respuestas inteligentes
- ✅ **Base de datos SQLite** con Entity Framework Core
- ✅ **Componentes Blazor reutilizables**
- ✅ **Diseño responsivo** para móviles y escritorio
- ✅ **Historial de conversaciones**
- ✅ **Indicador de escritura** animado

## 🛠️ Configuración

### 1. Configurar API Key de Mistral

Para usar Mistral AI, necesitas obtener una API key:

1. Ve a [https://console.mistral.ai/](https://console.mistral.ai/)
2. Crea una cuenta y inicia sesión
3. Ve a la sección "API Keys"
4. Genera una nueva API key

### 2. Configurar appsettings.json

Actualiza el archivo `appsettings.json` con tu API key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=chatbot.db"
  },
  "AI": {
    "Provider": "mistral"
  },
  "Mistral": {
    "ApiKey": "tu-mistral-api-key-aqui",
    "ApiUrl": "https://api.mistral.ai/v1/chat/completions",
    "Model": "mistral-small-latest"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Modelos disponibles de Mistral:**
- `mistral-small-latest` - Modelo rápido y económico
- `mistral-medium-latest` - Balance entre velocidad y calidad
- `mistral-large-latest` - Máxima calidad y capacidades

### 3. Configuración alternativa con variables de entorno

Para mayor seguridad, puedes usar variables de entorno:

**Windows PowerShell:**
```powershell
$env:Mistral__ApiKey = "tu-mistral-api-key-aqui"
```

**Windows Command Prompt:**
```cmd
set Mistral__ApiKey=tu-mistral-api-key-aqui
```

**Linux/macOS:**
```bash
export Mistral__ApiKey="tu-mistral-api-key-aqui"
```

## 🏃‍♂️ Ejecutar el Proyecto

1. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```

2. **Aplicar migraciones de base de datos:**
   ```bash
   dotnet ef database update
   ```

3. **Ejecutar el proyecto:**
   ```bash
   dotnet run
   ```

4. **Abrir en navegador:**
   - Ir a: `http://localhost:5021`
   - Navegar a la página `/chat`

## 📁 Estructura del Proyecto

```
MiChatbotBlazor/
├── Components/
│   └── Chat/
│       ├── ChatComponent.razor      # Componente principal del chat
│       ├── MessageItem.razor        # Componente de mensaje individual
│       ├── ChatInput.razor          # Input de mensaje
│       └── LoadingIndicator.razor   # Indicador de carga
├── Data/
│   ├── ApplicationDbContext.cs      # Contexto de Entity Framework
│   └── Models/
│       ├── ChatMessage.cs           # Modelo de mensaje
│       └── ChatSession.cs           # Modelo de sesión
├── Services/
│   ├── IAIService.cs               # Interfaz del servicio de IA
│   ├── MistralService.cs           # Implementación para Mistral AI
│   ├── IChatService.cs             # Interfaz del servicio de chat
│   └── ChatService.cs              # Implementación del servicio de chat
├── Pages/
│   └── Chat.razor                  # Página principal del chat
└── wwwroot/
    ├── css/
    │   └── chat.css                # Estilos personalizados
    ├── js/
    │   └── chat.js                 # JavaScript helpers
    └── images/                     # Avatares e iconos
```

## 🎨 Personalización

### Cambiar el modelo de Mistral

En `appsettings.json`, modifica el valor de `Model`:

```json
"Mistral": {
  "Model": "mistral-medium-latest"  // Cambiar aquí
}
```

### Personalizar el comportamiento del bot

En `MistralService.cs`, modifica el prompt del sistema:

```csharp
var messages = new List<object>
{
    new { role = "system", content = "Tu prompt personalizado aquí" }
};
```

### Cambiar el estilo visual

Modifica los archivos CSS en:
- `wwwroot/css/chat.css` - Estilos del chat
- `wwwroot/css/site.css` - Estilos generales

## 🔧 Comandos Útiles

```bash
# Limpiar y reconstruir
dotnet clean && dotnet build

# Ejecutar con hot reload
dotnet watch run

# Crear nueva migración
dotnet ef migrations add NombreMigracion

# Revertir última migración
dotnet ef migrations remove

# Ver el estado de la base de datos
dotnet ef database update
```

## 🐛 Solución de Problemas

### Error: "Mistral:ApiKey no encontrada"
- Asegúrate de que la API key esté configurada en `appsettings.json`
- Verifica que no haya espacios extra en la API key

### Error de conexión a Mistral
- Verifica tu conexión a internet
- Confirma que tu API key sea válida
- Revisa que tengas créditos en tu cuenta de Mistral

### Base de datos no se crea
- Ejecuta: `dotnet ef database update`
- Verifica que SQLite esté instalado

## 📚 Recursos Adicionales

- [Documentación de Mistral AI](https://docs.mistral.ai/)
- [Documentación de Blazor](https://docs.microsoft.com/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

## 🎯 Próximos Pasos

- [ ] Agregar autenticación de usuarios
- [ ] Implementar sesiones múltiples
- [ ] Agregar soporte para archivos
- [ ] Implementar modo de voz
- [ ] Desplegar en Azure/AWS

---

¡Disfruta construyendo con tu chatbot Blazor y Mistral AI! 🚀
