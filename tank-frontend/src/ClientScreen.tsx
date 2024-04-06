import useWebSocket, {ReadyState} from 'react-use-websocket';
import {useEffect, useRef} from 'react';
import './ClientScreen.css';

const pixelsPerRow = 352;
const pixelsPerCol = 160;

function getIndexes(bitIndex: number) {
    return {
        byteIndex: 10 + Math.floor(bitIndex / 8),
        bitInByteIndex: 7 - bitIndex % 8
    };
}

function drawPixelsToCanvas(pixels: Uint8Array, canvas: HTMLCanvasElement) {
    const drawContext = canvas.getContext('2d');
    if (!drawContext)
        throw new Error('could not get draw context');

    const imageData = drawContext.getImageData(0, 0, canvas.width, canvas.height, {colorSpace: 'srgb'});
    const data = imageData.data;

    console.log('draw', {width: canvas.width, height: canvas.height, dataLength: data.byteLength});

    for (let y = 0; y < canvas.height; y++) {
        const rowStartPixelIndex = y * pixelsPerRow;
        for (let x = 0; x < canvas.width; x++) {
            const pixelIndex = rowStartPixelIndex + x;
            const {byteIndex, bitInByteIndex} = getIndexes(pixelIndex);
            const byte = pixels[byteIndex];
            const mask = (1 << bitInByteIndex);
            const bitCheck = byte & mask;
            const isOn = bitCheck !== 0;

            const dataIndex = pixelIndex * 4;
            if (isOn) {
                data[dataIndex] = 0; // r
                data[dataIndex + 1] = 180; // g
                data[dataIndex + 2] = 0; // b
                data[dataIndex + 3] = 255; // a
            } else {
                data[dataIndex] = 0; // r
                data[dataIndex + 1] = 0; // g
                data[dataIndex + 2] = 0; // b
                data[dataIndex + 3] = 255; // a
            }
        }
    }

    drawContext.putImageData(imageData, 0, 0);
}

export default function ClientScreen({}: {}) {
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const {
        readyState,
        lastMessage,
        sendMessage,
        getWebSocket
    } = useWebSocket(import.meta.env.VITE_TANK_SCREEN_URL, {
        shouldReconnect: () => true,
    });

    const socket = getWebSocket();
    if (socket)
        (socket as WebSocket).binaryType = 'arraybuffer';

    useEffect(() => {
        if (lastMessage === null)
            return;
        if (canvasRef.current === null)
            throw new Error('canvas null');

        drawPixelsToCanvas(new Uint8Array(lastMessage.data), canvasRef.current);
        sendMessage('');
    }, [lastMessage, canvasRef.current]);

    const connectionStatus = {
        [ReadyState.CONNECTING]: 'Connecting',
        [ReadyState.OPEN]: 'Open',
        [ReadyState.CLOSING]: 'Closing',
        [ReadyState.CLOSED]: 'Closed',
        [ReadyState.UNINSTANTIATED]: 'Uninstantiated',
    }[readyState];

    return <>
        <span>The WebSocket is currently {connectionStatus}</span>
        <canvas ref={canvasRef} id="screen" width={pixelsPerRow} height={pixelsPerCol}/>
    </>;
}
