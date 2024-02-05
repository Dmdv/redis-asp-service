# Redis Background service

This service was born to provide a simple way to run a Redis server in the background. \
I found out that there are many ways to run a Redis server in the background \

I found out many challenges when working with Redis in ASP.NET Core 8.0. \
- Background service blocks the whole service in case of synchronous CONNECT calls.
- Redis may be disconnected during the service lifetime.
- Redis may be disconnected during publishing.

In another projects I found following approaches (not the best ones but for the sake of the example, I will put it here):
- Run Connect in a separate thread in the thread pool.
        - This will end up with connection loss without main thread knowing about it.
        - Not possible to make the parent thread let know about the connection established.
- Wrapping connect into Lazy<> pattern.
        - While this is lazy, at the same time this is not asynchronous. You need to develop LazyAsync<> pattern.
- Running Redis connection in the background service.
        - This will block the whole service.