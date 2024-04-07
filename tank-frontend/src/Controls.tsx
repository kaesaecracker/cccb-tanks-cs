import './Controls.css';
import useWebSocket from 'react-use-websocket';
import {useEffect} from 'react';

export default function Controls({playerId}: {
    playerId: string
}) {
    const url = new URL(import.meta.env.VITE_TANK_CONTROLS_URL);
    url.searchParams.set('playerId', playerId);
    const {
        sendMessage,
        getWebSocket
    } = useWebSocket(url.toString(), {
        shouldReconnect: () => true,
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

    return <div className="controls">
        <div className="control">
            <div className="row">
                <kbd className="up">▲</kbd>
            </div>
            <div className="row">
                <kbd>◀</kbd>
                <kbd>▼</kbd>
                <kbd>▶</kbd>
            </div>
            <h3>Move</h3>
        </div>
        <div className="control">
            <kbd className="space">Space</kbd>
            <h3>Fire</h3>
        </div>
    </div>;
}
