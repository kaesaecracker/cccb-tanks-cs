import {makeApiUrl, Scores, useMyWebSocket} from './serverCalls';
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

type TankInfo = {
    readonly magazine: string;
    readonly position: { x: number; y: number };
    readonly orientation: number;
    readonly moving: boolean;
}


type PlayerInfoMessage = {
    readonly name: string;
    readonly scores: Scores;
    readonly controls: string;
    readonly tank?: TankInfo;
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
        onMessage: () => setShouldSendMessage(true)
    });

    useEffect(() => {
        if (!shouldSendMessage || readyState !== ReadyState.OPEN)
            return;
        setShouldSendMessage(false);
        sendMessage('');
    }, [readyState, shouldSendMessage]);

    if (!lastJsonMessage || readyState !== ReadyState.OPEN)
        return <></>;

    return <Column className="PlayerInfo">
        <h3>
            Playing as {lastJsonMessage.name}
        </h3>
        <table>
            <tbody>
            <ScoreRow name="magazine" value={lastJsonMessage.tank?.magazine}/>
            <ScoreRow name="controls" value={lastJsonMessage.controls}/>
            <ScoreRow name="position" value={lastJsonMessage.tank?.position}/>
            <ScoreRow name="orientation" value={lastJsonMessage.tank?.orientation}/>
            <ScoreRow name="moving" value={lastJsonMessage.tank?.moving}/>

            <ScoreRow name="kills" value={lastJsonMessage.scores.kills}/>
            <ScoreRow name="deaths" value={lastJsonMessage.scores.deaths}/>

            <ScoreRow name="walls destroyed" value={lastJsonMessage.scores.wallsDestroyed}/>
            <ScoreRow name="bullets fired" value={lastJsonMessage.scores.shotsFired}/>
            <ScoreRow name="power ups collected" value={lastJsonMessage.scores.powerUpsCollected}/>
            <ScoreRow name="pixels moved" value={lastJsonMessage.scores.pixelsMoved}/>

            <ScoreRow name="score" value={lastJsonMessage.scores.overallScore}/>
            </tbody>
        </table>
    </Column>;
}
