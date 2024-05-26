# servicepoint-tanks

<!-- TODO: image -->

### Backend

<!-- TODO: image -->

- Uses the C# bindings from the [servicepoint library](https://github.com/cccb/servicepoint/) for communication with the display.
- Stack: .NET / C# / ASP.NET / AOT-compiled
- Both traditional JSON over HTTP APIs and real-time WebSocket communication
- runs all game logic
- sends image and text to the service point display
- sends image to clients
- The game has a dynamic update rate. Hundreds of updates per second on a laptop are expected.
- One frame is ~7KB, not including the text and player specific data
- maps can be loaded from png files containing black and white pixels or simple text files
- some values (like tank speed) can be configured but are fixed at run time
- By default, the backend also hosts the frontend

### Frontend

<!-- TODO: image -->

- Stack: React / Vite / TypeScript / plain CSS
- There is no server component dedicated to the frontend, everything is a single page after build
- Shows map rendered on server by setting canvas image data
- Sends user input to server
- real time communication via WebSockets, HTTP for the REST

### Binary formats

#### Controls WebSocket

- Client sends 2 byte messages.
  - on or off: `0x01` or `0x02`
  - input: Forward=`0x01`, Backward=`0x02`, Left=`0x03`, Right=`0x04`, Shoot=`0x05`
- The server never sends any messages.

### Observer screen WebSocket

- same image for all clients
- server sends same format as for the service point display
- client responds with empty message to request the next frame

### Player screen WebSocket

- image is rendered per player
- server sends same message as the observer WebSocket, but includes an additional 4 bits per set bit in the observer payload
  - first bit: belongs to current player
  - second bit: (reserved)
  - third and fourth bit: type of something
    - 00: wall
    - 01: tank
    - 10: bullet
    - 11: (reserved)
- client responds with empty message to request the next frame


