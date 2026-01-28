# Kanbmine

**Aplicación Kanban integrada con Redmine**

## 📋 Descripción

Kanbmine es una aplicación web tipo Kanban que consume la API REST de Redmine para:
- Autenticación de usuarios
- Visualización de tareas en tablero Kanban
- Actualización de estados mediante drag & drop
- Gestión de comentarios

## 🛠️ Stack Tecnológico

- **Frontend/Backend:** Blazor Server
- **Framework:** .NET 10
- **API:** Redmine REST API

## 🏗️ Estructura del Proyecto

```
Kanbmine/
├── Kanbmine.Web/              # Aplicación Blazor Server
├── Kanbmine.Core/             # Lógica de negocio
├── Kanbmine.Infrastructure/   # Acceso a datos y API
└── Kanbmine.Shared/           # DTOs y modelos compartidos
```

## ⚙️ Configuración

### Requisitos Previos

- .NET 10 SDK
- Redmine con API REST habilitada
- Visual Studio 2022, VS Code o Rider

### Configuración de Redmine

1. Habilitar API REST en Redmine:
   - Ir a: **Administración → Configuración → API**
   - Marcar: **Habilitar servicio REST**

2. Configurar `appsettings.json`:
```json
{
  "Redmine": {
    "BaseUrl": "https://tu-redmine.com",
    "Timeout": 30,
    "CacheDurationMinutes": 5,
    "PageSize": 100
  }
}
```

### Instalación

```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/Kanbmine.git
cd Kanbmine

# Restaurar paquetes
dotnet restore

# Compilar solución
dotnet build

# Ejecutar aplicación
dotnet run --project Kanbmine.Web
```

La aplicación estará disponible en `https://localhost:5001`

## 🚀 Uso

1. **Login:** Ingresar con credenciales de Redmine
2. **Seleccionar Proyecto:** Elegir proyecto en el selector
3. **Tablero Kanban:** Visualizar tareas organizadas por estado
4. **Drag & Drop:** Arrastrar tarjetas entre columnas para cambiar estado
5. **Detalle:** Click en tarjeta para ver información completa y agregar comentarios

## 📚 Documentación

- [Plan de Trabajo](PLAN_TRABAJO.md)
- [Endpoints API Redmine](REDMINE_API_ENDPOINTS.md)
- [Especificación de Implementación](docs/API_IMPLEMENTATION_SPEC.md)
- [Arquitectura](docs/ARCHITECTURE.md) _(próximamente)_

## 🧪 Testing

```bash
# Ejecutar tests unitarios
dotnet test

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true
```

## 📦 Despliegue

### Docker

```bash
# Construir imagen
docker build -t kanbmine .

# Ejecutar contenedor
docker run -p 5000:8080 -e Redmine__BaseUrl=https://redmine.com kanbmine
```

### Azure App Service / IIS

Ver [guía de despliegue](docs/DEPLOYMENT.md) _(próximamente)_

## 🤝 Contribuir

Ver [CONTRIBUTING.md](CONTRIBUTING.md) _(próximamente)_

## 📄 Licencia

Este proyecto es de código cerrado.

## 👨‍💻 Autor

Desarrollado por el equipo de Kanbmine

## 🔗 Enlaces

- [Redmine API Documentation](https://www.redmine.org/projects/redmine/wiki/Rest_api)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)

---

**Estado del Proyecto:** 🚧 En Desarrollo
