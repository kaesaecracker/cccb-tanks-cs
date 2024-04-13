import {useEffect, useState} from 'react';
import './JoinForm.css';
import {NameId, PlayerResponse, postPlayer} from './serverCalls';
import {Guid} from './Guid.ts';

export default function JoinForm({setNameId, clientId}: {
    setNameId: (mutator: (oldState: NameId) => NameId) => void,
    clientId: Guid
}) {
    const [name, setName] = useState('');
    const [clicked, setClicked] = useState(false);
    const [data, setData] = useState<PlayerResponse | null>(null);

    useEffect(() => {
        if (!clicked || data)
            return;

        try {
            postPlayer({name, id: clientId})
                .then(value => {
                    if (value)
                        setNameId(prev => ({...prev, ...value}));
                    else
                        setClicked(false);
                });
        } catch (e) {
            console.log(e);
            alert(e);
        }
    }, [clicked, setData, data, clientId, setClicked, setNameId]);

    const disableButtons = clicked || name.trim() === '';
    return <div className="TankWelcome">
        <h1 className="JoinElems" style={{'color': 'white'}}>
            Tanks
        </h1>
        <p className="JoinElems" style={{'color': 'white'}}> Welcome and have fun!</p>
        <div className="JoinForm">
            <p className="JoinElems" style={{'color': 'white'}}>
                Enter your name to join the game!
            </p>
            <input className="JoinElems"
                   type="text"
                   value={name}
                   placeholder="player name"
                   onChange={e => setName(e.target.value)}
            />
            <button className="JoinElems"
                    onClick={() => setClicked(true)}
                    disabled={disableButtons}
            >
                join
            </button>
        </div>
    </div>;
}
