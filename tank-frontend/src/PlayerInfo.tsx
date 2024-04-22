import {makeApiUrl, Scores} from './serverCalls';
import {Guid} from './Guid.ts';
import Column from './components/Column.tsx';
import useWebSocket, {ReadyState} from 'react-use-websocket';
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

type Controls = {
    readonly forward: boolean;
    readonly backward: boolean;
    readonly turnLeft: boolean;
    readonly turnRight: boolean;
    readonly shoot: boolean;
}

type TankInfo = {
    readonly explosiveBullets: number;
    readonly position: { x: number; y: number };
    readonly orientation: number;
    readonly moving: boolean;
}


type PlayerInfoMessage = {
    readonly name: string;
    readonly scores: Scores;
    readonly controls: Controls;
    readonly tank?: TankInfo;
}

function controlsString(controls: Controls) {
    let str = '';
    if (controls.forward)
        str += '▲';
    if (controls.backward)
        str += '▼';
    if (controls.turnLeft)
        str += '◄';
    if (controls.turnRight)
        str += '►';
    if (controls.shoot)
        str += '•';
    return str;
}

export default function PlayerInfo({playerId}: { playerId: Guid }) {
    const [shouldSendMessage, setShouldSendMessage] = useState(false);

    const url = makeApiUrl('/player');
    url.searchParams.set('id', playerId);

    const {lastJsonMessage, readyState, sendMessage} = useWebSocket<PlayerInfoMessage>(url.toString(), {
        onMessage: () => setShouldSendMessage(true),
        shouldReconnect: () => true,
    });

    useEffect(() => {
        if (!shouldSendMessage || readyState !== ReadyState.OPEN)
            return;
        setShouldSendMessage(false);
        sendMessage('');
    }, [readyState, shouldSendMessage]);

    if (!lastJsonMessage)
        return <></>;

    return <Column className="PlayerInfo">
        <h3>
            Playing as {lastJsonMessage.name}
        </h3>
        <table>
            <tbody>
            <ScoreRow name="controls" value={controlsString(lastJsonMessage.controls)}/>
            <ScoreRow name="kills" value={lastJsonMessage.scores.kills}/>
            <ScoreRow name="deaths" value={lastJsonMessage.scores.deaths}/>
            <ScoreRow name="walls" value={lastJsonMessage.scores.wallsDestroyed}/>
            <ScoreRow name="explosive bullets" value={lastJsonMessage.tank?.explosiveBullets}/>
            <ScoreRow name="position" value={lastJsonMessage.tank?.position}/>
            <ScoreRow name="orientation" value={lastJsonMessage.tank?.orientation}/>
            <ScoreRow name="moving" value={lastJsonMessage.tank?.moving}/>
            </tbody>
        </table>
    </Column>;
}
