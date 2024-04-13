import {useEffect, useState} from 'react';
import './PlayerInfo.css'
import {PlayerResponse, getPlayer} from './serverCalls';
import {Guid} from "./Guid.ts";
import Column from "./components/Column.tsx";
import Row from "./components/Row.tsx";
import Button from "./components/Button.tsx";

export default function PlayerInfo({playerId, logout, reset}: {
    playerId: Guid,
    logout: () => void,
    reset: () => void
}) {
    const [player, setPlayer] = useState<PlayerResponse | null>();

    useEffect(() => {
        const refresh = () => {
            getPlayer(playerId).then(value => {
                if (value)
                    setPlayer(value);
                else
                    logout();
            });
        };

        const timer = setInterval(refresh, 5000);
        return () => clearInterval(timer);
    }, [playerId]);

    return <Column className='PlayerInfo'>
        <Row>
            <h3 className='grow'>
                {player ? `Playing as "${player?.name}"` : 'loading...'}
            </h3>
            <Button className='PlayerInfo-Reset' onClick={() => reset()} text='x'/>
        </Row>
        <Row>
            <p className='Elems'> kills: </p>
            <p className='Elems'>{player?.scores.kills}</p>
        </Row>
        <Row>
            <p className='Elems'>deaths: </p>
            <p className='Elems'>{player?.scores.deaths}</p>
        </Row>
    </Column>;
}
