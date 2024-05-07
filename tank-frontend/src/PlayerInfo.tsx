import {makeApiUrl, PlayerInfoMessage, useMyWebSocket} from './serverCalls';
import Column from './components/Column.tsx';
import {ReadyState} from 'react-use-websocket';
import {useEffect, useState} from 'react';

function ScoreRow({name, value}: {
    name: string;
    value?: string | any;
}) {
    let valueStr;
    if (value === undefined)
        valueStr = '?';
    else if (typeof value === 'string' || value instanceof String)
        valueStr = value;
    else
        valueStr = JSON.stringify(value);

    return <tr>
        <td>{name}</td>
        <td>{valueStr}</td>
    </tr>;
}

export default function PlayerInfo({player}: { player: string }) {
    const [shouldSendMessage, setShouldSendMessage] = useState(false);

    const url = makeApiUrl('/player');
    url.searchParams.set('name', player);

    const {
        lastJsonMessage,
        readyState,
        sendMessage
    } = useMyWebSocket<PlayerInfoMessage>(url.toString(), {
        onMessage: () => setShouldSendMessage(true),
        onOpen: _ => setShouldSendMessage(true)
    });

    useEffect(() => {
        if (!shouldSendMessage || readyState !== ReadyState.OPEN)
            return;
        setShouldSendMessage(false);
        sendMessage('');
    }, [readyState, shouldSendMessage]);

    if (!lastJsonMessage || readyState !== ReadyState.OPEN)
        return <></>;

    let position = '';
    if (lastJsonMessage.tank)
        position = `(${Math.round(lastJsonMessage.tank.position.x)}|${Math.round(lastJsonMessage.tank.position.y)})`;

    return <Column className="PlayerInfo">
        <h3>
            Playing as {lastJsonMessage.player.name}
        </h3>
        <table>
            <tbody>
            <ScoreRow name="magazine" value={lastJsonMessage.tank?.magazine}/>
            <ScoreRow name="controls" value={lastJsonMessage.controls}/>
            <ScoreRow name="position" value={position}/>
            <ScoreRow name="orientation" value={lastJsonMessage.tank?.orientation}/>
            <ScoreRow name="bullet speed" value={lastJsonMessage.tank?.bulletStats.speed}/>
            <ScoreRow name="bullet acceleration" value={lastJsonMessage.tank?.bulletStats.acceleration}/>
            <ScoreRow name="smart bullets" value={lastJsonMessage.tank?.bulletStats.smart}/>
            <ScoreRow name="explosive bullets" value={lastJsonMessage.tank?.bulletStats.explosive}/>

            <ScoreRow name="kills" value={lastJsonMessage.player.scores.kills}/>
            <ScoreRow name="deaths" value={lastJsonMessage.player.scores.deaths}/>
            <ScoreRow name="walls destroyed" value={lastJsonMessage.player.scores.wallsDestroyed}/>
            <ScoreRow name="bullets fired" value={lastJsonMessage.player.scores.shotsFired}/>
            <ScoreRow name="power ups collected" value={lastJsonMessage.player.scores.powerUpsCollected}/>
            <ScoreRow name="pixels moved" value={lastJsonMessage.player.scores.pixelsMoved}/>
            <ScoreRow name="score" value={lastJsonMessage.player.scores.overallScore}/>

            <ScoreRow name="moving" value={lastJsonMessage.tank?.moving}/>
            <ScoreRow name="connections" value={lastJsonMessage.player.openConnections}/>
            </tbody>
        </table>
    </Column>;
}
