# Proyecto Estacionamiento (WebForms - .NET Framework 4.7.2)

Este es un sistema de gestión de estacionamiento desarrollado con ASP.NET WebForms utilizando .NET Framework 4.7.2. El proyecto utiliza múltiples dependencias NuGet que deben ser restauradas para compilar y ejecutar correctamente.

---

## ⚙️ Requisitos

- Visual Studio 2019 o superior
- .NET Framework 4.7.2
- NuGet CLI (opcional si usás la terminal)

---

## 📦 Restaurar paquetes NuGet

### Opción 1: Usar Visual Studio
1. Abrí la solución (`.sln`) en Visual Studio.
2. Hacé clic derecho sobre la solución.
3. Seleccioná **"Restaurar paquetes NuGet"**.
4. Compilá el proyecto normalmente.

### Opción 2: Usar terminal con NuGet CLI
1. Abrí una terminal en la carpeta raíz del proyecto.
2. Ejecutá el siguiente comando:

```bash
nuget restore
```

### Esto descargará los siguientes paquetes:

- AjaxControlToolkit v20.1.0
- Antlr v3.5.0.2
- bootstrap v5.3.7
- EntityFramework v6.5.1
- jQuery v3.7.1
- Microsoft.AspNet.FriendlyUrls v1.0.2
- Microsoft.AspNet.FriendlyUrls.Core v1.0.2
- Microsoft.AspNet.FriendlyUrls.Core.es v1.0.2
- Microsoft.AspNet.ScriptManager.MSAjax v5.0.0
- Microsoft.AspNet.ScriptManager.WebForms v5.0.0
- Microsoft.AspNet.Web.Optimization v1.1.3
- Microsoft.AspNet.Web.Optimization.es v1.1.3
- Microsoft.AspNet.Web.Optimization.WebForms v1.1.3
- Microsoft.CodeDom.Providers.DotNetCompilerPlatform v4.1.0
- Microsoft.Web.Infrastructure v2.0.0
- Modernizr v2.8.3
- Newtonsoft.Json v13.0.3
- WebGrease v1.6.0  

### 🛠️ Instalación manual (opcional)
```bash
Install-Package AjaxControlToolkit -Version 20.1.0
Install-Package Antlr -Version 3.5.0.2
Install-Package bootstrap -Version 5.3.7
Install-Package EntityFramework -Version 6.5.1
Install-Package jQuery -Version 3.7.1
Install-Package Microsoft.AspNet.FriendlyUrls -Version 1.0.2
Install-Package Microsoft.AspNet.FriendlyUrls.Core -Version 1.0.2
Install-Package Microsoft.AspNet.FriendlyUrls.Core.es -Version 1.0.2
Install-Package Microsoft.AspNet.ScriptManager.MSAjax -Version 5.0.0
Install-Package Microsoft.AspNet.ScriptManager.WebForms -Version 5.0.0
Install-Package Microsoft.AspNet.Web.Optimization -Version 1.1.3
Install-Package Microsoft.AspNet.Web.Optimization.es -Version 1.1.3
Install-Package Microsoft.AspNet.Web.Optimization.WebForms -Version 1.1.3
Install-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -Version 4.1.0
Install-Package Microsoft.Web.Infrastructure -Version 2.0.0
Install-Package Modernizr -Version 2.8.3
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package WebGrease -Version 1.6.0
```
