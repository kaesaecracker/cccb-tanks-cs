import { useEffect, useState } from 'react';
import './PlayerInfo.css'
import { PlayerResponse, getPlayer } from './serverCalls';

export default function PlayerInfo({ playerId }: { playerId: string }) {
    const [player, setPlayer] = useState<PlayerResponse | null>();

    const refresh = () => {
        getPlayer(playerId).then(setPlayer);
    };

    useEffect(() => {
        const timer = setInterval(refresh, 5000);
        return () => clearInterval(timer);
    });

    return <div className='TankWelcome'>
        <h1 className='Elems' style={{ "color": "white" }}>
            Tanks
        </h1>
        <div className="ScoreForm">
            <div className='ElemGroup'>
                <p className='Elems' style={{ "color": "white" }}>
                    name:
                </p>
                <p className='Elems' style={{ "color": "white" }}>
                    {player?.name}
                </p>
            </div>
            <div className='ElemGroup'>
                <p className='Elems' style={{ "color": "white" }}>
                    kills:
                </p>
                <p className='Elems' style={{ "color": "white" }}>
                    {player?.kills}
                </p>
            </div>
            <div className='ElemGroup'>
                <p className='Elems' style={{ "color": "white" }}>
                    deaths:
                </p>
                <p className='Elems' style={{ "color": "white" }}>
                    {player?.deaths}
                </p>
            </div>
        </div>
    </div >;
}
