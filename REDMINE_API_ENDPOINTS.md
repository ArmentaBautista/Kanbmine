# Documentación de Endpoints - Redmine REST API
## Para aplicación Kanbmine

---

## Tabla de Contenidos
1. [Autenticación](#autenticación)
2. [Usuarios (Users)](#usuarios-users)
3. [Proyectos (Projects)](#proyectos-projects)
4. [Issues (Tareas)](#issues-tareas)
5. [Estados de Issues (Issue Statuses)](#estados-de-issues-issue-statuses)
6. [Versiones (Versions)](#versiones-versions)
7. [Entradas de Tiempo (Time Entries)](#entradas-de-tiempo-time-entries)
8. [Archivos Adjuntos (Attachments)](#archivos-adjuntos-attachments)
9. [Paginación y Filtros](#paginación-y-filtros)
10. [Manejo de Errores](#manejo-de-errores)

---

## Autenticación

### Configuración Previa
La API REST debe estar habilitada en Redmine:
- Ir a: **Administración → Configuración → API**
- Marcar: **Habilitar servicio REST**

### Flujo de Autenticación para Kanbmine

#### Proceso de Login con Usuario y Password

Redmine utiliza **HTTP Basic Authentication** para validar credenciales de usuario. El flujo recomendado es:

1. **Usuario ingresa credenciales** (username y password)
2. **Validar credenciales** haciendo una petición a `/users/current.json` con HTTP Basic Auth
3. **Obtener API Key** del usuario autenticado
4. **Guardar API Key** para todas las peticiones subsecuentes

#### 1. Login Inicial - HTTP Basic Authentication

**Endpoint de Validación:** `GET /users/current.json`

**Headers:**
```http
Authorization: Basic BASE64(username:password)
```

**Ejemplo de Request:**
```http
GET /users/current.json HTTP/1.1
Host: redmine.example.com
Authorization: Basic am9obi5zbWl0aDpteXBhc3N3b3Jk
```

**Cálculo del Header Authorization:**
```
username = "john.smith"
password = "mypassword"
credentials = "john.smith:mypassword"
base64_credentials = Base64.encode(credentials)  // am9obi5zbWl0aDpteXBhc3N3b3Jk
header = "Authorization: Basic am9obi5zbWl0aDpteXBhc3N3b3Jk"
```

**Respuesta Exitosa (200 OK):**
```json
{
  "user": {
    "id": 3,
    "login": "john.smith",
    "firstname": "John",
    "lastname": "Smith",
    "mail": "john.smith@example.com",
    "api_key": "ebc3f6b781a6fb3f2b0a83ce0ebb80e0d585189d",
    "created_on": "2023-01-15T10:30:00Z",
    "last_login_on": "2026-01-28T08:15:30Z",
    "status": 1
  }
}
```

**Respuesta Error (401 Unauthorized):**
```json
{
  "errors": ["Invalid username or password"]
}
```

#### 2. Uso del API Key para Peticiones Subsecuentes

Una vez obtenido el **API Key**, úsalo en todas las peticiones posteriores:

**a) Como Header HTTP (Recomendado):**
```http
GET /issues.json
Headers:
  X-Redmine-API-Key: ebc3f6b781a6fb3f2b0a83ce0ebb80e0d585189d
```

**b) Como parámetro en URL:**
```http
GET /issues.json?key=ebc3f6b781a6fb3f2b0a83ce0ebb80e0d585189d
```

**c) Como usuario en HTTP Basic Auth:**
```http
GET /issues.json
Headers:
  Authorization: Basic BASE64(api_key:anypassword)
```

#### Resumen del Flujo

```
┌─────────────────────────────────────────────────────────────┐
│  1. Usuario ingresa username y password en formulario       │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  2. POST /users/current.json                                │
│     Authorization: Basic BASE64(username:password)          │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │   ¿Credenciales       │
              │    válidas?           │
              └─────┬─────────┬───────┘
                    │         │
              SI    │         │  NO
                    │         │
                    ▼         ▼
         ┌──────────────┐   ┌─────────────────┐
         │ 200 OK       │   │ 401 Unauthorized│
         │ + user.api_key│   │ Credenciales    │
         └──────┬───────┘   │ incorrectas     │
                │           └─────────────────┘
                ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Guardar API Key en LocalStorage/Cookie/Sesión          │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Todas las peticiones posteriores usan:                  │
│     X-Redmine-API-Key: {api_key}                            │
└─────────────────────────────────────────────────────────────┘
```

### Content-Type Requerido
Para peticiones POST/PUT siempre especificar:
```http
Content-Type: application/json   # Para JSON
Content-Type: application/xml    # Para XML
```

---

## Usuarios (Users)

### Obtener Usuario Actual (Endpoint de Login)
**Endpoint:** `GET /users/current.json`

**Descripción:** Retorna la información del usuario autenticado. **Este es el endpoint principal para el login con usuario y password.**

**Autenticación con Usuario y Password:**
```http
GET /users/current.json HTTP/1.1
Host: redmine.example.com
Authorization: Basic BASE64(username:password)
```

**Autenticación con API Key (peticiones posteriores):**
```http
GET /users/current.json HTTP/1.1
Host: redmine.example.com
X-Redmine-API-Key: ebc3f6b781a6fb3f2b0a83ce0ebb80e0d585189d
```

**Respuesta Exitosa (200 OK):**
```json
{
  "user": {
    "id": 3,
    "login": "jsmith",
    "firstname": "John",
    "lastname": "Smith",
    "mail": "john@example.com",
    "created_on": "2007-09-28T00:16:04+02:00",
    "updated_on": "2010-08-01T18:05:45+02:00",
    "last_login_on": "2026-01-28T08:15:30+02:00",
    "api_key": "ebc3f6b781a6fb3f2b0a83ce0ebb80e0d585189d",
    "status": 1
  }
}
```

**Campos Importantes:**
- `id`: Identificador único del usuario
- `login`: Nombre de usuario
- `api_key`: **Clave para todas las peticiones posteriores**
- `status`: 1 = Activo, 2 = Registrado, 3 = Bloqueado

**Respuestas de Error:**
- `401 Unauthorized`: Credenciales incorrectas o API Key inválida
- `403 Forbidden`: Usuario bloqueado

**Uso en Kanbmine:** 
- **Validar credenciales** en el formulario de login
- **Obtener API Key** para usar en todas las peticiones subsecuentes
- **Verificar sesión activa** al recargar la aplicación

---

### Obtener Usuario por ID
**Endpoint:** `GET /users/{id}.json`

**Parámetros URL:**
- `id`: ID del usuario o `current` para usuario actual
- `include` (opcional): `memberships`, `groups`

**Ejemplo:**
```http
GET /users/3.json?include=memberships,groups
X-Redmine-API-Key: YOUR_API_KEY
```

**Respuesta:**
```json
{
  "user": {
    "id": 3,
    "login": "jsmith",
    "firstname": "John",
    "lastname": "Smith",
    "mail": "john@example.com",
    "api_key": "...",
    "status": 1,
    "memberships": [
      {
        "project": { "id": 1, "name": "Proyecto A" },
        "roles": [
          { "id": 3, "name": "Developer" }
        ]
      }
    ],
    "groups": [
      { "id": 20, "name": "Developers" }
    ]
  }
}
```

---

### Listar Usuarios
**Endpoint:** `GET /users.json`

**Parámetros Query:**
- `status`: `1` (activo), `2` (registrado), `3` (bloqueado), vacío (todos)
- `name`: Filtrar por login, firstname, lastname o mail
- `group_id`: Filtrar por grupo
- `limit`: Cantidad de resultados (default: 25, max: 100)
- `offset`: Saltar N resultados

**Ejemplo:**
```http
GET /users.json?status=1&limit=50
```

**⚠️ Requiere privilegios de administrador**

---

## Proyectos (Projects)

### Listar Proyectos
**Endpoint:** `GET /projects.json`

**Descripción:** Retorna todos los proyectos públicos y privados a los que el usuario tiene acceso.

**Parámetros Query:**
- `include`: Datos asociados separados por coma
  - `trackers`: Tipos de issues
  - `issue_categories`: Categorías
  - `enabled_modules`: Módulos activos (desde 2.6.0)
  - `time_entry_activities`: Actividades de tiempo (desde 3.4.0)
- `limit`, `offset`: Paginación

**Ejemplo:**
```http
GET /projects.json?include=trackers,issue_categories&limit=50
X-Redmine-API-Key: YOUR_API_KEY
```

**Respuesta (200 OK):**
```json
{
  "projects": [
    {
      "id": 1,
      "name": "Proyecto Web",
      "identifier": "proyecto-web",
      "description": "Desarrollo del sitio web corporativo",
      "status": 1,
      "is_public": true,
      "created_on": "2023-01-15T10:30:00Z",
      "updated_on": "2024-05-20T14:20:00Z"
    }
  ],
  "total_count": 15,
  "limit": 50,
  "offset": 0
}
```

**Uso en Kanbmine:** Listar proyectos disponibles para filtrar el tablero Kanban.

---

### Obtener Proyecto Específico
**Endpoint:** `GET /projects/{id}.json`

**Parámetros URL:**
- `id`: ID numérico o identificador de texto del proyecto
- `include`: `trackers`, `issue_categories`, `enabled_modules`

**Ejemplo:**
```http
GET /projects/proyecto-web.json?include=trackers,issue_categories
GET /projects/1.json
```

**Respuesta:**
```json
{
  "project": {
    "id": 1,
    "name": "Proyecto Web",
    "identifier": "proyecto-web",
    "description": "...",
    "homepage": "",
    "status": 1,
    "is_public": true,
    "parent": { "id": 5, "name": "Proyecto Padre" },
    "trackers": [
      { "id": 1, "name": "Bug" },
      { "id": 2, "name": "Feature" }
    ],
    "issue_categories": [
      { "id": 1, "name": "Backend" },
      { "id": 2, "name": "Frontend" }
    ]
  }
}
```

---

## Issues (Tareas)

### Listar Issues
**Endpoint:** `GET /issues.json`

**Descripción:** Retorna una lista paginada de issues. Por defecto solo issues abiertos.

**Parámetros Query Principales:**

| Parámetro | Descripción | Ejemplo |
|-----------|-------------|---------|
| `issue_id` | ID específico o múltiples separados por coma | `1` o `1,2,3` |
| `project_id` | Filtrar por proyecto (ID numérico) | `2` |
| `subproject_id` | Incluir/excluir subproyectos | `!*` (sin subproyectos) |
| `tracker_id` | Tipo de issue | `1` |
| `status_id` | Estado: `open`, `closed`, `*` (todos), o ID | `open` |
| `assigned_to_id` | Asignado a usuario (ID o `me`) | `me` |
| `parent_id` | Issue padre | `123` |
| `cf_x` | Custom field con ID x | `cf_1=valor` |
| `created_on` | Fecha de creación (codificada) | Ver ejemplos |
| `updated_on` | Fecha de actualización | Ver ejemplos |
| `sort` | Ordenar por columna (`:desc` invierte) | `updated_on:desc` |
| `limit` | Resultados por página (max 100) | `50` |
| `offset` | Saltar N resultados | `100` |
| `include` | Datos relacionados: `attachments`, `relations` | `attachments` |

**Filtros de Fecha (requieren URL encoding):**
```http
# Rango de fechas (entre): ><2012-03-01|2012-03-07
GET /issues.json?created_on=%3E%3C2012-03-01|2012-03-07

# Mayor o igual a: >=2012-03-01
GET /issues.json?created_on=%3E%3D2012-03-01

# Menor o igual a: <=2012-03-07
GET /issues.json?created_on=%3C%3D2012-03-07

# Con timestamp: >=2014-01-02T08:12:32Z
GET /issues.json?updated_on=%3E%3D2014-01-02T08:12:32Z

# Custom field contiene texto: ~foo
GET /issues.json?cf_4=~foo
```

**Ejemplos de Uso:**
```http
# Todos los issues del proyecto 2
GET /issues.json?project_id=2

# Issues abiertos asignados a mí
GET /issues.json?assigned_to_id=me&status_id=open

# Issues cerrados del proyecto 2, tracker 1
GET /issues.json?project_id=2&tracker_id=1&status_id=closed

# Todos los issues (abiertos y cerrados)
GET /issues.json?status_id=*

# Ordenados por última actualización
GET /issues.json?sort=updated_on:desc&limit=100
```

**Respuesta (200 OK):**
```json
{
  "issues": [
    {
      "id": 4326,
      "project": { "id": 1, "name": "Proyecto A" },
      "tracker": { "id": 2, "name": "Feature" },
      "status": { "id": 1, "name": "New" },
      "priority": { "id": 4, "name": "Normal" },
      "author": { "id": 10, "name": "John Smith" },
      "assigned_to": { "id": 5, "name": "Jane Doe" },
      "subject": "Implementar sistema de login",
      "description": "Desarrollar autenticación con OAuth",
      "start_date": "2024-01-15",
      "due_date": "2024-02-28",
      "done_ratio": 30,
      "estimated_hours": 16.0,
      "spent_hours": 5.5,
      "custom_fields": [
        { "id": 1, "name": "Sprint", "value": "Sprint 5" }
      ],
      "created_on": "2024-01-10T15:30:00Z",
      "updated_on": "2024-01-20T10:15:00Z",
      "closed_on": null
    }
  ],
  "total_count": 245,
  "limit": 25,
  "offset": 0
}
```

**Uso en Kanbmine:** Cargar todas las tarjetas del tablero Kanban filtradas por proyecto y estado.

---

### Obtener Issue Específico
**Endpoint:** `GET /issues/{id}.json`

**Parámetros Query:**
- `include`: Datos relacionados separados por coma
  - `children`: Issues hijos
  - `attachments`: Archivos adjuntos
  - `relations`: Relaciones con otros issues
  - `changesets`: Commits relacionados
  - `journals`: Historial de cambios y comentarios
  - `watchers`: Observadores (desde 2.3.0)
  - `allowed_statuses`: Estados permitidos según workflow (desde 5.0)

**Ejemplo:**
```http
GET /issues/2.json?include=journals,attachments,watchers
X-Redmine-API-Key: YOUR_API_KEY
```

**Respuesta:**
```json
{
  "issue": {
    "id": 2,
    "project": { "id": 1, "name": "Proyecto A" },
    "tracker": { "id": 1, "name": "Bug" },
    "status": { "id": 2, "name": "In Progress" },
    "priority": { "id": 5, "name": "High" },
    "author": { "id": 10, "name": "John Smith" },
    "assigned_to": { "id": 5, "name": "Jane Doe" },
    "subject": "Error en login",
    "description": "Los usuarios no pueden iniciar sesión...",
    "start_date": "2024-01-15",
    "due_date": null,
    "done_ratio": 50,
    "estimated_hours": 4.0,
    "spent_hours": 2.5,
    "journals": [
      {
        "id": 101,
        "user": { "id": 10, "name": "John Smith" },
        "notes": "Investigando la causa del problema",
        "created_on": "2024-01-16T09:30:00Z",
        "details": [
          {
            "property": "attr",
            "name": "status_id",
            "old_value": "1",
            "new_value": "2"
          }
        ]
      }
    ],
    "attachments": [
      {
        "id": 50,
        "filename": "screenshot.png",
        "filesize": 45678,
        "content_type": "image/png",
        "author": { "id": 10, "name": "John Smith" },
        "created_on": "2024-01-16T10:00:00Z",
        "content_url": "https://redmine.example.com/attachments/download/50/screenshot.png"
      }
    ],
    "watchers": [
      { "id": 10, "name": "John Smith" },
      { "id": 15, "name": "Bob Johnson" }
    ],
    "allowed_statuses": [
      { "id": 3, "name": "Resolved" },
      { "id": 5, "name": "Closed" }
    ]
  }
}
```

**Uso en Kanbmine:** 
- Mostrar detalle completo al hacer clic en una tarjeta
- Obtener historial (journals) para mostrar comentarios
- Ver archivos adjuntos
- Conocer estados permitidos para el drag & drop

---

### Crear Issue
**Endpoint:** `POST /issues.json`

**Headers:**
```http
Content-Type: application/json
X-Redmine-API-Key: YOUR_API_KEY
```

**Body (campos principales):**
```json
{
  "issue": {
    "project_id": 1,
    "tracker_id": 2,
    "status_id": 1,
    "priority_id": 4,
    "subject": "Nueva funcionalidad",
    "description": "Descripción detallada...",
    "category_id": 3,
    "fixed_version_id": 5,
    "assigned_to_id": 10,
    "parent_issue_id": 100,
    "start_date": "2024-02-01",
    "due_date": "2024-02-28",
    "estimated_hours": 8.0,
    "done_ratio": 0,
    "is_private": false,
    "watcher_user_ids": [10, 15],
    "custom_fields": [
      { "id": 1, "value": "Sprint 5" }
    ],
    "uploads": [
      {
        "token": "7167.ed1ccdb093229ca1bd0b043618d88743",
        "filename": "document.pdf",
        "description": "Documentación técnica",
        "content_type": "application/pdf"
      }
    ]
  }
}
```

**Respuestas:**
- `201 Created`: Issue creado exitosamente
- `422 Unprocessable Entity`: Error de validación (ver cuerpo de respuesta)

**Uso en Kanbmine:** Crear nuevas tareas desde el tablero Kanban.

---

### Actualizar Issue
**Endpoint:** `PUT /issues/{id}.json`

**Headers:**
```http
Content-Type: application/json
X-Redmine-API-Key: YOUR_API_KEY
```

**Body:**
```json
{
  "issue": {
    "status_id": 3,
    "assigned_to_id": 15,
    "done_ratio": 75,
    "notes": "Comentario sobre la actualización",
    "private_notes": false,
    "custom_fields": [
      { "id": 1, "value": "Sprint 6" }
    ]
  }
}
```

**Parámetros Importantes:**
- `notes`: Agregar un comentario al actualizar
- `private_notes`: `true` para comentarios privados
- Puedes actualizar cualquier campo del issue

**Respuestas:**
- `204 No Content`: Actualización exitosa
- `422 Unprocessable Entity`: Error de validación

**Uso en Kanbmine:** 
- **Drag & Drop:** Actualizar `status_id` al arrastrar tarjeta entre columnas
- Agregar comentarios desde el modal de detalle
- Cambiar asignación, prioridad, etc.

---

### Eliminar Issue
**Endpoint:** `DELETE /issues/{id}.json`

**Respuesta:**
- `204 No Content`: Issue eliminado

---

### Agregar Observador (Watcher)
**Endpoint:** `POST /issues/{id}/watchers.json`

**Body:**
```json
{
  "user_id": 10
}
```

**Disponible desde:** Redmine 2.3.0

---

### Eliminar Observador
**Endpoint:** `DELETE /issues/{id}/watchers/{user_id}.json`

**Disponible desde:** Redmine 2.3.0

---

## Estados de Issues (Issue Statuses)

### Listar Estados
**Endpoint:** `GET /issue_statuses.json`

**Descripción:** Retorna todos los estados de issues disponibles en Redmine.

**Ejemplo:**
```http
GET /issue_statuses.json
X-Redmine-API-Key: YOUR_API_KEY
```

**Respuesta (200 OK):**
```json
{
  "issue_statuses": [
    {
      "id": 1,
      "name": "New",
      "is_closed": false
    },
    {
      "id": 2,
      "name": "In Progress",
      "is_closed": false
    },
    {
      "id": 3,
      "name": "Resolved",
      "is_closed": false
    },
    {
      "id": 4,
      "name": "Feedback",
      "is_closed": false
    },
    {
      "id": 5,
      "name": "Closed",
      "is_closed": true
    },
    {
      "id": 6,
      "name": "Rejected",
      "is_closed": true
    }
  ]
}
```

**Uso en Kanbmine:** 
- **Crear columnas del tablero Kanban** basadas en estados
- Filtrar estados abiertos (`is_closed: false`) para mostrar en el board
- Mapear estados a columnas personalizadas

---

## Versiones (Versions)

### Listar Versiones de un Proyecto
**Endpoint:** `GET /projects/{project_id}/versions.json`

**Ejemplo:**
```http
GET /projects/1/versions.json
GET /projects/proyecto-web/versions.json
```

**Respuesta:**
```json
{
  "versions": [
    {
      "id": 1,
      "project": { "id": 1, "name": "Proyecto A" },
      "name": "v1.0",
      "description": "Primera versión estable",
      "status": "open",
      "due_date": "2024-06-30",
      "sharing": "none",
      "created_on": "2024-01-01T10:00:00Z",
      "updated_on": "2024-01-15T14:30:00Z"
    },
    {
      "id": 2,
      "project": { "id": 1, "name": "Proyecto A" },
      "name": "v1.1",
      "status": "locked",
      "due_date": "2024-09-30",
      "sharing": "descendants"
    }
  ],
  "total_count": 5
}
```

**Campos:**
- `status`: `open`, `locked`, `closed`
- `sharing`: `none`, `descendants`, `hierarchy`, `tree`, `system`

**Uso en Kanbmine:** Filtrar issues por versión/milestone.

---

### Obtener Versión Específica
**Endpoint:** `GET /versions/{id}.json`

**Respuesta:**
```json
{
  "version": {
    "id": 2,
    "project": { "id": 1, "name": "Proyecto A" },
    "name": "v1.1",
    "description": "...",
    "status": "open",
    "due_date": "2024-09-30",
    "estimated_hours": 120.0,
    "spent_hours": 45.5,
    "created_on": "2024-02-01T10:00:00Z",
    "updated_on": "2024-03-15T16:20:00Z"
  }
}
```

---

### Crear Versión
**Endpoint:** `POST /projects/{project_id}/versions.json`

**Body:**
```json
{
  "version": {
    "name": "v2.0",
    "status": "open",
    "sharing": "none",
    "due_date": "2024-12-31",
    "description": "Nueva versión mayor",
    "wiki_page_title": "Version_2_0"
  }
}
```

**Respuestas:**
- `201 Created`: Versión creada
- `422 Unprocessable Entity`: Error de validación

---

### Actualizar Versión
**Endpoint:** `PUT /versions/{id}.json`

**Respuesta:**
- `204 No Content`: Actualizada exitosamente

---

### Eliminar Versión
**Endpoint:** `DELETE /versions/{id}.json`

**Respuesta:**
- `204 No Content`: Eliminada exitosamente

---

## Entradas de Tiempo (Time Entries)

### Listar Entradas de Tiempo
**Endpoint:** `GET /time_entries.json`

**Parámetros Query:**
- `offset`, `limit`: Paginación
- `user_id`: Filtrar por usuario
- `project_id`: Por proyecto (ID o identificador)
- `spent_on`: Por fecha
- `from`, `to`: Rango de fechas

**Ejemplo:**
```http
GET /time_entries.json?project_id=1&from=2024-01-01&to=2024-01-31&limit=100
```

**Respuesta:**
```json
{
  "time_entries": [
    {
      "id": 10,
      "project": { "id": 1, "name": "Proyecto A" },
      "issue": { "id": 50 },
      "user": { "id": 5, "name": "Jane Doe" },
      "activity": { "id": 9, "name": "Development" },
      "hours": 3.5,
      "comments": "Implementación de login",
      "spent_on": "2024-01-15",
      "created_on": "2024-01-15T17:30:00Z",
      "updated_on": "2024-01-15T17:30:00Z"
    }
  ],
  "total_count": 45,
  "limit": 25,
  "offset": 0
}
```

---

### Obtener Entrada de Tiempo
**Endpoint:** `GET /time_entries/{id}.json`

---

### Crear Entrada de Tiempo
**Endpoint:** `POST /time_entries.json`

**Body:**
```json
{
  "time_entry": {
    "issue_id": 50,
    "spent_on": "2024-01-20",
    "hours": 4.0,
    "activity_id": 9,
    "comments": "Corrección de bugs en login"
  }
}
```

**Campos:**
- `issue_id` o `project_id`: Uno es requerido
- `spent_on`: Fecha (formato: `YYYY-MM-DD`, default: hoy)
- `hours`: Horas gastadas (requerido)
- `activity_id`: ID de actividad (requerido si no hay default)
- `comments`: Descripción (máx 255 caracteres)
- `user_id`: Para registrar en nombre de otro usuario

**Respuestas:**
- `201 Created`: Entrada creada
- `422 Unprocessable Entity`: Error de validación

---

### Actualizar Entrada de Tiempo
**Endpoint:** `PUT /time_entries/{id}.json`

**Respuesta:**
- `204 No Content`: Actualizada exitosamente

---

### Eliminar Entrada de Tiempo
**Endpoint:** `DELETE /time_entries/{id}.json`

---

## Archivos Adjuntos (Attachments)

### Subir Archivo (Paso 1)
**Endpoint:** `POST /uploads.json`

**Headers:**
```http
Content-Type: application/octet-stream
X-Redmine-API-Key: YOUR_API_KEY
```

**URL Parameters:**
```http
POST /uploads.json?filename=documento.pdf
```

**Body:** Contenido binario del archivo

**Respuesta (201 Created):**
```json
{
  "upload": {
    "token": "7167.ed1ccdb093229ca1bd0b043618d88743"
  }
}
```

**Respuesta (422 Error - archivo muy grande):**
```json
{
  "errors": [
    "This file cannot be uploaded because it exceeds the maximum allowed file size (1024000)"
  ]
}
```

---

### Adjuntar Archivo a Issue (Paso 2)
Usa el token obtenido al crear/actualizar un issue:

**POST /issues.json:**
```json
{
  "issue": {
    "project_id": 1,
    "subject": "Issue con adjunto",
    "uploads": [
      {
        "token": "7167.ed1ccdb093229ca1bd0b043618d88743",
        "filename": "documento.pdf",
        "description": "Especificación técnica",
        "content_type": "application/pdf"
      }
    ]
  }
}
```

**PUT /issues/{id}.json:**
```json
{
  "issue": {
    "notes": "Adjuntando documentación",
    "uploads": [
      {
        "token": "7167.ed1ccdb093229ca1bd0b043618d88743",
        "filename": "documento.pdf",
        "content_type": "application/pdf"
      }
    ]
  }
}
```

**Múltiples Archivos:**
```json
{
  "issue": {
    "subject": "Issue con múltiples adjuntos",
    "uploads": [
      {
        "token": "7167.ed1ccdb093229ca1bd0b043618d88743",
        "filename": "image1.png",
        "content_type": "image/png"
      },
      {
        "token": "7168.d595398bbb104ed3bba0eed666785cc6",
        "filename": "image2.png",
        "content_type": "image/png"
      }
    ]
  }
}
```

---

### Obtener Archivo Adjunto
**Endpoint:** `GET /attachments/{id}.json`

**Respuesta:**
```json
{
  "attachment": {
    "id": 50,
    "filename": "screenshot.png",
    "filesize": 45678,
    "content_type": "image/png",
    "description": "Error en la pantalla principal",
    "author": { "id": 10, "name": "John Smith" },
    "created_on": "2024-01-16T10:00:00Z",
    "content_url": "https://redmine.example.com/attachments/download/50/screenshot.png"
  }
}
```

---

## Paginación y Filtros

### Estructura de Paginación

Todos los endpoints de colecciones retornan metadatos:

```json
{
  "issues": [...],
  "total_count": 2595,
  "limit": 25,
  "offset": 0
}
```

**Parámetros:**
- `limit`: Cantidad por página (default: 25, **máximo: 100**)
- `offset`: Saltar N elementos

**Ejemplos:**
```http
# Primera página (25 elementos)
GET /issues.json

# Segunda página (25 elementos)
GET /issues.json?offset=25&limit=25

# Primera página con 100 elementos
GET /issues.json?limit=100

# Elementos 30-40
GET /issues.json?offset=30&limit=10
```

---

### Eliminar Metadatos (nometa)

Si el cliente REST no soporta metadatos a nivel raíz:

```http
GET /issues.json?nometa=1
```

**O usar header:**
```http
X-Redmine-Nometa: 1
```

**Respuesta sin metadatos:**
```json
{
  "issues": [...]
}
```

---

### Incluir Datos Asociados (include)

Usa el parámetro `include` con valores separados por coma:

```http
GET /issues/2.json?include=journals,attachments,watchers
GET /projects/1.json?include=trackers,issue_categories
GET /users/current.json?include=memberships,groups
```

---

## Manejo de Errores

### Códigos de Estado HTTP

| Código | Descripción | Cuándo ocurre |
|--------|-------------|---------------|
| `200 OK` | Petición exitosa | GET exitoso |
| `201 Created` | Recurso creado | POST exitoso |
| `204 No Content` | Sin contenido | PUT/DELETE exitoso |
| `401 Unauthorized` | No autenticado | API Key inválida o faltante |
| `403 Forbidden` | Sin permisos | Requiere permisos admin |
| `404 Not Found` | No encontrado | ID inexistente |
| `412 Precondition Failed` | Precondición fallida | User impersonation con usuario inválido |
| `422 Unprocessable Entity` | Error de validación | Datos inválidos en POST/PUT |

---

### Respuestas de Error (422)

**XML:**
```xml
<errors type="array">
  <error>First name can't be blank</error>
  <error>Email is invalid</error>
</errors>
```

**JSON:**
```json
{
  "errors": [
    "First name can't be blank",
    "Email is invalid"
  ]
}
```

---

### Custom Fields

#### Leer Custom Fields

**GET /issues/296.json:**
```json
{
  "issue": {
    "id": 296,
    "custom_fields": [
      { "id": 1, "name": "Affected version", "value": "1.0.1" },
      { "id": 2, "name": "Resolution", "value": "Fixed" }
    ]
  }
}
```

#### Actualizar Custom Fields

**PUT /issues/296.json:**
```json
{
  "issue": {
    "subject": "Actualizar custom fields",
    "custom_fields": [
      { "id": 1, "value": "1.0.2" },
      { "id": 2, "value": "Invalid" }
    ]
  }
}
```

#### Custom Fields Múltiples (Multiselect)

**Formato desde Redmine 1.4.0:**
```json
{
  "issue": {
    "custom_fields": [
      {
        "id": 1,
        "name": "Affected version",
        "value": ["1.0.1", "1.0.2"],
        "multiple": true
      }
    ]
  }
}
```

---

## Resumen de Endpoints para Kanbmine

### Autenticación y Sesión
| Método | Endpoint | Uso | Auth |
|--------|----------|-----|------|
| `GET` | `/users/current.json` | **Login:** Validar username/password y obtener API Key | HTTP Basic Auth |
| `GET` | `/users/current.json` | Verificar sesión activa | API Key |
| `GET` | `/users/{id}.json?include=memberships` | Obtener proyectos del usuario | API Key |

### Tablero Kanban
| Método | Endpoint | Uso |
|--------|----------|-----|
| `GET` | `/projects.json` | Listar proyectos disponibles |
| `GET` | `/issue_statuses.json` | Obtener estados para columnas |
| `GET` | `/issues.json?project_id={id}&status_id=open` | Cargar issues del board |
| `PUT` | `/issues/{id}.json` | Actualizar estado (drag & drop) |

### Detalle de Tarjeta
| Método | Endpoint | Uso |
|--------|----------|-----|
| `GET` | `/issues/{id}.json?include=journals,attachments` | Ver detalle completo |
| `PUT` | `/issues/{id}.json` | Agregar comentario (`notes`) |
| `POST` | `/uploads.json` | Subir archivo |
| `PUT` | `/issues/{id}.json` | Adjuntar archivo con token |

### Filtros y Búsqueda
| Método | Endpoint | Uso |
|--------|----------|-----|
| `GET` | `/issues.json?assigned_to_id=me` | Issues asignados a mí |
| `GET` | `/issues.json?updated_on=%3E%3D{date}` | Issues actualizados desde fecha |
| `GET` | `/issues.json?sort=updated_on:desc` | Ordenar por actualización |

---

## Notas de Implementación

### HttpClient en .NET

#### Login con Usuario y Password
```csharp
public async Task<(bool success, string apiKey, RedmineUser user)> LoginAsync(
    string username, string password)
{
    var client = new HttpClient
    {
        BaseAddress = new Uri("https://redmine.example.com")
    };
    
    // Crear credenciales Base64
    var credentials = Convert.ToBase64String(
        Encoding.ASCII.GetBytes($"{username}:{password}"));
    
    // Agregar header de autenticación
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Basic", credentials);
    
    try
    {
        // Validar credenciales llamando a /users/current.json
        var response = await client.GetAsync("/users/current.json");
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (false, null, null);
        }
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UserResponse>(json);
        
        // Retornar API Key para futuras peticiones
        return (true, result.User.ApiKey, result.User);
    }
    catch (HttpRequestException)
    {
        return (false, null, null);
    }
}

// Modelo de respuesta
public class UserResponse
{
    [JsonPropertyName("user")]
    public RedmineUser User { get; set; }
}

public class RedmineUser
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; }
    
    [JsonPropertyName("lastname")]
    public string Lastname { get; set; }
    
    [JsonPropertyName("mail")]
    public string Mail { get; set; }
    
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
}
```

#### Peticiones Subsecuentes con API Key
```csharp
// Configurar HttpClient con API Key obtenida del login
var client = new HttpClient
{
    BaseAddress = new Uri("https://redmine.example.com")
};
client.DefaultRequestHeaders.Add("X-Redmine-API-Key", apiKey);

// GET
var response = await client.GetAsync("/issues.json?project_id=1");
var json = await response.Content.ReadAsStringAsync();

// POST/PUT - Especificar Content-Type
var issueData = new { issue = new { subject = "Nueva tarea" } };
var jsonContent = JsonSerializer.Serialize(issueData);
var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
var response = await client.PostAsync("/issues.json", content);
```

### Manejo de Errores
```csharp
if (!response.IsSuccessStatusCode)
{
    if (response.StatusCode == HttpStatusCode.Unauthorized)
        // API Key inválida
    if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        // Parsear errores de validación
}
```

### Persistencia de Sesión
```csharp
// Guardar API Key en LocalStorage (Blazor)
await localStorage.SetItemAsync("redmine_api_key", apiKey);
await localStorage.SetItemAsync("redmine_user", JsonSerializer.Serialize(user));

// Recuperar sesión al recargar
var apiKey = await localStorage.GetItemAsync<string>("redmine_api_key");
if (!string.IsNullOrEmpty(apiKey))
{
    // Validar que la API Key siga siendo válida
    var isValid = await ValidateApiKeyAsync(apiKey);
    if (isValid)
    {
        // Restaurar sesión
        var userJson = await localStorage.GetItemAsync<string>("redmine_user");
        var user = JsonSerializer.Deserialize<RedmineUser>(userJson);
    }
    else
    {
        // API Key expirada o inválida, logout
        await localStorage.RemoveItemAsync("redmine_api_key");
        await localStorage.RemoveItemAsync("redmine_user");
    }
}

// Validar API Key
public async Task<bool> ValidateApiKeyAsync(string apiKey)
{
    var client = new HttpClient { BaseAddress = new Uri(redmineUrl) };
    client.DefaultRequestHeaders.Add("X-Redmine-API-Key", apiKey);
    
    var response = await client.GetAsync("/users/current.json");
    return response.IsSuccessStatusCode;
}
```

### Cache
- Cachear issues con TTL de 2-5 minutos
- Invalidar cache al actualizar un issue
- Cachear issue_statuses (rara vez cambian)
- Cachear proyectos del usuario
- **NO cachear API Key** - usar almacenamiento seguro

### Optimización Drag & Drop
1. Actualizar UI optimistamente
2. Llamar `PUT /issues/{id}.json` con nuevo `status_id`
3. Si falla, revertir UI y mostrar error
4. Agregar `notes` opcional con texto del cambio

---

## Referencias
- [Redmine REST API Documentation](https://www.redmine.org/projects/redmine/wiki/Rest_api)
- [Rest Issues](https://www.redmine.org/projects/redmine/wiki/Rest_Issues)
- [Rest Projects](https://www.redmine.org/projects/redmine/wiki/Rest_Projects)
- [Rest Users](https://www.redmine.org/projects/redmine/wiki/Rest_Users)
- [Rest TimeEntries](https://www.redmine.org/projects/redmine/wiki/Rest_TimeEntries)
- [Rest IssueStatuses](https://www.redmine.org/projects/redmine/wiki/Rest_IssueStatuses)
- [Rest Versions](https://www.redmine.org/projects/redmine/wiki/Rest_Versions)
