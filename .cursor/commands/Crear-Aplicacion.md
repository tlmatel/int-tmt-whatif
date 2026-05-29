# Prompt – Aplicación ASP.NET Core MVC (C#) con PostgreSQL y tabla dummy

Eres un ingeniero senior en ASP.NET Core. Quiero que crees una aplicación **ASP.NET Core MVC (C#)** desde cero, con estas condiciones:

---

## 0. Objetivo

Crear un **esqueleto sólido y simple** de aplicación MVC en C# con PostgreSQL, migraciones automáticas bien controladas y una única tabla dummy para validar toda la infraestructura, sin complejidad innecesaria.

---

## 1. Stack y estructura

- Framework: **.NET LTS** con **ASP.NET Core MVC**.
- ORM: **Entity Framework Core** con proveedor **Npgsql (PostgreSQL)**.
- Front-end: **Bootstrap estándar** (sin plantillas externas), usando Razor Views.
- Arquitectura simple:
  - MVC clásico
  - `DbContext` para acceso a datos
  - Evitar sobreingeniería.
- Al crear el proyecto hazlo en la raíz de la carpeta en la que te encuentres.
- El proyecto y sus carpetas las queremos en la raiz donde estamos situados.

---

## 2. Base de datos y migraciones

- Base de datos: **PostgreSQL**.
- Sistema de **migraciones automáticas**:

### Development
- Aplicar `Database.Migrate()` al arrancar.
- Permitir seed mínimo de datos.

### Producción / Explotación
- Las migraciones **Se aplican por defecto**.
- Se controlan mediante el flag:

```json
"Database": {
  "ApplyMigrationsOnStartup": true
}
```

---

## 3. Configuración (appsettings)

### 3.1 Archivos
- `appsettings.json`
- `appsettings.Development.json`

### 3.2 Configuración de base de datos (por atributos)

```json
"Database": {
  "Host": "localhost",
  "Port": 5432,
  "Name": "master_db",
  "User": "postgres",
  "Password": "postgres",
  "SslMode": "Disable",
  "TrustServerCertificate": true,
  "ApplyMigrationsOnStartup": true,
  "EnableSeed": true
}
```

- Construir la connection string en runtime con `NpgsqlConnectionStringBuilder`.

---

## 4. Configuración para APIs externas

```json
"ExternalApis": {
  "ExampleApi": {
    "BaseUrl": "https://api.example.com",
    "ApiKey": "changeme",
    "TimeoutSeconds": 30,
    "RetryCount": 3
  }
}
```

---

## 5. Proyecto inicial funcional (mínimo)

### Tabla dummy

Entidad `DummyRecord`:
- `Id` (int, PK)
- `Name` (string)
- `CreatedAt` (DateTime)

---

## 6. Gestión de usuarios con ASP.NET Identity

### 6.1 Requisitos mínimos
- Implementar **ASP.NET Core Identity** para gestión de usuarios.
- Sistema de autenticación basado en cookies.
- Validación de formularios estándar de ASP.NET.

### 6.2 Funcionalidades básicas
- **Registro de usuarios** (Register).
- **Login/Logout**.
- **Validación de contraseñas** (requisitos configurables).
- Tablas de Identity en PostgreSQL (AspNetUsers, AspNetRoles, etc.).

### 6.3 Configuración de Identity

```json
"Identity": {
  "Password": {
    "RequireDigit": true,
    "RequiredLength": 6,
    "RequireNonAlphanumeric": false,
    "RequireUppercase": false,
    "RequireLowercase": false
  },
  "Lockout": {
    "DefaultLockoutTimeSpan": "00:05:00",
    "MaxFailedAccessAttempts": 5
  }
}
```

### 6.4 Usuario extendido (opcional)

Si necesitas campos adicionales, extender `IdentityUser`:
- `DisplayName` (string)
- `CreatedAt` (DateTime)

---

## 7. Entregables

- Código completo y compilable.
- Comandos de creación, migración y ejecución.

---

## 8. Decisiones asumidas

Indicar:
- Nombre del proyecto
- Versión de .NET
- Puertos
- Base de datos

---

## 9. Criterios clave

- Simplicidad
- Claridad
- Robustez
