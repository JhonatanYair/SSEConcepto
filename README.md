1. Ejecuta el comando docker compose up -d. 

Se creara 4 contenedores

- rabbitmq
- clientesse
- producermessage
- serversignal

2. Ahora abre en un pestaña la siguente url: http://localhost:8082/
Este es el cliente del servicio sse, navegar la carpeta ClienteSSE para ver todos los clientes de ejemplo  que se han configurado.

El cliente index.html se suscribe a un exchange de CompanyService  y recibe todos los mensajes de ese exchange
El cliente CompanyUser.html se suscribe al  exchange de CompanyService y solo recibe los mensajes que son destinados para el user.

Objeto json que llega:

{
    "Event_id":15,
    "tracking":"65923a86-5d05-455f-9bb9-5eaa2ca804ba",
    "From":"ProduccerMessage",
    "To":null,
    "Exchange":"CompanyService"
    "EventType":"company_new",
    "DateCreate":"2023-09-26T06:58:46.6227291-05:00",
    "Data":{
        "company_id":"de93bd20-6c3f-4be1-bc3e-72ec5222091a"
        }
}

3. Abre en una pestaña la siguente url: http://localhost:8080/swagger/index.html
Encontraras algunos endpoints de prueba para generar los mensajes en la colas, algun cliente esta conectado al servicio sse, le debera llegar dichos mensajes.

Documentacion de signalR:

https://learn.microsoft.com/es-es/aspnet/core/signalr/javascript-client?view=aspnetcore-7.0&tabs=visual-studio
https://learn.microsoft.com/es-es/aspnet/core/signalr/dotnet-client?view=aspnetcore-7.0&tabs=visual-studio
https://learn.microsoft.com/es-es/aspnet/signalr/overview/getting-started/introduction-to-signalr