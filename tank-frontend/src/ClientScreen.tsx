import {useEffect, useState} from 'react';
import {makeApiUrl, useMyWebSocket} from './serverCalls.tsx';
import {ReadyState} from 'react-use-websocket';
import PixelGridCanvas from './components/PixelGridCanvas.tsx';


export default function ClientScreen({player}: {
    player: string | null
}) {
    const [shouldSendMessage, setShouldSendMessage] = useState(false);

    const url = makeApiUrl('/screen', 'ws');
    if (player && player !== '')
        url.searchParams.set('playerName', player);

    const {
        lastMessage,
        sendMessage,
        getWebSocket,
        readyState
    } = useMyWebSocket(url.toString(), {
        onOpen: _ => setShouldSendMessage(true),
        onMessage: _ => setShouldSendMessage(true)
    });

    const socket = getWebSocket();
    if (socket)
        (socket as WebSocket).binaryType = 'arraybuffer';

    useEffect(() => {
        if (!shouldSendMessage || readyState !== ReadyState.OPEN)
            return;
        setShouldSendMessage(false);
        sendMessage('');
    }, [readyState, shouldSendMessage]);

    if (!lastMessage)
        return <></>;

    const pixels = new Uint8ClampedArray(lastMessage.data);
    return <PixelGridCanvas pixels={pixels}/>;
}
