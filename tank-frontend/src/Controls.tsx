import './Controls.css';
import useWebSocket from 'react-use-websocket';
import {useEffect} from 'react';
import Column from "./components/Column.tsx";

export default function Controls({playerId, logout}: {
    playerId: string,
    logout: () => void
}) {
    const url = new URL(import.meta.env.VITE_TANK_CONTROLS_URL);
    url.searchParams.set('playerId', playerId);
    const {
        sendMessage,
        getWebSocket,
    } = useWebSocket(url.toString(), {
        onError: logout
    });

    const socket = getWebSocket();
    if (socket)
        (socket as WebSocket).binaryType = 'arraybuffer';

    const keyEventListener = (type: string) => (event: KeyboardEvent) => {
        if (event.defaultPrevented)
            return;

        const typeCode = type === 'input-on' ? 0x01 : 0x02;

        const controls = {
            'ArrowLeft': 0x03,
            'ArrowUp': 0x01,
            'ArrowRight': 0x04,
            'ArrowDown': 0x02,
            'Space': 0x05,
            'KeyW': 0x01,
            'KeyA': 0x03,
            'KeyS': 0x02,
            'KeyD': 0x04,
        };

        // @ts-ignore
        const value = controls[event.code];
        if (!value)
            return;

        event.preventDefault();
        const message = new Uint8Array([typeCode, value]);
        sendMessage(message);
    };

    useEffect(() => {
        const up = keyEventListener('input-off');
        const down = keyEventListener('input-on');
        window.onkeyup = up;
        window.onkeydown = down;
        return () => {
            window.onkeydown = null;
            window.onkeyup = null;
        };
    }, [sendMessage]);

    return <Column className="Controls">
        <div className='flex-column Controls-Container'>
            <h3>Move</h3>
            <kbd>▲</kbd>
            <div className='flex-row Controls-Container'>
                <kbd>◄</kbd>
                <kbd>▼</kbd>
                <kbd>►</kbd>
            </div>
        </div>

        <div className='flex-column Controls-Container'>
            <h3>Fire</h3>
            <kbd className="space">Space</kbd>
        </div>
    </Column>;
}
