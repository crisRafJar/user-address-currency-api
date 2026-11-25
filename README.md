# User, Address & Currency API

API REST en .NET (Minimal API) para gestionar usuarios, direcciones y conversión de divisas.

## Versión de .NET

.NET 8.0. Se necesita el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) o superior.

## Cómo correr el proyecto

```powershell
git clone https://github.com/crisRafJar/user-address-currency-api.git
cd user-address-currency-api
dotnet restore
dotnet run
```

La API escucha en `http://localhost:5230` y Swagger está en `http://localhost:5230/swagger`.

## Base de datos

La cadena de conexión está en `appsettings.json`. Al arrancar, la aplicación crea el archivo SQLite
y aplica las migraciones pendientes.


## API Key

Todos los endpoints requieren el header `X-API-KEY`. Si falta o es incorrecto, responde `401 Unauthorized`.
La clave está en `appsettings.json` (`ApiKey`).

- API Key de prueba: `test-api-key-123`

```powershell
curl.exe -H "X-API-KEY: test-api-key-123" http://localhost:5230/users
```

Swagger se puede abrir sin clave. Para probar los endpoints desde ahí, usa el botón **Authorize**.

## Carga masiva

`POST /users/bulk` procesa los usuarios en paralelo con `Task.WhenAll`, creando un `AppDbContext`
por operación con `IDbContextFactory<AppDbContext>`. La respuesta indica cuántos se crearon y cuáles
fallaron, con el motivo.

Archivo de prueba: [`sample-data/bulk_users_test.json`](sample-data/bulk_users_test.json)

```powershell
curl.exe -X POST http://localhost:5230/users/bulk -H "X-API-KEY: test-api-key-123" -H "Content-Type: application/json" --data-binary "@sample-data/bulk_users_test.json"
```

## Qué quedó implementado

Todo lo pedido: CRUD de usuarios y direcciones, módulo de divisas (listar, crear y convertir),
API Key, EF Core con SQLite, FluentValidation, CQRS, paralelismo (carga masiva y conversión),
`IDbContextFactory`, carga masiva y Swagger.

Notas:

- El `password` es opcional al crear un usuario. Si se envía, se guarda con hash BCrypt y no se devuelve.
- La base de datos arranca vacía: hay que crear las monedas con `POST /currencies` antes de convertir.

## Extras

- Campos de auditoria para las tres tablas.
    `createdAt`
    `updatedAt`
- Obtener por id, modificar y eliminar una divisa.
