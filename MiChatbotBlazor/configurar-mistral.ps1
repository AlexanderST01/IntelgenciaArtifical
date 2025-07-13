# Script para configurar Mistral API Key con variables de entorno
# Ejecuta este comando en PowerShell ANTES de ejecutar dotnet run

# Reemplaza 'tu-api-key-aqui' con tu API key real
$env:MISTRAL_API_KEY = "tu-api-key-aqui"

# Luego ejecuta el proyecto
dotnet run
