# Plan de Trabajo - Kanbmine
## Aplicación Kanban con integración Redmine

**Stack:** Blazor Server + .NET 10  
**API:** Redmine REST API

---

## Fase 1: Configuración Inicial del Proyecto

### 1.1 Estructura del Proyecto
- [x] Crear solución .NET 10 con estructura de carpetas:
  - `Kanbmine.Web` (Blazor Server)
  - `Kanbmine.Core` (Lógica de negocio)
  - `Kanbmine.Infrastructure` (Acceso a datos/API)
  - `Kanbmine.Shared` (DTOs y modelos compartidos)
- [x] Configurar `.gitignore` para .NET
- [x] Crear `README.md` con documentación básica
- [x] Configurar archivo de configuración `appsettings.json` con endpoints de Redmine

### 1.2 Dependencias
- [x] Instalar paquetes NuGet necesarios:
  - `Microsoft.AspNetCore.Components.Server` ✓
  - `System.Net.Http.Json` ✓
  - `Microsoft.Extensions.Configuration` ✓
  - `Microsoft.Extensions.Http.Polly` ✓
  - `Blazored.LocalStorage` ✓

---

## Fase 2: Integración con API de Redmine

### 2.1 Cliente HTTP para Redmine
- [x] Analiza la documentación de la API de Redmine #Redmine_API_ENDPOINTS.md
- [x] Crear interfaz `IRedmineApiClient` con métodos:
  - `AuthenticateAsync(username, password)` → Validar credenciales
  - `GetUserAsync(apiKey)` → Obtener datos del usuario
  - `GetIssuesAsync(projectId, filters)` → Obtener tareas
  - `UpdateIssueStatusAsync(issueId, statusId)` → Actualizar estatus
  - `AddCommentAsync(issueId, comment)` → Agregar comentario
- [x] Implementar `RedmineApiClient` con manejo de:
  - Autenticación por API Key
  - Paginación de resultados
  - Manejo de errores HTTP
  - Deserialización de respuestas JSON
- [x] Crear modelos DTO para respuestas de Redmine:
  - `RedmineUser`
  - `RedmineIssue`
  - `RedmineStatus`
  - `RedmineProject`

### 2.2 Servicios de Negocio
- [x] Crear `IAuthenticationService` para:
  - Login con credenciales Redmine
  - Almacenar API Key de forma segura
  - Validar sesión activa
- [x] Crear `IIssueService` para:
  - Mapear issues de Redmine a modelo de tarjetas Kanban
  - Filtrar y agrupar tareas por estado
  - Cachear datos para optimizar llamadas
- [x] Configurar inyección de dependencias en `Program.cs`

---

## Fase 3: Autenticación y Sesión

### 3.1 Sistema de Login
- [x] Crear componente `Login.razor`:
  - Formulario con usuario/contraseña
  - Validación de campos
  - Indicador de carga
  - Manejo de errores de autenticación
- [x] Implementar `AuthStateProvider` personalizado:
  - Mantener estado de autenticación
  - Persistir API Key en LocalStorage
  - Recuperar sesión al recargar página
- [ ] Configurar autorización en rutas con `[Authorize]`

### 3.2 Gestión de Sesión
- [ ] Crear servicio `SessionManager`:
  - Verificar token válido
  - Auto-renovación de sesión
  - Logout y limpieza de datos
- [ ] Implementar interceptor HTTP para agregar API Key en headers
- [ ] Agregar manejo de sesión expirada (redirect a login)

---

## Fase 4: Interfaz Kanban

### 4.1 Componentes Base
- [ ] Crear `KanbanBoard.razor` (contenedor principal):
  - Diseño de columnas horizontales
  - Drag & drop entre columnas
  - Filtros por proyecto/asignado
  - Botón de recarga manual
- [ ] Crear `KanbanColumn.razor`:
  - Título del estado
  - Contador de tarjetas
  - Drop zone para tarjetas
- [ ] Crear `KanbanCard.razor`:
  - ID y título de tarea
  - Asignado y prioridad
  - Etiquetas/tags
  - Click para abrir detalle
  - Draggable habilitado

### 4.2 Drag & Drop
- [ ] Implementar lógica de arrastre:
  - Evento `ondragstart` en tarjeta
  - Evento `ondrop` en columna
  - Validar cambio de estado permitido
  - Actualizar en Redmine via API
  - Actualizar UI optimistamente
  - Rollback si falla actualización

### 4.3 Filtros y Búsqueda
- [ ] Agregar barra de filtros:
  - Por proyecto
  - Por asignado
  - Por prioridad
  - Por fecha de creación
- [ ] Implementar búsqueda por texto en título/descripción
- [ ] Persistir filtros en sesión del usuario

---

## Fase 5: Detalle de Tarjetas

### 5.1 Modal de Detalle
- [ ] Crear componente `CardDetail.razor`:
  - Información completa de la tarea
  - Descripción formateada (Markdown/Textile)
  - Historial de cambios
  - Lista de comentarios
  - Formulario para agregar comentario
- [ ] Implementar apertura de modal al click en tarjeta
- [ ] Agregar botón para abrir en Redmine (link externo)

### 5.2 Comentarios
- [ ] Crear componente `CommentSection.razor`:
  - Lista de comentarios con autor y fecha
  - Editor de texto para nuevo comentario
  - Botón enviar con validación
  - Actualización automática tras agregar
- [ ] Implementar `ICommentService` para gestionar comentarios
- [ ] Agregar notificación de éxito/error

---

## Fase 6: Diseño y UX

### 6.1 Estilos y Layout
- [ ] Configurar CSS/SCSS base:
  - Variables de colores
  - Tipografía
  - Espaciados consistentes
- [ ] Implementar diseño responsive:
  - Vista desktop (múltiples columnas)
  - Vista tablet (2 columnas)
  - Vista móvil (1 columna con tabs)
- [ ] Agregar temas claro/oscuro (opcional)

### 6.2 Componentes de UI
- [ ] Crear componente `NavMenu.razor`:
  - Logo/nombre de app
  - Usuario actual
  - Botón de logout
  - Selector de proyecto
- [ ] Crear componente `LoadingSpinner.razor`
- [ ] Crear componente `ErrorAlert.razor`
- [ ] Crear componente `Toast.razor` para notificaciones

### 6.3 Animaciones
- [ ] Agregar transiciones CSS para:
  - Drag & drop
  - Apertura de modal
  - Aparición de tarjetas
  - Cambio entre estados

---

## Fase 7: Optimización y Cache

### 7.1 Performance
- [ ] Implementar cache de issues con `MemoryCache`:
  - TTL de 2-5 minutos
  - Invalidación manual
  - Refresh en background
- [ ] Optimizar renderizado con `@key` en listas
- [ ] Usar `StateHasChanged()` estratégicamente
- [ ] Implementar paginación/lazy loading para muchas tarjetas

### 7.2 Manejo de Errores
- [ ] Crear middleware de errores global
- [ ] Implementar retry policy para llamadas API
- [ ] Agregar logs estructurados (Serilog)
- [ ] Crear página de error genérica

---

## Fase 8: Testing

### 8.1 Tests Unitarios
- [ ] Crear proyecto `Kanbmine.Tests`
- [ ] Tests para `RedmineApiClient`:
  - Mock de respuestas HTTP
  - Validar deserialización
  - Manejo de errores
- [ ] Tests para servicios de negocio:
  - `AuthenticationService`
  - `IssueService`
- [ ] Configurar cobertura mínima (70%)

### 8.2 Tests de Integración
- [ ] Tests de flujo completo:
  - Login → Obtener issues → Actualizar estado
  - Agregar comentario a tarea
- [ ] Usar TestServer para simular API Redmine
- [ ] Validar manejo de sesión

---

## Fase 9: Documentación

### 9.1 Documentación Técnica
- [ ] Actualizar `README.md` con:
  - Descripción del proyecto
  - Requisitos y dependencias
  - Instrucciones de instalación
  - Configuración de Redmine API
  - Comandos para ejecutar
- [ ] Crear `CONTRIBUTING.md` con guías de desarrollo
- [ ] Documentar arquitectura en `docs/ARCHITECTURE.md`

### 9.2 Documentación de Usuario
- [ ] Crear manual de usuario:
  - Cómo hacer login
  - Uso del tablero Kanban
  - Mover tarjetas
  - Agregar comentarios
- [ ] Screenshots de funcionalidades principales

---

## Fase 10: Deployment

### 10.1 Preparación
- [ ] Configurar variables de entorno para producción
- [ ] Crear `Dockerfile` para containerización
- [ ] Configurar CI/CD (GitHub Actions):
  - Build automático
  - Ejecución de tests
  - Deploy a staging/producción
- [ ] Configurar HTTPS y certificados

### 10.2 Monitoreo
- [ ] Implementar health checks
- [ ] Configurar Application Insights o similar
- [ ] Agregar métricas de uso
- [ ] Dashboard de monitoreo

---

## Notas Importantes

### Configuración de Redmine
```json
{
  "Redmine": {
    "BaseUrl": "https://redmine.ejemplo.com",
    "ApiPath": "/api",
    "DefaultPageSize": 50,
    "CacheExpirationMinutes": 5
  }
}
```

### Endpoints Principales de Redmine
- `GET /users/current.json` - Usuario actual
- `GET /issues.json` - Listar tareas
- `PUT /issues/{id}.json` - Actualizar tarea
- `POST /issues/{id}.json` - Agregar comentario (via notes)

### Consideraciones de Seguridad
- Nunca exponer API Keys en código fuente
- Usar HTTPS para todas las comunicaciones
- Validar entrada de usuario
- Sanitizar comentarios antes de guardar
- Implementar rate limiting

### Mejoras Futuras
- WebSocket para actualizaciones en tiempo real
- Notificaciones push
- Exportar tablero a imagen
- Estadísticas y reportes
- Múltiples proyectos simultáneos
