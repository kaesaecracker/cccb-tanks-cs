import useWebSocket from 'react-use-websocket';
import {useEffect, useRef} from 'react';
import './ClientScreen.css';

const pixelsPerRow = 352;
const pixelsPerCol = 160;

function getIndexes(bitIndex: number) {
    return {
        byteIndex: Math.floor(bitIndex / 8),
        bitInByteIndex: 7 - bitIndex % 8
    };
}

// @ts-ignore
const rootElement = document.querySelector(':root')!;

function normalizeColor(context: CanvasRenderingContext2D, color: string) {
    context.fillStyle = color;
    context.fillRect(0, 0, 1, 1);
    return context.getImageData(0, 0, 1, 1).data;
}

function drawPixelsToCanvas(pixels: Uint8Array, canvas: HTMLCanvasElement) {
    const drawContext = canvas.getContext('2d');
    if (!drawContext)
        throw new Error('could not get draw context');

    const rootStyle = getComputedStyle(rootElement);
    const colorPrimary = normalizeColor(drawContext, rootStyle.getPropertyValue('--color-primary'));
    const colorBackground = normalizeColor(drawContext, rootStyle.getPropertyValue('--color-background'));

    const imageData = drawContext.getImageData(0, 0, canvas.width, canvas.height, {colorSpace: 'srgb'});
    const data = imageData.data;

    for (let y = 0; y < canvas.height; y++) {
        const rowStartPixelIndex = y * pixelsPerRow;
        for (let x = 0; x < canvas.width; x++) {
            const pixelIndex = rowStartPixelIndex + x;
            const {byteIndex, bitInByteIndex} = getIndexes(pixelIndex);
            const mask = (1 << bitInByteIndex);
            const isOn = (pixels[byteIndex] & mask) !== 0;
            const color = isOn ? colorPrimary : colorBackground;

            for (let colorChannel of [0, 1, 2, 3])
                data[pixelIndex * 4 + colorChannel] = color[colorChannel];
        }
    }

    drawContext.putImageData(imageData, 0, 0);
}

export default function ClientScreen({logout}: { logout: () => void }) {
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const {
        lastMessage,
        sendMessage,
        getWebSocket
    } = useWebSocket(import.meta.env.VITE_TANK_SCREEN_URL, {
        onError: logout
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

    return <canvas ref={canvasRef} id="screen" width={pixelsPerRow} height={pixelsPerCol}/>;
}
