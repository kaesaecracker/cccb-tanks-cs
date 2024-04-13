import {useEffect, useState} from 'react';
import './PlayerInfo.css'
import {PlayerResponse, getPlayer} from './serverCalls';
import {Guid} from "./Guid.ts";
import Column from "./components/Column.tsx";

export default function PlayerInfo({playerId, logout}: {
    playerId: Guid,
    logout: () => void
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
        <h3>
            {player ? `Playing as ${player?.name}` : 'loading...'}
        </h3>
        <table>
            <tbody>
            <tr>
                <td>kills:</td>
                <td>{player?.scores.kills}</td>
            </tr>
            <tr>
                <td>deaths:</td>
                <td>{player?.scores.deaths}</td>
            </tr>
            </tbody>
        </table>
    </Column>;
}
