# CCCB-Tanks

<!-- TODO: image -->

## Service point display

<!-- TODO: image -->

In CCCB, there is a big pixel matrix hanging on the wall. It is called "Airport Display" or "Service Point Display".

- Resolution: 352x160=56,320 pixels
- Pixels are grouped into 44x20=880 tiles (8x8=64 pixels each)
- Smallest addressable unit: row of pixels inside of a tile (8 pixels = 1 byte)
- The brightness can only be set per tile
- Screen content can be changed using a simple UDP protocol
- Between each row of tiles, there is a gap of around 4 pixels size. This gap changes the aspect ratio of the display.

### Binary format

A UDP package sent to the display has a header size of 10 bytes.
Each header value has a size of two bytes (unsigned 16 bit integer).
Depending on the command, there can be a payload following the header.

The commands are implemented in DisplayCommands.

To change screen contents, these commands are the most relevant:
1. Clear screen
    - command: `0x0002`
    - (rest does not matter)
2. Send CP437 data: render specified text into rectangular region
    - command: `0x0003`
    - top left tile x
    - top left tile y
    - width in tiles
    - height in tiles
    - payload: (width in tiles * height in tiles) bytes
        - 1 byte = 1 character
        - each character is rendered into one tile (mono-spaced)
        - characters are encoded using code page 437
3. Send bitmap window: set pixel states for a rectangular region
   - command: `0x0013`
   - top left tile x
   - top left _pixel_ y
   - width in tiles
   - height in _pixels_
   - payload: (width in tiles * height in pixels) bytes
     - network byte order
     - 1 bit = 1 pixel

There are other commands implemented as well, e.g. for changing the brightness.

## Tanks game

- By default, the backend also hosts the frontend

### Backend

<!-- TODO: image -->

- Stack: .NET / C# / ASP.NET / AOT-compiled
- Both traditional JSON over HTTP APIs and real-time WebSocket communication
- runs all game logic
- sends image and text to the service point display
- sends image to clients
- currently, the game has a fixed tick and frame rate of 25/s
- One frame is ~7KB, not including the text
- maps can be loaded from png files containing black and white pixels or simple text files
- some values (like tank speed) can be configured but are fixed at run time

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

## Backlog: Bugs, Wishes, Ideas
- Generalize drawing of entities as there are multiple classes with pretty much the same code
- Generalize hit box collision
