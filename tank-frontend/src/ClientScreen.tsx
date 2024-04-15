import useWebSocket from 'react-use-websocket';
import {useEffect, useRef} from 'react';
import './ClientScreen.css';
import {hslToString, Theme} from "./theme.ts";
import {Guid} from "./Guid.ts";

const pixelsPerRow = 352;
const pixelsPerCol = 160;
const observerMessageSize = pixelsPerCol * pixelsPerRow / 8;

const isPlayerMask = 1;

function getIndexes(bitIndex: number) {
    return {
        byteIndex: Math.floor(bitIndex / 8),
        bitInByteIndex: 7 - bitIndex % 8
    };
}

function normalizeColor(context: CanvasRenderingContext2D, color: string) {
    context.fillStyle = color;
    context.fillRect(0, 0, 1, 1);
    return context.getImageData(0, 0, 1, 1).data;
}

function drawPixelsToCanvas({context, width, height, pixels, additional, foreground, background, playerColor}: {
    context: CanvasRenderingContext2D,
    width: number,
    height: number,
    pixels: Uint8ClampedArray,
    additional: Uint8ClampedArray | null,
    background: Uint8ClampedArray,
    foreground: Uint8ClampedArray,
    playerColor: Uint8ClampedArray
}) {
    let additionalDataIndex = 0;
    let additionalDataByte: number | null = null;
    const nextPixelColor = (isOn: boolean) => {
        if (!isOn)
            return background;
        if (!additional)
            return foreground;

        let info;
        if (additionalDataByte === null) {
            additionalDataByte = additional[additionalDataIndex];
            additionalDataIndex++;
            info = additionalDataByte;
        } else {
            info = additionalDataByte >> 4;
            additionalDataByte = null;
        }

        if ((info & isPlayerMask) != 0) {
            return playerColor;
        }

        return foreground;
    }

    const imageData = context.getImageData(0, 0, width, height, {colorSpace: 'srgb'});
    const data = imageData.data;

    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            const pixelIndex = y * pixelsPerRow + x;
            const {byteIndex, bitInByteIndex} = getIndexes(pixelIndex);
            const isOn = (pixels[byteIndex] & (1 << bitInByteIndex)) !== 0;
            const color = nextPixelColor(isOn);

            for (let colorChannel of [0, 1, 2, 3])
                data[pixelIndex * 4 + colorChannel] = color[colorChannel];
        }
    }

    context.putImageData(imageData, 0, 0);
}

export default function ClientScreen({logout, theme, playerId}: {
    logout: () => void,
    theme: Theme,
    playerId?: Guid
}) {
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const url = new URL('/screen', import.meta.env.VITE_TANK_WS);
    if (playerId)
        url.searchParams.set('player', playerId);

    const {
        lastMessage,
        sendMessage,
        getWebSocket
    } = useWebSocket(url.toString(), {
        onError: logout,
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

        const canvas = canvasRef.current;
        const drawContext = canvas.getContext('2d');
        if (!drawContext)
            throw new Error('could not get draw context');

        const colorBackground = normalizeColor(drawContext, hslToString(theme.background));
        const colorPrimary = normalizeColor(drawContext, hslToString(theme.primary));
        const colorSecondary = normalizeColor(drawContext, hslToString(theme.secondary));

        let pixels = new Uint8ClampedArray(lastMessage.data);
        let additionalData: Uint8ClampedArray | null = null;
        if (pixels.length > observerMessageSize) {
            additionalData = pixels.slice(observerMessageSize);
            pixels = pixels.slice(0, observerMessageSize);
        }

        console.log('', {pixelLength: pixels.length, additionalLength: additionalData?.length});

        drawPixelsToCanvas({
            context: drawContext,
            width: canvas.width,
            height: canvas.height,
            pixels,
            additional: additionalData,
            background: colorBackground,
            foreground: colorPrimary,
            playerColor: colorSecondary
        });
        sendMessage('');
    }, [lastMessage, canvasRef.current, theme]);

    return <canvas ref={canvasRef} id="screen" width={pixelsPerRow} height={pixelsPerCol}/>;
}
