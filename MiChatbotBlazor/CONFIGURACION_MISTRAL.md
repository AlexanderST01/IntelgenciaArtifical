# 🔑 Configuración Rápida de Mistral AI

## Paso 1: Obtener API Key

1. Ve a: https://console.mistral.ai/
2. Crea una cuenta (gratis)
3. Ve a "API Keys" en el dashboard
4. Genera una nueva API key
5. Copia la key (empieza con algo como: `sk-...`)

## Paso 2: Configurar en el Proyecto

Reemplaza `REEMPLAZA_CON_TU_MISTRAL_API_KEY` en estos archivos:

### appsettings.json
```json
"Mistral": {
  "ApiKey": "tu-api-key-aqui",
  ...
}
```

### appsettings.Development.json
```json
"Mistral": {
  "ApiKey": "tu-api-key-aqui",
  ...
}
```

## Paso 3: Ejecutar

```bash
dotnet run
```

## ⚠️ Importante

- **NO** subas tu API key a repositorios públicos
- Mistral ofrece créditos gratuitos para pruebas
- Los modelos `mistral-small-latest` son los más económicos

## 🚀 Modelos Disponibles

| Modelo | Descripción | Velocidad | Costo |
|--------|-------------|-----------|-------|
| `mistral-small-latest` | Rápido y económico | ⚡⚡⚡ | 💰 |
| `mistral-medium-latest` | Balance velocidad/calidad | ⚡⚡ | 💰💰 |
| `mistral-large-latest` | Máxima calidad | ⚡ | 💰💰💰 |

## 🔄 Cambiar Modelo

En `appsettings.json`:
```json
"Mistral": {
  "Model": "mistral-medium-latest"  // Cambiar aquí
}
```

---

**¿Problemas?** Consulta el README.md para soluciones detalladas.
